using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Lighting.EventArguments;
using ICD.Connect.Lighting.Mock.Controls;
using ICD.Connect.Lighting.RoomInterface;
using ICD.Connect.Partitioning.Commercial.Controls.Occupancy;
using ICD.Connect.Protocol.Extensions;
using ICD.Connect.Protocol.Network.RemoteProcedure;
using ICD.Connect.Protocol.Network.Attributes.Rpc;
using ICD.Connect.Protocol.Network.Settings;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Settings;

namespace ICD.Connect.Lighting.Server
{
	/// <summary>
	/// Provides a way to communicate with the lighting features of BmsOS.
	/// </summary>
	public sealed class LightingProcessorClientDevice : AbstractLightingRoomInterfaceDevice<LightingProcessorClientDeviceSettings>
	{
		public const string CLEAR_CONTROLS_RPC = "ClearControls";
		public const string SET_CACHED_ROOM_RPC = "SetCachedRoom";
		public const string ADD_CACHED_CONTROL_RPC = "AddCachedControl";
		public const string SET_CACHED_OCCUPANCY_RPC = "SetCachedOccupancy";
		public const string SET_CACHED_ACTIVE_PRESET_RPC = "SetCachedPreset";
		public const string SET_CACHED_LOAD_LEVEL_RPC = "SetCachedLoadLevel";

		public event EventHandler<BoolEventArgs> OnConnectedStateChanged;
		public override event EventHandler<GenericEventArgs<eOccupancyState>> OnOccupancyChanged;
		public override event EventHandler<GenericEventArgs<int?>> OnPresetChanged;
		public override event EventHandler<LoadLevelEventArgs> OnLoadLevelChanged;
		public override event EventHandler OnControlsChanged;

		private readonly ClientSerialRpcController m_RpcController;
		private readonly SecureNetworkProperties m_NetworkProperties;

		[CanBeNull]
		private MockLightingRoom m_Room;

		private int m_RoomId;

		/// <summary>
		/// Gets the connected state of the client.
		/// </summary>
		[PublicAPI]
		public bool IsConnected
		{
			get { return m_RpcController.IsConnected; }
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public LightingProcessorClientDevice()
		{
			m_NetworkProperties = new SecureNetworkProperties();

			m_RpcController = new ClientSerialRpcController(this);

			Subscribe(m_RpcController);
		}

		#region Methods

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			OnOccupancyChanged = null;
			OnLoadLevelChanged = null;
			OnPresetChanged = null;
			OnControlsChanged = null;
			OnConnectedStateChanged = null;

			base.DisposeFinal(disposing);

			Unsubscribe(m_RpcController);
			m_RpcController.Dispose();
			ClearCache();
		}

		/// <summary>
		/// Sets the room id for looking up controls.
		/// </summary>
		/// <param name="roomId"></param>
		[PublicAPI]
		public void SetRoomId(int roomId)
		{
			ClearCache();

			m_RoomId = roomId;

			if (IsConnected)
				m_RpcController.CallMethod(LightingProcessorServer.REGISTER_FEEDBACK_RPC, m_RoomId);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return m_RpcController != null && m_RpcController.IsOnline;
		}

		#endregion

		#region Rpc Controller Callbacks

		private void Subscribe(ClientSerialRpcController rpcController)
		{
			if (m_RpcController == null)
				return;

			rpcController.OnConnectedStateChanged += RpcControllerOnConnectedStateChanged;
			rpcController.OnIsOnlineStateChanged += RpcControllerOnIsOnlineStateChanged;
		}

		private void Unsubscribe(ClientSerialRpcController rpcController)
		{
			if (m_RpcController == null)
				return;

			rpcController.OnConnectedStateChanged -= RpcControllerOnConnectedStateChanged;
			rpcController.OnIsOnlineStateChanged -= RpcControllerOnIsOnlineStateChanged;
		}

		private void RpcControllerOnIsOnlineStateChanged(object sender, BoolEventArgs args)
		{
			UpdateCachedOnlineStatus();
		}

		private void RpcControllerOnConnectedStateChanged(object sender, BoolEventArgs args)
		{
			OnConnectedStateChanged.Raise(this, new BoolEventArgs(args.Data));
			UpdateCachedOnlineStatus();

			if (args.Data)
				m_RpcController.CallMethod(LightingProcessorServer.REGISTER_FEEDBACK_RPC, m_RoomId);
			else
				ClearCache();
		}

		#endregion

		#region Room Callbacks

		/// <summary>
		/// Subscribe to the room events.
		/// </summary>
		/// <param name="room"></param>
		private void Subscribe(MockLightingRoom room)
		{
			if (room == null)
				return;

			room.OnActivePresetChanged += RoomOnActivePresetChanged;
			room.OnLoadLevelChanged += RoomOnLoadLevelChanged;
			room.OnOccupancyChanged += RoomOnOccupancyChanged;
			room.OnControlsChanged += RoomOnControlsChanged;
		}

		/// <summary>
		/// Unsubscribe from the room events.
		/// </summary>
		/// <param name="room"></param>
		private void Unsubscribe(MockLightingRoom room)
		{
			if (room == null)
				return;

			room.OnActivePresetChanged -= RoomOnActivePresetChanged;
			room.OnLoadLevelChanged -= RoomOnLoadLevelChanged;
			room.OnOccupancyChanged -= RoomOnOccupancyChanged;
			room.OnControlsChanged -= RoomOnControlsChanged;
		}

		/// <summary>
		/// Called when the room occupancy changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void RoomOnOccupancyChanged(object sender, RoomOccupancyEventArgs args)
		{
			if (m_Room != null && m_Room.Id == args.RoomId)
				OnOccupancyChanged.Raise(this, new GenericEventArgs<eOccupancyState>(args.OccupancyState));
		}

		/// <summary>
		/// Called when a load level changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void RoomOnLoadLevelChanged(object sender, RoomLoadLevelEventArgs args)
		{
			if (m_Room != null && args.RoomId == m_Room.Id)
				OnLoadLevelChanged.Raise(this, new LoadLevelEventArgs(args.LoadId, args.Percentage));
		}

		/// <summary>
		/// Called when the room preset changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void RoomOnActivePresetChanged(object sender, RoomPresetChangeEventArgs args)
		{
			if (m_Room != null && args.RoomId == m_Room.Id)
				OnPresetChanged.Raise(this, new GenericEventArgs<int?>(args.Preset));
		}

		/// <summary>
		/// Called when the room controls change.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void RoomOnControlsChanged(object sender, EventArgs args)
		{
			OnControlsChanged.Raise(this);
		}

		#endregion

		#region Settings

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(LightingProcessorClientDeviceSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.RoomId = m_RoomId;
			settings.Port = m_RpcController.PortNumber;

			settings.Copy(m_NetworkProperties);
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			m_RpcController.SetPort(null, false);
			ClearCache();

			m_NetworkProperties.ClearNetworkProperties();
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(LightingProcessorClientDeviceSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			m_NetworkProperties.Copy(settings);

			SetRoomId(settings.RoomId);

			ISerialPort port = null;

			if (settings.Port != null)
			{
				try
				{
					port = factory.GetPortById((int)settings.Port) as ISerialPort;
				}
				catch (KeyNotFoundException)
				{
					Logger.Log(eSeverity.Error, "No Serial Port with id {0}", settings.Port);
				}	
			}

			m_RpcController.SetPort(port, false);
		}

		/// <summary>
		/// Override to add actions on StartSettings
		/// This should be used to start communications with devices and perform initial actions
		/// </summary>
		protected override void StartSettingsFinal()
		{
			base.StartSettingsFinal();

			m_RpcController.Start();
		}

		#endregion

		#region Lighting Processor Methods

		#region Controls

		/// <summary>
		/// Gets the available light loads for the room.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public override IEnumerable<LightingProcessorControl> GetLoads()
		{
			return m_Room == null ? Enumerable.Empty<LightingProcessorControl>() : m_Room.GetLoads();
		}

		/// <summary>
		/// Gets the available individual shades for the room.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public override IEnumerable<LightingProcessorControl> GetShades()
		{
			return m_Room == null ? Enumerable.Empty<LightingProcessorControl>() : m_Room.GetShades();
		}

		/// <summary>
		/// Gets the available shade groups for the room.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public override IEnumerable<LightingProcessorControl> GetShadeGroups()
		{
			return m_Room == null ? Enumerable.Empty<LightingProcessorControl>() : m_Room.GetShadeGroups();
		}

		/// <summary>
		/// Gets the available presets for the room.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public override IEnumerable<LightingProcessorControl> GetPresets()
		{
			return m_Room == null ? Enumerable.Empty<LightingProcessorControl>() : m_Room.GetPresets();
		}

		#endregion

		#region Rooms

		/// <summary>
		/// Gets the current occupancy state for the room.
		/// Defaults to Unknown if we are waiting for a server response.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public override eOccupancyState GetOccupancy()
		{
			return m_Room == null ? eOccupancyState.Unknown : m_Room.Occupancy;
		}

		/// <summary>
		/// Sets the preset for the room.
		/// </summary>
		/// <param name="preset"></param>
		[PublicAPI]
		public override void SetPreset(int? preset)
		{
			if (m_Room == null)
				throw new InvalidOperationException("No Room has been configured");

			m_RpcController.CallMethod(LightingProcessorServer.SET_ROOM_PRESET_RPC, m_Room.Id, preset);
		}

		/// <summary>
		/// Gets the current preset for the given room.
		/// </summary>
		[PublicAPI]
		public override int? GetPreset()
		{
			return m_Room == null ? null : m_Room.ActivePreset;
		}

		#endregion

		#region Loads

		/// <summary>
		/// Sets the lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		/// <param name="percentage"></param>
		[PublicAPI]
		public override void SetLoadLevel(int load, float percentage)
		{
			if (m_Room == null)
				throw new InvalidOperationException("No Room has been configured");

			m_RpcController.CallMethod(LightingProcessorServer.SET_LOAD_LEVEL_RPC, m_Room.Id, load, percentage);
		}

		/// <summary>
		/// Gets the current lighting level for the given load.
		/// Returns zero if we are waiting for a server response.
		/// </summary>
		/// <param name="load"></param>
		[PublicAPI]
		public override float GetLoadLevel(int load)
		{
			return m_Room == null ? 0.0f : m_Room.GetLoad(load).LoadLevel;
		}

		/// <summary>
		/// Starts raising the lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		[PublicAPI]
		public override void StartRaisingLoadLevel(int load)
		{
			if (m_Room == null)
				throw new InvalidOperationException("No Room has been configured");

			m_RpcController.CallMethod(LightingProcessorServer.START_RAISING_LOAD_LEVEL_RPC, m_Room.Id, load);
		}

		/// <summary>
		/// Starts lowering the lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		[PublicAPI]
		public override void StartLoweringLoadLevel(int load)
		{
			if (m_Room == null)
				throw new InvalidOperationException("No Room has been configured");

			m_RpcController.CallMethod(LightingProcessorServer.START_LOWERING_LOAD_LEVEL_RPC, m_Room.Id, load);
		}

		/// <summary>
		/// Stops raising/lowering the lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		[PublicAPI]
		public override void StopRampingLoadLevel(int load)
		{
			if (m_Room == null)
				throw new InvalidOperationException("No Room has been configured");

			m_RpcController.CallMethod(LightingProcessorServer.STOP_RAMPING_LOAD_LEVEL_RPC, m_Room.Id, load);
		}

		#endregion

		#region Shades

		/// <summary>
		/// Starts raising the shade.
		/// </summary>
		/// <param name="shade"></param>
		[PublicAPI]
		public override void StartRaisingShade(int shade)
		{
			if (m_Room == null)
				throw new InvalidOperationException("No Room has been configured");

			m_RpcController.CallMethod(LightingProcessorServer.START_RAISING_SHADE_RPC, m_Room.Id, shade);
		}

		/// <summary>
		/// Starts lowering the shade.
		/// </summary>
		/// <param name="shade"></param>
		[PublicAPI]
		public override void StartLoweringShade(int shade)
		{
			if (m_Room == null)
				throw new InvalidOperationException("No Room has been configured");

			m_RpcController.CallMethod(LightingProcessorServer.START_LOWERING_SHADE_RPC, m_Room.Id, shade);
		}

		/// <summary>
		/// Stops moving the shade.
		/// </summary>
		/// <param name="shade"></param>
		[PublicAPI]
		public override void StopMovingShade(int shade)
		{
			if (m_Room == null)
				throw new InvalidOperationException("No Room has been configured");

			m_RpcController.CallMethod(LightingProcessorServer.STOP_MOVING_SHADE_RPC, m_Room.Id, shade);
		}

		#endregion

		#region Shade Groups

		/// <summary>
		/// Starts raising the shade group.
		/// </summary>
		/// <param name="shadeGroup"></param>
		[PublicAPI]
		public override void StartRaisingShadeGroup(int shadeGroup)
		{
			if (m_Room == null)
				throw new InvalidOperationException("No Room has been configured");

			m_RpcController.CallMethod(LightingProcessorServer.START_RAISING_SHADE_GROUP_RPC, m_Room.Id, shadeGroup);
		}

		/// <summary>
		/// Starts lowering the shade group.
		/// </summary>
		/// <param name="shadeGroup"></param>
		[PublicAPI]
		public override void StartLoweringShadeGroup(int shadeGroup)
		{
			if (m_Room == null)
				throw new InvalidOperationException("No Room has been configured");

			m_RpcController.CallMethod(LightingProcessorServer.START_LOWERING_SHADE_GROUP_RPC, m_Room.Id, shadeGroup);
		}

		/// <summary>
		/// Stops moving the shade group.
		/// </summary>
		/// <param name="shadeGroup"></param>
		[PublicAPI]
		public override void StopMovingShadeGroup(int shadeGroup)
		{
			if (m_Room == null)
				throw new InvalidOperationException("No Room has been configured");

			m_RpcController.CallMethod(LightingProcessorServer.STOP_MOVING_SHADE_GROUP_RPC, m_Room.Id, shadeGroup);
		}

		#endregion

		#endregion

		#region RPCs

		/// <summary>
		/// Clears the cached data from the server.
		/// </summary>
		[Rpc(CLEAR_CONTROLS_RPC), UsedImplicitly]
		private void ClearCache()
		{
			Unsubscribe(m_Room);

			if (m_Room != null)
				m_Room.Dispose();

			m_Room = null;
		}

		/// <summary>
		/// Called by the server to start sending all of the available controls.
		/// </summary>
		/// <param name="roomId"></param>
		[Rpc(SET_CACHED_ROOM_RPC), UsedImplicitly]
		private void SetCachedRoom(int roomId)
		{
			if (m_Room != null && roomId == m_Room.Id)
				return;

			ClearCache();
			m_Room = new MockLightingRoom(roomId);
			Subscribe(m_Room);
		}

		/// <summary>
		/// Called by the server to add a control.
		/// </summary>
		/// <param name="control"></param>
		[Rpc(ADD_CACHED_CONTROL_RPC), UsedImplicitly]
		private void AddCachedControl(LightingProcessorControl control)
		{
			if (m_Room == null)
				throw new InvalidOperationException("No Room has been configured");

			// Server tells us about controls for all rooms?
			if (control.Room != m_Room.Id)
				return;

			m_Room.AddControl(control);
		}

		/// <summary>
		/// Called by the server to set the current occupancy for the room.
		/// </summary>
		/// <param name="occupancy"></param>
		[Rpc(SET_CACHED_OCCUPANCY_RPC), UsedImplicitly]
		private void SetCachedOccupancy(eOccupancyState occupancy)
		{
			if (m_Room == null)
				throw new InvalidOperationException("No Room has been configured");

			m_Room.Occupancy = occupancy;
		}

		/// <summary>
		/// Called by the server to set the current preset for the room.
		/// </summary>
		/// <param name="preset"></param>
		[Rpc(SET_CACHED_ACTIVE_PRESET_RPC), UsedImplicitly]
		private void SetCachedPreset(int preset)
		{
			if (m_Room == null)
				throw new InvalidOperationException("No Room has been configured");

			m_Room.ActivePreset = preset;
		}

		/// <summary>
		/// Called by the server to set the current level for the load.
		/// </summary>
		/// <param name="load"></param>
		/// <param name="percentage"></param>
		[Rpc(SET_CACHED_LOAD_LEVEL_RPC), UsedImplicitly]
		private void SetCachedLoadLevel(int load, float percentage)
		{
			if (m_Room == null)
				throw new InvalidOperationException("No Room has been configured");

			m_Room.GetLoad(load).LoadLevel = percentage;
		}

		#endregion

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			addRow("IsConnected", IsConnected);
			addRow("Room Id", m_RoomId);
			addRow("OccupancyStatus", GetOccupancy());
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new ConsoleCommand("Connect", "Connects the RPC", () => m_RpcController.Connect());
			yield return new ConsoleCommand("Disconnect", "Disconnect the RPC", () => m_RpcController.Disconnect());
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
			foreach (IConsoleNodeBase node in GetBaseConsoleNodes())
				yield return node;

			if (m_Room != null)
				yield return m_Room;
		}

		private IEnumerable<IConsoleNodeBase> GetBaseConsoleNodes()
		{
			return base.GetConsoleNodes();
		}

		#endregion
	}
}
