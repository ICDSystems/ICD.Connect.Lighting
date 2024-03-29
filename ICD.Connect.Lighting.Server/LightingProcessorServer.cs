﻿using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Lighting.EventArguments;
using ICD.Connect.Lighting.Processors;
using ICD.Connect.Lighting.Shades;
using ICD.Connect.Lighting.Shades.Controls;
using ICD.Connect.Partitioning.Commercial.Controls.Occupancy;
using ICD.Connect.Protocol.EventArguments;
using ICD.Connect.Protocol.Network.Ports.Tcp;
using ICD.Connect.Protocol.Network.Ports.TcpSecure;
using ICD.Connect.Protocol.Network.RemoteProcedure;
using ICD.Connect.Protocol.Network.Attributes.Rpc;
using ICD.Connect.Protocol.Network.Servers;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Settings;

namespace ICD.Connect.Lighting.Server
{
	/// <summary>
	/// Server for hosting ILightingProcessorDevices.
	/// </summary>
	[PublicAPI]
	public sealed class LightingProcessorServer : AbstractLightingProcessorDevice<LightingProcessorServerSettings>
	{
		#region RPC Constants
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
		#endregion

		#region Private Members
		private readonly ServerSerialRpcController m_RpcController;

		private ITcpServer m_Server;
		private ILightingProcessorDevice m_Processor;

		private readonly Dictionary<uint, int> m_ClientRoomMap;
		private readonly SafeCriticalSection m_ClientRoomSection;
		private readonly Dictionary<int, IcdHashSet<IShadeDevice>> m_ShadeOriginatorsByRoom;
		private readonly Dictionary<int, IcdHashSet<IOccupancySensorControl>> m_OccupancyControlsByRoom;
		private readonly Dictionary<IOccupancySensorControl, IcdHashSet<int>> m_OccupancyControlForRooms;

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		public LightingProcessorServer()
		{
			m_ClientRoomMap = new Dictionary<uint, int>();
			m_ClientRoomSection = new SafeCriticalSection();
			m_ShadeOriginatorsByRoom = new Dictionary<int, IcdHashSet<IShadeDevice>>();
			m_OccupancyControlsByRoom = new Dictionary<int, IcdHashSet<IOccupancySensorControl>>();
			m_OccupancyControlForRooms = new Dictionary<IOccupancySensorControl, IcdHashSet<int>>();

			m_RpcController = new ServerSerialRpcController(this);
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			SetServer(null);
			SetLightingProcessor(null);

			m_ShadeOriginatorsByRoom.Clear();

			foreach(IOccupancySensorControl sensor in m_OccupancyControlForRooms.Keys)
				Unsubscribe(sensor);

			m_OccupancyControlsByRoom.Clear();
			m_OccupancyControlForRooms.Clear();
				
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
		public void SetServer(ITcpServer server)
		{
			if (server == m_Server)
				return;

			Unsubscribe(m_Server);
			m_Server = server;
			m_RpcController.SetServer(m_Server);
			Subscribe(m_Server);

			UpdateCachedOnlineStatus();
		}

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return m_Server != null && m_Server.Listening;
		}

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
			m_RpcController.CallMethod(client, LightingProcessorClientDevice.CLEAR_CONTROLS_RPC);

			int room;
			if (!TryGetRoomForClient(client, out room))
				return;

			m_RpcController.CallMethod(client, LightingProcessorClientDevice.SET_CACHED_ROOM_RPC, room);

			// Send the loads
			if (m_Processor != null)
				foreach (LightingProcessorControl load in m_Processor.GetLoadsForRoom(room))
				{
					SendControlToClient(client, load);

					float percentage = m_Processor.GetLoadLevel(load);
					if (Math.Abs(percentage) > 0.0001f)
						m_RpcController.CallMethod(client, LightingProcessorClientDevice.SET_CACHED_LOAD_LEVEL_RPC, load.Id, percentage);
				}

			SendControlsToClient(client, GetShadesForRoom(room));
			SendControlsToClient(client, GetShadeGroupsForRoom(room));
			SendControlsToClient(client, GetPresetsForRoom(room));

			// Send the room occupancy
			eOccupancyState occupancy = GetOccupancyForRoom(room);
			
			if (occupancy != default(eOccupancyState))
				m_RpcController.CallMethod(client, LightingProcessorClientDevice.SET_CACHED_OCCUPANCY_RPC, occupancy);

			// Send the room preset
			if (m_Processor != null)
			{
				int? activePreset = m_Processor.GetPresetForRoom(room);
				if (activePreset != null)
					m_RpcController.CallMethod(client, LightingProcessorClientDevice.SET_CACHED_ACTIVE_PRESET_RPC, activePreset);
			}
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
			m_RpcController.CallMethod(client, LightingProcessorClientDevice.ADD_CACHED_CONTROL_RPC, control);
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
			const string key = LightingProcessorClientDevice.SET_CACHED_ACTIVE_PRESET_RPC;
			CallActionForClientsByRoomId(args.RoomId, clientId => m_RpcController.CallMethod(clientId, key, args.Preset));
		}

		/// <summary>
		/// Called when a room occupancy state changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void ProcessorOnRoomOccupancyChanged(object sender, RoomOccupancyEventArgs args)
		{
			const string key = LightingProcessorClientDevice.SET_CACHED_OCCUPANCY_RPC;
			CallActionForClientsByRoomId(args.RoomId, clientId => m_RpcController.CallMethod(clientId, key, args.OccupancyState));
		}

		/// <summary>
		/// Called when a load level changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void ProcessorOnRoomLoadLevelChanged(object sender, RoomLoadLevelEventArgs args)
		{
			const string key = LightingProcessorClientDevice.SET_CACHED_LOAD_LEVEL_RPC;
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
		private void Subscribe(INetworkServer server)
		{
			if (server == null)
				return;

			server.OnSocketStateChange += ServerOnSocketStateChange;
			server.OnListeningStateChanged += ServerOnListeningStateChanged;
		}

		/// <summary>
		/// Unsubscribe from server events.
		/// </summary>
		/// <param name="server"></param>
		private void Unsubscribe(INetworkServer server)
		{
			if (server == null)
				return;

			server.OnSocketStateChange -= ServerOnSocketStateChange;
			server.OnListeningStateChanged -= ServerOnListeningStateChanged;
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

		private void ServerOnListeningStateChanged(object sender, BoolEventArgs boolEventArgs)
		{
			UpdateCachedOnlineStatus();
		}

		#endregion

		#region Occupancy Sensor Control Callbacks

		private void Subscribe(IOccupancySensorControl occupancySensorControl)
		{
			if (occupancySensorControl == null)
				return;

			occupancySensorControl.OnOccupancyStateChanged += OccupancySensorControlOnOccupancyStateChanged;
		}

		private void Unsubscribe(IOccupancySensorControl occupancySensorControl)
		{
			if (occupancySensorControl == null)
				return;

			occupancySensorControl.OnOccupancyStateChanged -= OccupancySensorControlOnOccupancyStateChanged;
		}

		private void OccupancySensorControlOnOccupancyStateChanged(object sender, GenericEventArgs<eOccupancyState> args)
		{
			IOccupancySensorControl sensor = sender as IOccupancySensorControl;

			if (sensor == null)
				return;


			IcdHashSet<int> rooms;

			if (!m_OccupancyControlForRooms.TryGetValue(sensor, out rooms))
				return;

			const string key = LightingProcessorClientDevice.SET_CACHED_OCCUPANCY_RPC;
			foreach (int roomId in rooms)
				CallActionForClientsByRoomId(roomId, clientId => m_RpcController.CallMethod(clientId, key, args.Data));
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
			if (m_Processor != null)
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
			if (m_Processor != null)
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
			if (m_Processor != null)
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
			if (m_Processor != null)
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
			if (m_Processor != null)
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
			StartRaisingShade(room, shade);
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
			StartLoweringShade(room, shade);
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
			StopMovingShade(room, shade);
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
			StartRaisingShadeGroup(room, shadeGroup);
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
			StartLoweringShadeGroup(room, shadeGroup);
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
			StopMovingShadeGroup(room, shadeGroup);
		}

		#endregion

		#endregion

		#region ILightingProcessorDevice
		/// <summary>
		/// Gets the available rooms.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<int> GetRooms()
		{
			return m_ShadeOriginatorsByRoom.Keys.Union(m_Processor == null
													   ? Enumerable.Empty<int>()
													   : m_Processor.GetRooms());
		}

		/// <summary>
		/// Returns true if the given room is available.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public override bool ContainsRoom(int room)
		{
			return m_ShadeOriginatorsByRoom.ContainsKey(room) || m_OccupancyControlsByRoom.ContainsKey(room) || m_Processor != null && m_Processor.ContainsRoom(room);
		}

		/// <summary>
		/// Gets the available light loads for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public override IEnumerable<LightingProcessorControl> GetLoadsForRoom(int room)
		{
			return m_Processor == null ? Enumerable.Empty<LightingProcessorControl>() : m_Processor.GetLoadsForRoom(room);
		}

		/// <summary>
		/// Gets the available individual shades for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public override IEnumerable<LightingProcessorControl> GetShadesForRoom(int room)
		{
			List<LightingProcessorControl> list = new List<LightingProcessorControl>();
			if (m_ShadeOriginatorsByRoom.ContainsKey(room))
			{
				list.AddRange(m_ShadeOriginatorsByRoom[room].Where(orig => !(orig is ShadeGroup))
															.Select(originator => new LightingProcessorControl(
															LightingProcessorControl.ePeripheralType.Shade,
															originator.Id, room, originator.Name, originator.ShadeType)));
			}
			
			if (m_Processor == null)
				return list;
			
			return m_Processor.GetShadesForRoom(room).Concat(list);
		}

		/// <summary>
		/// Gets the available shade groups for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public override IEnumerable<LightingProcessorControl> GetShadeGroupsForRoom(int room)
		{
			List<LightingProcessorControl> list = new List<LightingProcessorControl>();
			if (m_ShadeOriginatorsByRoom.ContainsKey(room))
				list.AddRange(m_ShadeOriginatorsByRoom[room].Where(orig => (orig is ShadeGroup))
			                                            .Select(originator => new LightingProcessorControl(
			                                            LightingProcessorControl.ePeripheralType.ShadeGroup,
			                                            originator.Id, room, originator.Name, originator.ShadeType)));

			if (m_Processor == null)
				return list;
			
			return m_Processor.GetShadeGroupsForRoom(room).Concat(list);
		}

		/// <summary>
		/// Gets the available presets for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public override IEnumerable<LightingProcessorControl> GetPresetsForRoom(int room)
		{
			return m_Processor == null ? Enumerable.Empty<LightingProcessorControl>() : m_Processor.GetPresetsForRoom(room);
		}

		/// <summary>
		/// Sets the preset for the given room.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="preset"></param>
		public override void SetPresetForRoom(int room, int? preset)
		{
			if (m_Processor == null)
				return;

			m_Processor.SetPresetForRoom(room, preset);
		}

		/// <summary>
		/// Gets the current preset for the given room.
		/// </summary>
		/// <param name="room"></param>
		public override int? GetPresetForRoom(int room)
		{
			if (m_Processor == null)
				return null;

			return m_Processor.GetPresetForRoom(room);
		}

		public override eOccupancyState GetOccupancyForRoom(int room)
		{
			eOccupancyState occupancy = eOccupancyState.Unknown;

			// Get the occupancy state from the processor, if there is one
			if (m_Processor != null)
			{
				occupancy = m_Processor.GetOccupancyForRoom(room);
				if (occupancy == eOccupancyState.Occupied)
					return occupancy;
			}
				

			// Get occupancy state from sensors
			// Any sensor occupied overrides, 
			IcdHashSet<IOccupancySensorControl> sensors;
			if (m_OccupancyControlsByRoom.TryGetValue(room, out sensors))
			{
				if (sensors.Any(s => s.OccupancyState == eOccupancyState.Occupied))
					return eOccupancyState.Occupied;
				// If processor already set it to unoccupied, don't bother checking the sensors
				if (occupancy == eOccupancyState.Unoccupied || sensors.Any(s => s.OccupancyState == eOccupancyState.Unoccupied))
					return eOccupancyState.Unoccupied;
			}

			return occupancy;

		}

		/// <summary>
		/// Sets the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		/// <param name="percentage"></param>
		public override void SetLoadLevel(int room, int load, float percentage)
		{
			if (m_Processor == null)
				return;

			m_Processor.SetLoadLevel(room, load, percentage);
		}

		/// <summary>
		/// Gets the current lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		public override float GetLoadLevel(int room, int load)
		{
			return m_Processor.GetLoadLevel(room, load);
		}

		/// <summary>
		/// Starts raising the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		public override void StartRaisingLoadLevel(int room, int load)
		{
			if (m_Processor == null)
				return;

			m_Processor.StartRaisingLoadLevel(room, load);
		}

		/// <summary>
		/// Starts lowering the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		public override void StartLoweringLoadLevel(int room, int load)
		{
			if (m_Processor == null)
				return;

			m_Processor.StartLoweringLoadLevel(room, load);
		}

		/// <summary>
		/// Stops raising/lowering the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		public override void StopRampingLoadLevel(int room, int load)
		{
			if (m_Processor == null)
				return;

			m_Processor.StopRampingLoadLevel(room, load);
		}

		/// <summary>
		/// Starts raising the shade.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shade"></param>
		public override void StartRaisingShade(int room, int shade)
		{
			IShadeWithStop shadeOriginator = GetShadeOriginatorFromShadeOriginatorsByRoom(room, shade);
			if (shadeOriginator != null)
			{
				shadeOriginator.Open();
				return;
			}

			m_Processor.StartRaisingShade(room, shade);
		}

		/// <summary>
		/// Starts lowering the shade.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shade"></param>
		public override void StartLoweringShade(int room, int shade)
		{
			IShadeWithStop shadeOriginator = GetShadeOriginatorFromShadeOriginatorsByRoom(room, shade);
			if (shadeOriginator != null)
			{
				shadeOriginator.Close();
				return;
			}

			m_Processor.StartLoweringShade(room, shade);
		}

		/// <summary>
		/// Stops moving the shade.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shade"></param>
		public override void StopMovingShade(int room, int shade)
		{
			IShadeWithStop shadeOriginator = GetShadeOriginatorFromShadeOriginatorsByRoom(room, shade);
			if (shadeOriginator != null)
			{
				shadeOriginator.Stop();
				return;
			}

			m_Processor.StopMovingShade(room, shade);
		}

		/// <summary>
		/// Starts raising the shade group.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shadeGroup"></param>
		public override void StartRaisingShadeGroup(int room, int shadeGroup)
		{
			IShadeWithStop shadeOriginator = GetShadeOriginatorFromShadeOriginatorsByRoom(room, shadeGroup);
			if (shadeOriginator != null)
			{
				shadeOriginator.Open();
				return;
			}

			m_Processor.StartRaisingShadeGroup(room, shadeGroup);
		}

		/// <summary>
		/// Starts lowering the shade group.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shadeGroup"></param>
		public override void StartLoweringShadeGroup(int room, int shadeGroup)
		{
			IShadeWithStop shadeOriginator = GetShadeOriginatorFromShadeOriginatorsByRoom(room, shadeGroup);
			if (shadeOriginator != null)
			{
				shadeOriginator.Close();
				return;
			}

			m_Processor.StartLoweringShadeGroup(room, shadeGroup);
		}

		/// <summary>
		/// Stops moving the shade group.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shadeGroup"></param>
		public override void StopMovingShadeGroup(int room, int shadeGroup)
		{
			IShadeWithStop shadeOriginator = GetShadeOriginatorFromShadeOriginatorsByRoom(room, shadeGroup);
			if (shadeOriginator != null)
			{
				shadeOriginator.Stop();
				return;
			}

			m_Processor.StopMovingShadeGroup(room, shadeGroup);
		}

		#endregion

		#region Private Helper Methods
		[CanBeNull]
		private IShadeWithStop GetShadeOriginatorFromShadeOriginatorsByRoom(int room, int id)
		{
			if (!m_ShadeOriginatorsByRoom.ContainsKey(room))
				return null;

			IShadeDevice shadeOriginator = m_ShadeOriginatorsByRoom[room].FirstOrDefault(originator => originator.Id == id);

			if (shadeOriginator is IShadeWithStop)
			{
				return shadeOriginator as IShadeWithStop;
			}

			return null;
		}
		#endregion

		#region Console

		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		public override string ConsoleName { get { return "LightingServer"; } }

		/// <summary>
		/// Gets the help information for the node.
		/// </summary>
		public override string ConsoleHelp { get { return "The BmsOS lighting server"; } }

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new ConsoleCommand("PrintOccSensors", "Prints Occupancy Sensors", () => PrintOccupancySensorsToConsole());
			yield return new ConsoleCommand("PrintClientsToRooms", "Prints clients, and what room is associated with them", () => PrintClientsToRooms());
		}

		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			foreach (IConsoleNodeBase command in GetBaseConsoleNodes())
				yield return command;

			if (m_Processor != null)
				yield return m_Processor;

			if (m_Server != null)
				yield return m_Server;
		}

		private IEnumerable<IConsoleNodeBase> GetBaseConsoleNodes()
		{
			return base.GetConsoleNodes();
		}

		private void PrintOccupancySensorsToConsole()
		{
			if (m_OccupancyControlForRooms.Count == 0)
			{
				IcdConsole.PrintLine("No Occupancy Sensors Configured");
				return;
			}

			TableBuilder table = new TableBuilder("ID", "ControlId", "Rooms" );
			foreach (KeyValuePair<IOccupancySensorControl, IcdHashSet<int>> kvp in m_OccupancyControlForRooms)
			{
				bool first = true;
				foreach (int room in kvp.Value)
				{
					if (first)
						table.AddRow(kvp.Key.Parent.Id, kvp.Key.Id, room);
					else
						table.AddRow(null, null, room);
					first = false;
				}
				table.AddSeparator();
			}

			IcdConsole.PrintLine(table.ToString());

		}

		private string PrintClientsToRooms()
		{
			KeyValuePair<uint, int>[] clientRoomMap = m_ClientRoomSection.Execute(() => m_ClientRoomMap.ToArray(m_ClientRoomMap.Count));

			TableBuilder table = new TableBuilder("Client ID", "Room ID", "HostInfo");

			foreach (var kvp in clientRoomMap)
			{
				HostInfo info;
				m_Server.TryGetClientInfo(kvp.Key, out info);

				table.AddRow(kvp.Key, info, kvp.Value);
			}

			table.AddSeparator();

			return table.ToString();
		}
		#endregion

		#region Settings

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(LightingProcessorServerSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			if (settings.LightingProcessorId != null)
			{
				m_Processor = factory.GetOriginatorById<ILightingProcessorDevice>(settings.LightingProcessorId.Value);

				if (m_Processor == null)
				{
					LogApplySettingsError("No Lighting Processor found with Id:" + settings.LightingProcessorId.Value);
					return;
				}

				Subscribe(m_Processor);
			}

			foreach (int shade in settings.ShadeIds.Concat(settings.ShadeGroupIds))
			{
				IShadeDevice shadeOriginator = factory.GetOriginatorById<IShadeDevice>(shade);
				if (shadeOriginator == null)
					continue;

				int? roomId = settings.FindRoomIdForPeripheral(shade);
				if (roomId == null)
					continue;

				if (!m_ShadeOriginatorsByRoom.ContainsKey(roomId.Value))
				{
					m_ShadeOriginatorsByRoom[roomId.Value] = new IcdHashSet<IShadeDevice>();
				}
				m_ShadeOriginatorsByRoom[roomId.Value].Add(shadeOriginator);
			}

			foreach (int sensorId in settings.OccupancySensorIds)
			{
				IDevice sensor = factory.GetOriginatorById<IDevice>(sensorId);
				if (sensor == null)
					continue;

				IOccupancySensorControl occupancySensorControl = sensor.Controls.GetControl<IOccupancySensorControl>();
				if (occupancySensorControl == null)
					continue;

				int? roomId = settings.FindRoomIdForPeripheral(sensorId);
				if (roomId == null)
					continue;

				m_OccupancyControlsByRoom.GetOrAddNew(roomId.Value).Add(occupancySensorControl);

				m_OccupancyControlForRooms.GetOrAddNew(occupancySensorControl).Add(roomId.Value);

				Subscribe(occupancySensorControl);
			}

			ITcpServer tcpServer;

			if (settings.UseSecureServer)
			{
				tcpServer = new IcdSecureTcpServer
				{
					Port = settings.ServerPort,
					MaxNumberOfClients = settings.ServerMaxClients
				};
			}
			else
			{
				tcpServer = new IcdTcpServer
				{
					Port = settings.ServerPort,
					MaxNumberOfClients = settings.ServerMaxClients
				};
			}

			SetServer(tcpServer);
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(LightingProcessorServerSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.ClearIdCollections();

			settings.LightingProcessorId = m_Processor == null ? null : (int?)m_Processor.Id;

			if (m_Server != null)
			{
				settings.ServerPort = m_Server.Port;
				settings.ServerMaxClients = m_Server.MaxNumberOfClients;
			}

			settings.RoomIds = GetRooms();

			foreach (int room in GetRooms())
			{
				if (m_ShadeOriginatorsByRoom.ContainsKey(room))
				{
					foreach (IShadeDevice shade in m_ShadeOriginatorsByRoom[room].Where(shade => !(shade is ShadeGroup)))
					{
						settings.AddShade(room, shade.Id);
					}

					foreach (IShadeDevice shade in m_ShadeOriginatorsByRoom[room].Where(shade => (shade is ShadeGroup)))
					{
						settings.AddShadeGroup(room, shade.Id);
					}
				}

				if (m_OccupancyControlsByRoom.ContainsKey(room))
				{
					foreach (IOccupancySensorControl sensor in m_OccupancyControlsByRoom[room])
						settings.AddOccupancySensor(room, sensor.Parent.Id);
				}
			}
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			SetServer(null);

			SetLightingProcessor(null);

			m_ShadeOriginatorsByRoom.Clear();

			m_OccupancyControlsByRoom.Clear();
			m_OccupancyControlForRooms.Clear();
		}

		/// <summary>
		/// Override to add actions on StartSettings
		/// This should be used to start communications with devices and perform initial actions
		/// </summary>
		protected override void StartSettingsFinal()
		{
			base.StartSettingsFinal();

			m_Server.Start();
			InitializeClients();
		}

		private void LogApplySettingsError(string message)
		{
			Logger.Log(eSeverity.Error, "Failed to apply settings for Lighting Processor Server - {0}", message);
		}

		#endregion
	}
}
