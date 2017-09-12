using System;
using System.Collections.Generic;
using ICD.Common.Attributes.Rpc;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Lighting;
using ICD.Connect.Lighting.EventArguments;
using ICD.Connect.Protocol.EventArguments;
using ICD.Connect.Protocol.Network.RemoteProcedure;
using ICD.Connect.Protocol.Network.Tcp;

namespace ICD.SimplSharp.BmsOS.Devices.Lighting
{
	/// <summary>
	/// Server for hosting ILightingProcessorDevices.
	/// </summary>
	[PublicAPI]
	public sealed class BmsLightingProcessorServer : IConsoleNode, IDisposable
	{
		public const string REGISTER_FEEDBACK_RPC = "RegisterFeedback";

		public const string SET_ROOM_PRESET_RPC = "SetRoomPreset";

		public const string SET_LOAD_LEVEL_RPC = "SetLoadLevel";
		public const string START_RAISING_LOAD_LEVEL_RPC = "StartRaisingLoadLevel";
		public const string START_LOWERING_LOAD_LEVEL_RPC = "StartLoweringLoadLevel";
		public const string STOP_RAMPING_LOAD_LEVEL_RPC = "StopRampingLoadLevel";

		public const string START_RAISING_SHADE_RPC = "StartRaisingShade";
		public const string START_LOWERING_SHADE_RPC = "StartLoweringShade";
		public const string STOP_MOVING_SHADE_RPC = "StopMovingShade";

		public const string START_RAISING_SHADE_GROUP_RPC = "StartRaisingShadeGroup";
		public const string START_LOWERING_SHADE_GROUP_RPC = "StartLoweringShadeGroup";
		public const string STOP_MOVING_SHADE_GROUP_RPC = "StopMovingShadeGroup";

		private readonly ServerSerialRpcController m_RpcController;

		private AsyncTcpServer m_Server;
		private ILightingProcessorDevice m_Processor;

		private readonly Dictionary<uint, int> m_ClientRoomMap;
		private readonly SafeCriticalSection m_ClientRoomSection;

		#region Properties

		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		public string ConsoleName { get { return "LightingServer"; } }

		/// <summary>
		/// Gets the help information for the node.
		/// </summary>
		public string ConsoleHelp { get { return "The BmsOS lighting server"; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		public BmsLightingProcessorServer()
		{
			m_ClientRoomMap = new Dictionary<uint, int>();
			m_ClientRoomSection = new SafeCriticalSection();

			m_RpcController = new ServerSerialRpcController(this);
		}

		#region Methods

		/// <summary>
		/// Release resources.
		/// </summary>
		public void Dispose()
		{
			SetServer(null);
			SetLightingProcessor(null);
		}

		/// <summary>
		/// Sets the lighting processor to share with clients.
		/// </summary>
		/// <param name="processor"></param>
		[PublicAPI]
		public void SetLightingProcessor(ILightingProcessorDevice processor)
		{
			if (processor == m_Processor)
				return;

			Unsubscribe(m_Processor);
			m_Processor = processor;
			Subscribe(m_Processor);

			InitializeClients();
		}

		/// <summary>
		/// Sets the server for communication with clients.
		/// </summary>
		/// <param name="server"></param>
		[PublicAPI]
		public void SetServer(AsyncTcpServer server)
		{
			if (server == m_Server)
				return;

			Unsubscribe(m_Server);
			m_Server = server;
			m_RpcController.SetServer(m_Server);
			Subscribe(m_Server);

			InitializeClients();
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Sends all of the initial data to the clients.
		/// </summary>
		private void InitializeClients()
		{
			if (m_Server != null)
				m_Server.GetClients().ForEach(InitializeClient);
		}

		/// <summary>
		/// Sends all of the initial data to the client.
		/// </summary>
		/// <param name="client"></param>
		private void InitializeClient(uint client)
		{
			m_RpcController.CallMethod(client, BmsLightingProcessorClientDevice.CLEAR_CONTROLS_RPC);

			if (m_Processor == null)
				return;

			int room;
			if (!TryGetRoomForClient(client, out room))
				return;

			m_RpcController.CallMethod(client, BmsLightingProcessorClientDevice.SET_CACHED_ROOM_RPC, room);

			// Send the loads
			foreach (LightingProcessorControl load in m_Processor.GetLoadsForRoom(room))
			{
				SendControlToClient(client, load);

				float percentage = m_Processor.GetLoadLevel(load);
				if (Math.Abs(percentage) > 0.0001f)
					m_RpcController.CallMethod(client, BmsLightingProcessorClientDevice.SET_CACHED_LOAD_LEVEL_RPC, load.Id, percentage);
			}

			SendControlsToClient(client, m_Processor.GetShadesForRoom(room));
			SendControlsToClient(client, m_Processor.GetShadeGroupsForRoom(room));
			SendControlsToClient(client, m_Processor.GetPresetsForRoom(room));

			// Send the room occupancy
			RoomOccupancyEventArgs.eOccupancyState occupancy = m_Processor.GetOccupancyForRoom(room);
			if (occupancy != default(RoomOccupancyEventArgs.eOccupancyState))
				m_RpcController.CallMethod(client, BmsLightingProcessorClientDevice.SET_CACHED_OCCUPANCY_RPC, occupancy);

			// Send the room preset
			int? activePreset = m_Processor.GetPresetForRoom(room);
			if (activePreset != null)
				m_RpcController.CallMethod(client, BmsLightingProcessorClientDevice.SET_CACHED_ACTIVE_PRESET_RPC, activePreset);
		}

		/// <summary>
		/// Gets the room id for the given client.
		/// Returns false if client has not registered feedback.
		/// </summary>
		/// <param name="clientId"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		private bool TryGetRoomForClient(uint clientId, out int roomId)
		{
			m_ClientRoomSection.Enter();

			try
			{
				return m_ClientRoomMap.TryGetValue(clientId, out roomId);
			}
			finally
			{
				m_ClientRoomSection.Leave();
			}
		}

		/// <summary>
		/// Sends the controls to the given client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="controls"></param>
		private void SendControlsToClient(uint client, IEnumerable<LightingProcessorControl> controls)
		{
			foreach (LightingProcessorControl control in controls)
				SendControlToClient(client, control);
		}

		/// <summary>
		/// Sends the control to the given client via RPC.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="control"></param>
		private void SendControlToClient(uint client, LightingProcessorControl control)
		{
			m_RpcController.CallMethod(client, BmsLightingProcessorClientDevice.ADD_CACHED_CONTROL_RPC, control);
		}

		#endregion

		#region Processor Callbacks

		/// <summary>
		/// Subscribe to the processor events.
		/// </summary>
		/// <param name="processor"></param>
		private void Subscribe(ILightingProcessorDevice processor)
		{
			if (processor == null)
				return;

			processor.OnRoomLoadLevelChanged += ProcessorOnRoomLoadLevelChanged;
			processor.OnRoomOccupancyChanged += ProcessorOnRoomOccupancyChanged;
			processor.OnRoomPresetChanged += ProcessorOnRoomPresetChanged;
			processor.OnRoomControlsChanged += ProcessorOnRoomControlsChanged;
		}

		/// <summary>
		/// Unsubscribe from the processor events.
		/// </summary>
		/// <param name="processor"></param>
		private void Unsubscribe(ILightingProcessorDevice processor)
		{
			if (processor == null)
				return;

			processor.OnRoomLoadLevelChanged -= ProcessorOnRoomLoadLevelChanged;
			processor.OnRoomOccupancyChanged -= ProcessorOnRoomOccupancyChanged;
			processor.OnRoomPresetChanged -= ProcessorOnRoomPresetChanged;
			processor.OnRoomControlsChanged -= ProcessorOnRoomControlsChanged;
		}

		/// <summary>
		/// Called when a room preset changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void ProcessorOnRoomPresetChanged(object sender, RoomPresetChangeEventArgs args)
		{
			const string key = BmsLightingProcessorClientDevice.SET_CACHED_ACTIVE_PRESET_RPC;
			CallActionForClientsByRoomId(args.RoomId, clientId => m_RpcController.CallMethod(clientId, key, args.Preset));
		}

		/// <summary>
		/// Called when a room occupancy state changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void ProcessorOnRoomOccupancyChanged(object sender, RoomOccupancyEventArgs args)
		{
			const string key = BmsLightingProcessorClientDevice.SET_CACHED_OCCUPANCY_RPC;
			CallActionForClientsByRoomId(args.RoomId, clientId => m_RpcController.CallMethod(clientId, key, args.OccupancyState));
		}

		/// <summary>
		/// Called when a load level changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void ProcessorOnRoomLoadLevelChanged(object sender, RoomLoadLevelEventArgs args)
		{
			const string key = BmsLightingProcessorClientDevice.SET_CACHED_LOAD_LEVEL_RPC;
			CallActionForClientsByRoomId(args.RoomId,
			                             clientId => m_RpcController.CallMethod(clientId, key, args.LoadId, args.Percentage));
		}

		/// <summary>
		/// Called when a rooms controls change.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="intEventArgs"></param>
		private void ProcessorOnRoomControlsChanged(object sender, IntEventArgs intEventArgs)
		{
			CallActionForClientsByRoomId(intEventArgs.Data, InitializeClient);
		}

		/// <summary>
		/// Calls the given action for every active client that has registered feedback with the given room id.
		/// </summary>
		/// <param name="roomId"></param>
		/// <param name="action"></param>
		private void CallActionForClientsByRoomId(int roomId, Action<uint> action)
		{
			if (m_Server == null)
				return;

			foreach (uint client in m_Server.GetClients())
			{
				int room;
				if (!TryGetRoomForClient(client, out room) || room != roomId)
					continue;

				action(client);
			}
		}

		#endregion

		#region Server Callbacks

		/// <summary>
		/// Subscribe to server events.
		/// </summary>
		/// <param name="server"></param>
		private void Subscribe(AsyncTcpServer server)
		{
			if (server == null)
				return;

			server.OnSocketStateChange += ServerOnSocketStateChange;
		}

		/// <summary>
		/// Unsubscribe from server events.
		/// </summary>
		/// <param name="server"></param>
		private void Unsubscribe(AsyncTcpServer server)
		{
			if (server == null)
				return;

			server.OnSocketStateChange -= ServerOnSocketStateChange;
		}

		/// <summary>
		/// Called when a client socket state changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void ServerOnSocketStateChange(object sender, SocketStateEventArgs args)
		{
			if (args.SocketState == SocketStateEventArgs.eSocketStatus.SocketStatusConnected)
				return;

			m_ClientRoomSection.Enter();

			try
			{
				m_ClientRoomMap.Remove(args.ClientId);
			}
			finally
			{
				m_ClientRoomSection.Leave();
			}
		}

		#endregion

		#region RPCs

		/// <summary>
		/// Called by clients to request feedback for a given room.
		/// </summary>
		/// <param name="clientId"></param>
		/// <param name="room"></param>
		[Rpc(REGISTER_FEEDBACK_RPC), UsedImplicitly]
		private void RegisterFeedback(uint clientId, int room)
		{
			m_ClientRoomSection.Execute(() => m_ClientRoomMap[clientId] = room);
			InitializeClient(clientId);
		}

		/// <summary>
		/// Called by clients to set the preset for the given room.
		/// </summary>
		/// <param name="clientId"></param>
		/// <param name="room"></param>
		/// <param name="preset"></param>
		[Rpc(SET_ROOM_PRESET_RPC), UsedImplicitly]
		private void SetPresetForRoom(uint clientId, int room, int preset)
		{
			m_Processor.SetPresetForRoom(room, preset);
		}

		#region Load

		/// <summary>
		/// Called by clients to set the lighting level for the given load.
		/// </summary>
		/// <param name="clientId"></param>
		/// <param name="room"></param>
		/// <param name="load"></param>
		/// <param name="percentage"></param>
		[Rpc(SET_LOAD_LEVEL_RPC), UsedImplicitly]
		private void SetLoadLevel(uint clientId, int room, int load, float percentage)
		{
			m_Processor.SetLoadLevel(room, load, percentage);
		}

		/// <summary>
		/// Called by clients to start raising the lighting level for the given load.
		/// </summary>
		/// <param name="clientId"></param>
		/// <param name="room"></param>
		/// <param name="load"></param>
		[Rpc(START_RAISING_LOAD_LEVEL_RPC), UsedImplicitly]
		private void StartRaisingLoadLevel(uint clientId, int room, int load)
		{
			m_Processor.StartRaisingLoadLevel(room, load);
		}

		/// <summary>
		/// Called by clients to start lowering the lighting level for the given load.
		/// </summary>
		/// <param name="clientId"></param>
		/// <param name="room"></param>
		/// <param name="load"></param>
		[Rpc(START_LOWERING_LOAD_LEVEL_RPC), UsedImplicitly]
		private void StartLoweringLoadLevel(uint clientId, int room, int load)
		{
			m_Processor.StartLoweringLoadLevel(room, load);
		}

		/// <summary>
		/// Called by clients to stop ramping the lighting level for the given load.
		/// </summary>
		/// <param name="clientId"></param>
		/// <param name="room"></param>
		/// <param name="load"></param>
		[Rpc(STOP_RAMPING_LOAD_LEVEL_RPC), UsedImplicitly]
		private void StopRampingLoadLevel(uint clientId, int room, int load)
		{
			m_Processor.StopRampingLoadLevel(room, load);
		}

		#endregion

		#region Shade

		/// <summary>
		/// Called by clients to start raising the shade.
		/// </summary>
		/// <param name="clientId"></param>
		/// <param name="room"></param>
		/// <param name="shade"></param>
		[Rpc(START_RAISING_SHADE_RPC), UsedImplicitly]
		private void StartRaisingShade(uint clientId, int room, int shade)
		{
			m_Processor.StartRaisingShade(room, shade);
		}

		/// <summary>
		/// Called by clients to start lowering the shade.
		/// </summary>
		/// <param name="clientId"></param>
		/// <param name="room"></param>
		/// <param name="shade"></param>
		[Rpc(START_LOWERING_SHADE_RPC), UsedImplicitly]
		private void StartLoweringShade(uint clientId, int room, int shade)
		{
			m_Processor.StartLoweringShade(room, shade);
		}

		/// <summary>
		/// Called by clients to stop moving the shade.
		/// </summary>
		/// <param name="clientId"></param>
		/// <param name="room"></param>
		/// <param name="shade"></param>
		[Rpc(STOP_MOVING_SHADE_RPC), UsedImplicitly]
		private void StopMovingShade(uint clientId, int room, int shade)
		{
			m_Processor.StopMovingShade(room, shade);
		}

		#endregion

		#region Shade Group

		/// <summary>
		/// Called by clients to start raising the shade group.
		/// </summary>
		/// <param name="clientId"></param>
		/// <param name="room"></param>
		/// <param name="shadeGroup"></param>
		[Rpc(START_RAISING_SHADE_GROUP_RPC), UsedImplicitly]
		private void StartRaisingShadeGroup(uint clientId, int room, int shadeGroup)
		{
			m_Processor.StartRaisingShadeGroup(room, shadeGroup);
		}

		/// <summary>
		/// Called by clients to start lowering the shade group.
		/// </summary>
		/// <param name="clientId"></param>
		/// <param name="room"></param>
		/// <param name="shadeGroup"></param>
		[Rpc(START_LOWERING_SHADE_GROUP_RPC), UsedImplicitly]
		private void StartLoweringShadeGroup(uint clientId, int room, int shadeGroup)
		{
			m_Processor.StartLoweringShadeGroup(room, shadeGroup);
		}

		/// <summary>
		/// Called by clients to stop moving the shade group.
		/// </summary>
		/// <param name="clientId"></param>
		/// <param name="room"></param>
		/// <param name="shadeGroup"></param>
		[Rpc(STOP_MOVING_SHADE_GROUP_RPC), UsedImplicitly]
		private void StopMovingShadeGroup(uint clientId, int room, int shadeGroup)
		{
			m_Processor.StopMovingShadeGroup(room, shadeGroup);
		}

		#endregion

		#endregion

		#region Console

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			IConsoleNode processor = m_Processor as IConsoleNode;
			if (processor != null)
				yield return processor;
		}

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			yield break;
		}

		#endregion
	}
}
