using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Common.Utils.Timers;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Lighting.EventArguments;
using ICD.Connect.Lighting.Mock.Controls;
using ICD.Connect.Misc.Occupancy;
using ICD.Connect.Protocol.Extensions;
using ICD.Connect.Protocol.Network.RemoteProcedure;
using ICD.Connect.Protocol.Network.Attributes.Rpc;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Lighting.Server
{
	/// <summary>
	/// Provides a way to communicate with the lighting features of BmsOS.
	/// </summary>
	public sealed partial class LightingProcessorClientDevice : AbstractDevice<LightingProcessorClientDeviceSettings>
	{
		// How often to check the connection and reconnect if necessary.
		private const long CONNECTION_CHECK_MILLISECONDS = 30 * 1000;

		public const string CLEAR_CONTROLS_RPC = "ClearControls";
		public const string SET_CACHED_ROOM_RPC = "SetCachedRoom";
		public const string ADD_CACHED_CONTROL_RPC = "AddCachedControl";
		public const string SET_CACHED_OCCUPANCY_RPC = "SetCachedOccupancy";
		public const string SET_CACHED_ACTIVE_PRESET_RPC = "SetCachedPreset";
		public const string SET_CACHED_LOAD_LEVEL_RPC = "SetCachedLoadLevel";

		public event EventHandler<RoomOccupancyEventArgs> OnRoomOccupancyChanged;
		public event EventHandler<RoomPresetChangeEventArgs> OnRoomPresetChanged;
		public event EventHandler<RoomLoadLevelEventArgs> OnRoomLoadLevelChanged;
		public event EventHandler<IntEventArgs> OnRoomControlsChanged;

		public event EventHandler<BoolEventArgs> OnConnectedStateChanged;

		private readonly SafeTimer m_ConnectionTimer;
		private readonly ClientSerialRpcController m_RpcController;

		private ISerialPort m_Port;
		private bool m_IsConnected;

		private int m_RoomId;
		private MockLightingRoom m_Room;

		/// <summary>
		/// Gets the connected state of the client.
		/// </summary>
		[PublicAPI]
		public bool IsConnected
		{
			get { return m_IsConnected; }
			set
			{
				if (value == m_IsConnected)
					return;

				m_IsConnected = value;

				UpdateCachedOnlineStatus();

				if (m_IsConnected)
					Log(eSeverity.Informational, "Connected to server");
				else
					Log(eSeverity.Alert, "Lost connection to server");

				ClearCache();
				if (m_IsConnected)
					m_RpcController.CallMethod(LightingProcessorServer.REGISTER_FEEDBACK_RPC, m_RoomId);

				OnConnectedStateChanged.Raise(this, new BoolEventArgs(m_IsConnected));
			}
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public LightingProcessorClientDevice()
		{
			m_RpcController = new ClientSerialRpcController(this);

			m_ConnectionTimer = new SafeTimer(ConnectionTimerCallback, 0, CONNECTION_CHECK_MILLISECONDS);
		}

		#region Methods

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			OnRoomOccupancyChanged = null;
			OnRoomLoadLevelChanged = null;
			OnRoomPresetChanged = null;
			OnConnectedStateChanged = null;

			m_ConnectionTimer.Dispose();

			base.DisposeFinal(disposing);

			SetPort(null);
			m_RpcController.Dispose();
			ClearCache();
		}

		/// <summary>
		/// Connects to the lighting server.
		/// </summary>
		[PublicAPI]
		public void Connect()
		{
			if (m_Port == null)
			{
				Log(eSeverity.Critical, "Unable to connect, port is null");
				return;
			}

			m_Port.Connect();
		}

		/// <summary>
		/// Disconnects from the lighting server.
		/// </summary>
		[PublicAPI]
		public void Disconnect()
		{
			if (m_Port == null)
			{
				Log(eSeverity.Critical, "Unable to disconnect, port is null");
				return;
			}

			m_Port.Disconnect();
		}

		/// <summary>
		/// Sets the port for communication with the server.
		/// </summary>
		/// <param name="port"></param>
		[PublicAPI]
		public void SetPort(ISerialPort port)
		{
			if (port == m_Port)
				return;

			Unsubscribe(m_Port);

			m_Port = port;
			m_RpcController.SetPort(m_Port);

			Subscribe(m_Port);

			IsConnected = m_Port != null && m_Port.IsConnected;

			UpdateCachedOnlineStatus();
		}

		/// <summary>
		/// Sets the room id for looking up controls.
		/// TODO - this will be replaced when we figure out the scope of the room framework.
		/// </summary>
		/// <param name="roomId"></param>
		[PublicAPI]
		public void SetRoomId(int roomId)
		{
			if (roomId == m_RoomId)
				return;

			m_RoomId = roomId;

			ClearCache();

			if (IsConnected)
				m_RpcController.CallMethod(LightingProcessorServer.REGISTER_FEEDBACK_RPC, m_RoomId);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Logs to logging core.
		/// </summary>
		/// <param name="severity"></param>
		/// <param name="message"></param>
		/// <param name="args"></param>
		private void Log(eSeverity severity, string message, params object[] args)
		{
			message = string.Format(message, args);
			message = string.Format("{0} - {1}", GetType().Name, message);

			Logger.AddEntry(severity, message);
		}

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return m_Port != null && m_Port.IsConnected;
		}

		/// <summary>
		/// Called periodically to maintain connection to the server.
		/// </summary>
		private void ConnectionTimerCallback()
		{
			if (!IsConnected)
				Connect();
		}

		#endregion

		#region Port Callbacks

		/// <summary>
		/// Subscribe to the port events.
		/// </summary>
		/// <param name="port"></param>
		private void Subscribe(ISerialPort port)
		{
			if (port == null)
				return;

			port.OnIsOnlineStateChanged += PortOnIsOnlineStateChanged;
			port.OnConnectedStateChanged += PortOnConnectedStateChanged;
		}

		/// <summary>
		/// Unsubscribe from the port events.
		/// </summary>
		/// <param name="port"></param>
		private void Unsubscribe(ISerialPort port)
		{
			if (port == null)
				return;

			port.OnIsOnlineStateChanged -= PortOnIsOnlineStateChanged;
			port.OnConnectedStateChanged += PortOnConnectedStateChanged;
		}

		/// <summary>
		/// Called when the port online status changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="boolEventArgs"></param>
		private void PortOnIsOnlineStateChanged(object sender, BoolEventArgs boolEventArgs)
		{
			UpdateCachedOnlineStatus();
		}

		/// <summary>
		/// Called when the port connection state changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void PortOnConnectedStateChanged(object sender, BoolEventArgs args)
		{
			IsConnected = args.Data;
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
			OnRoomOccupancyChanged.Raise(this, new RoomOccupancyEventArgs(args.RoomId, args.OccupancyState));
		}

		/// <summary>
		/// Called when a load level changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void RoomOnLoadLevelChanged(object sender, RoomLoadLevelEventArgs args)
		{
			OnRoomLoadLevelChanged.Raise(this, new RoomLoadLevelEventArgs(args.RoomId, args.LoadId, args.Percentage));
		}

		/// <summary>
		/// Called when the room preset changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void RoomOnActivePresetChanged(object sender, RoomPresetChangeEventArgs args)
		{
			OnRoomPresetChanged.Raise(this, new RoomPresetChangeEventArgs(args.RoomId, args.Preset));
		}

		/// <summary>
		/// Called when the room controls change.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void RoomOnControlsChanged(object sender, EventArgs eventArgs)
		{
			OnRoomControlsChanged.Raise(this, new IntEventArgs(m_RoomId));
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
			settings.Port = m_Port == null ? (int?)null : m_Port.Id;
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			SetPort(null);
			m_RoomId = 0;
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(LightingProcessorClientDeviceSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			ISerialPort port = null;

			if (settings.Port != null)
				port = factory.GetPortById((int)settings.Port) as ISerialPort;

			if (port == null)
				Log(eSeverity.Error, "No Serial Port with id {0}", settings.Port);

			SetPort(port);

			SetRoomId(settings.RoomId);
		}

		#endregion

		#region Lighting Processor Methods

		#region Controls

		/// <summary>
		/// Gets the available light loads for the room.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public IEnumerable<LightingProcessorControl> GetLoads()
		{
			return m_Room == null ? new LightingProcessorControl[0] : m_Room.GetLoads();
		}

		/// <summary>
		/// Gets the available individual shades for the room.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public IEnumerable<LightingProcessorControl> GetShades()
		{
			return m_Room == null ? new LightingProcessorControl[0] : m_Room.GetShades();
		}

		/// <summary>
		/// Gets the available shade groups for the room.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public IEnumerable<LightingProcessorControl> GetShadeGroups()
		{
			return m_Room == null ? new LightingProcessorControl[0] : m_Room.GetShadeGroups();
		}

		/// <summary>
		/// Gets the available presets for the room.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public IEnumerable<LightingProcessorControl> GetPresets()
		{
			return m_Room == null ? new LightingProcessorControl[0] : m_Room.GetPresets();
		}

		#endregion

		#region Rooms

		/// <summary>
		/// Gets the current occupancy state for the room.
		/// Defaults to Unknown if we are waiting for a server response.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public eOccupancyState GetOccupancy()
		{
			return m_Room == null ? eOccupancyState.Unknown : m_Room.Occupancy;
		}

		/// <summary>
		/// Sets the preset for the room.
		/// </summary>
		/// <param name="preset"></param>
		[PublicAPI]
		public void SetPreset(int? preset)
		{
			m_RpcController.CallMethod(LightingProcessorServer.SET_ROOM_PRESET_RPC, m_Room.Id, preset);
		}

		/// <summary>
		/// Gets the current preset for the room.
		/// Returns zero if we are waiting for a server response.
		/// </summary>
		[PublicAPI]
		public int? GetActivePreset()
		{
			return m_Room.ActivePreset;
		}

		#endregion

		#region Loads

		/// <summary>
		/// Sets the lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		/// <param name="percentage"></param>
		[PublicAPI]
		public void SetLoadLevel(int load, float percentage)
		{
			m_RpcController.CallMethod(LightingProcessorServer.SET_LOAD_LEVEL_RPC, m_RoomId, load, percentage);
		}

		/// <summary>
		/// Gets the current lighting level for the given load.
		/// Returns zero if we are waiting for a server response.
		/// </summary>
		/// <param name="load"></param>
		[PublicAPI]
		public float GetLoadLevel(int load)
		{
			return m_Room.GetLoad(load).LoadLevel;
		}

		/// <summary>
		/// Starts raising the lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		[PublicAPI]
		public void StartRaisingLoadLevel(int load)
		{
			m_RpcController.CallMethod(LightingProcessorServer.START_RAISING_LOAD_LEVEL_RPC, m_RoomId, load);
		}

		/// <summary>
		/// Starts lowering the lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		[PublicAPI]
		public void StartLoweringLoadLevel(int load)
		{
			m_RpcController.CallMethod(LightingProcessorServer.START_LOWERING_LOAD_LEVEL_RPC, m_RoomId, load);
		}

		/// <summary>
		/// Stops raising/lowering the lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		[PublicAPI]
		public void StopRampingLoadLevel(int load)
		{
			m_RpcController.CallMethod(LightingProcessorServer.STOP_RAMPING_LOAD_LEVEL_RPC, m_RoomId, load);
		}

		#endregion

		#region Shades

		/// <summary>
		/// Starts raising the shade.
		/// </summary>
		/// <param name="shade"></param>
		[PublicAPI]
		public void StartRaisingShade(int shade)
		{
			m_RpcController.CallMethod(LightingProcessorServer.START_RAISING_SHADE_RPC, m_RoomId, shade);
		}

		/// <summary>
		/// Starts lowering the shade.
		/// </summary>
		/// <param name="shade"></param>
		[PublicAPI]
		public void StartLoweringShade(int shade)
		{
			m_RpcController.CallMethod(LightingProcessorServer.START_LOWERING_SHADE_RPC, m_RoomId, shade);
		}

		/// <summary>
		/// Stops moving the shade.
		/// </summary>
		/// <param name="shade"></param>
		[PublicAPI]
		public void StopMovingShade(int shade)
		{
			m_RpcController.CallMethod(LightingProcessorServer.STOP_MOVING_SHADE_RPC, m_RoomId, shade);
		}

		#endregion

		#region Shade Groups

		/// <summary>
		/// Starts raising the shade group.
		/// </summary>
		/// <param name="shadeGroup"></param>
		[PublicAPI]
		public void StartRaisingShadeGroup(int shadeGroup)
		{
			m_RpcController.CallMethod(LightingProcessorServer.START_RAISING_SHADE_GROUP_RPC, m_RoomId, shadeGroup);
		}

		/// <summary>
		/// Starts lowering the shade group.
		/// </summary>
		/// <param name="shadeGroup"></param>
		[PublicAPI]
		public void StartLoweringShadeGroup(int shadeGroup)
		{
			m_RpcController.CallMethod(LightingProcessorServer.START_LOWERING_SHADE_GROUP_RPC, m_RoomId, shadeGroup);
		}

		/// <summary>
		/// Stops moving the shade group.
		/// </summary>
		/// <param name="shadeGroup"></param>
		[PublicAPI]
		public void StopMovingShadeGroup(int shadeGroup)
		{
			m_RpcController.CallMethod(LightingProcessorServer.STOP_MOVING_SHADE_GROUP_RPC, m_RoomId, shadeGroup);
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
			if (m_Room != null && control.Room == m_Room.Id)
				m_Room.AddControl(control);
		}

		/// <summary>
		/// Called by the server to set the current occupancy for the room.
		/// </summary>
		/// <param name="occupancy"></param>
		[Rpc(SET_CACHED_OCCUPANCY_RPC), UsedImplicitly]
		private void SetCachedOccupancy(eOccupancyState occupancy)
		{
			m_Room.Occupancy = occupancy;
		}

		/// <summary>
		/// Called by the server to set the current preset for the room.
		/// </summary>
		/// <param name="preset"></param>
		[Rpc(SET_CACHED_ACTIVE_PRESET_RPC), UsedImplicitly]
		private void SetCachedPreset(int preset)
		{
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

			addRow("IsConnected", m_IsConnected);
		}

		#endregion
	}
}
