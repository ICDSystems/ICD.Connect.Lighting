using System;
using System.Collections.Generic;
using System.Text;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.IO;
using ICD.Common.Utils.Services.Logging;
using ICD.Common.Utils.Timers;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Lighting.EventArguments;
using ICD.Connect.Lighting.Lutron.Nwk.EventArguments;
using ICD.Connect.Protocol;
using ICD.Connect.Protocol.Extensions;
using ICD.Connect.Protocol.Network.Ports;
using ICD.Connect.Protocol.Network.Settings;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.Ports.ComPort;
using ICD.Connect.Protocol.SerialBuffers;
using ICD.Connect.Protocol.Settings;
using ICD.Connect.Settings;

namespace ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice
{

		/// <summary>
		/// Callback for parser events.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="data"></param>
		public delegate void ParserCallback(ILutronNwkDevice sender, string data);
	
	/// <summary>
	/// The LutronQuantumNwkDevice provides an interface for controlling a QuantumNwk
	/// lighting processor over a serial connection.
	/// 
	/// Commands  must be sent at timed intervals due to the limited speed of the processor.
	/// Due to this we prioritise EXECUTE commands and hold QUERY commands until all
	/// EXECUTE commands have been processed.
	/// </summary>
	public abstract partial class AbstractLutronNwkDevice<T> : AbstractDevice<T>, ILutronNwkDevice where T : ILutronNwkDeviceSettings, new()
	{
		/// <summary>
		/// How often we send a new data string to the lighting processor.
		/// </summary>
		private const int COMMUNICATION_INTERVAL_MILLISECONDS = 250;

		/// <summary>
		/// Raised when the class initializes.
		/// </summary>
		public event EventHandler<BoolEventArgs> OnInitializedChanged;

		/// <summary>
		/// Raised when the device becomes connected or disconnected.
		/// </summary>
		public event EventHandler<BoolEventArgs> OnConnectedStateChanged;

		public event EventHandler<RoomOccupancyEventArgs> OnRoomOccupancyChanged;
		public event EventHandler<RoomPresetChangeEventArgs> OnRoomPresetChanged;
		public event EventHandler<RoomLoadLevelEventArgs> OnRoomLoadLevelChanged;
		public event EventHandler<IntEventArgs> OnRoomControlsChanged;

		/// <summary>
		/// Gets the available rooms.
		/// </summary>
		/// <returns></returns>
		public abstract IEnumerable<int> GetRooms();

		/// <summary>
		/// Returns true if the given room is available.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public abstract bool ContainsRoom(int room);

		private readonly ISerialBuffer m_SerialBuffer;

		/// <summary>
		/// True when the system is logged in, or NWK> is on the prompt
		/// </summary>
		private bool m_ConnectionIsReady;



		private readonly Dictionary<string, IcdHashSet<ParserCallback>> m_ParserCallbacks;

		private readonly Queue<string> m_QueryQueue;
		private readonly Queue<string> m_ExecuteQueue;
		private readonly SafeCriticalSection m_CommandQueuesSection;

		private readonly SafeTimer m_CommandTimer;

		private readonly ComSpecProperties m_ComSpecProperties;
		private readonly SecureNetworkProperties m_NetworkProperties;

		private readonly ConnectionStateManager m_ConnectionStateManager;

		private bool m_Initialized;

		// Used with settings.
		private string m_Config;

		#region Properties

		/// <summary>
		/// Username for logging in to the device.
		/// </summary>
		[PublicAPI]
		public string Username { get; set; }

		/// <summary>
		/// Device Initialized Status.
		/// </summary>
		[PublicAPI]
		public bool Initialized
		{
			get { return m_Initialized; }
			private set
			{
				if (value == m_Initialized)
					return;

				m_Initialized = value;

				OnInitializedChanged.Raise(this, new BoolEventArgs(m_Initialized));
			}
		}

		/// <summary>
		/// Returns true when the device is connected.
		/// </summary>
		[PublicAPI]
		public bool IsConnected
		{
			get { return m_ConnectionStateManager.IsConnected; }
		}

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractLutronNwkDevice()
		{
			m_ComSpecProperties = new ComSpecProperties();
			m_NetworkProperties = new SecureNetworkProperties();

			m_ParserCallbacks = new Dictionary<string, IcdHashSet<ParserCallback>>();

			m_QueryQueue = new Queue<string>();
			m_ExecuteQueue = new Queue<string>();
			m_CommandQueuesSection = new SafeCriticalSection();

			m_CommandTimer = new SafeTimer(CommandTimerCallback, COMMUNICATION_INTERVAL_MILLISECONDS, COMMUNICATION_INTERVAL_MILLISECONDS);

			m_SerialBuffer = new LutronQuantumNwkSerialBuffer();
			Subscribe(m_SerialBuffer);

			m_ConnectionStateManager = new ConnectionStateManager(this) {ConfigurePort = ConfigurePort};
			m_ConnectionStateManager.OnConnectedStateChanged += PortOnConnectionStatusChanged;
			m_ConnectionStateManager.OnIsOnlineStateChanged += PortOnIsOnlineStateChanged;
			m_ConnectionStateManager.OnSerialDataReceived += PortOnSerialDataReceived;
		}

		#region Methods

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			OnInitializedChanged = null;
			OnConnectedStateChanged = null;
			OnRoomOccupancyChanged = null;
			OnRoomPresetChanged = null;
			OnRoomLoadLevelChanged = null;
			OnRoomControlsChanged = null;

			base.DisposeFinal(disposing);

			m_CommandTimer.Dispose();

			ClearIntegrations();

			m_ConnectionStateManager.OnConnectedStateChanged -= PortOnConnectionStatusChanged;
			m_ConnectionStateManager.OnIsOnlineStateChanged -= PortOnIsOnlineStateChanged;
			m_ConnectionStateManager.OnSerialDataReceived -= PortOnSerialDataReceived;
			m_ConnectionStateManager.Dispose();
		}

		/// <summary>
		/// Sets the port for communicating with the device.
		/// </summary>
		/// <param name="port"></param>
		[PublicAPI]
		public void SetPort(ISerialPort port)
		{
			m_ConnectionStateManager.SetPort(port, false);
		}

		/// <summary>
		/// Configures the given port for communication with the device.
		/// </summary>
		/// <param name="port"></param>
		private void ConfigurePort(IPort port)
		{
			// Com
			if (port is IComPort)
				(port as IComPort).ApplyDeviceConfiguration(m_ComSpecProperties);

			// Network (TCP, UDP, SSH)
			if (port is ISecureNetworkPort)
				(port as ISecureNetworkPort).ApplyDeviceConfiguration(m_NetworkProperties);
			else if (port is INetworkPort)
				(port as INetworkPort).ApplyDeviceConfiguration(m_NetworkProperties);
		}

		/// <summary>
		/// Parses the XML file at the given path for configuration data.
		/// </summary>
		/// <param name="path"></param>
		[PublicAPI]
		public void LoadIntegrationConfig(string path)
		{
			m_Config = path;

			string fullPath = PathUtils.GetDefaultConfigPath("Lutron", path);
			string xml;

			try
			{
				xml = IcdFile.ReadToEnd(fullPath, new UTF8Encoding(false));
				xml = EncodingUtils.StripUtf8Bom(xml);
			}
			catch (Exception e)
			{
				Logger.Log(eSeverity.Error, "Failed to load integration config {0} - {1}", fullPath, e.Message);
				return;
			}

			ParseXml(xml);
		}

		#endregion

		#region Internal Methods

		/// <summary>
		/// Enqueue the data string to be sent to the device.
		/// </summary>
		/// <param name="data"></param>
		public void EnqueueData(string data)
		{
			m_CommandQueuesSection.Enter();

			try
			{
				switch (LutronUtils.GetMode(data))
				{
					case LutronUtils.MODE_QUERY:
						m_QueryQueue.Enqueue(data);
						break;

					case LutronUtils.MODE_EXECUTE:
						m_ExecuteQueue.Enqueue(data);
						break;

					default:
						string message = string.Format("Unexpected mode {0}", LutronUtils.GetMode(data));
						throw new ArgumentOutOfRangeException("data", message);
				}
			}
			finally
			{
				m_CommandQueuesSection.Leave();
			}
		}

		/// <summary>
		/// Gets the area integrations for the given room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public abstract IEnumerable<ILutronRoomContainer> GetRoomContainersForRoom(int room);

		/// <summary>
		/// Sends the data to the device.
		/// </summary>
		/// <param name="data"></param>
		private void SendData(string data)
		{
			if (!IsConnected)
			{
				Logger.Log(eSeverity.Critical, "Unable to communicate with device");

				m_CommandQueuesSection.Enter();

				try
				{
					m_QueryQueue.Clear();
					m_ExecuteQueue.Clear();
				}
				finally
				{
					m_CommandQueuesSection.Leave();
				}

				return;
			}

			m_ConnectionStateManager.Send(data);
		}

		/// <summary>
		/// Registers a callback for handling feedback from the lighting processor.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="callback"></param>
		public void RegisterIntegrationCallback(string key, ParserCallback callback)
		{
			if (!m_ParserCallbacks.ContainsKey(key))
				m_ParserCallbacks[key] = new IcdHashSet<ParserCallback>();

			m_ParserCallbacks[key].Add(callback);
		}

		/// <summary>
		/// Unregisters a callback delegate.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="callback"></param>
		public void UnregisterIntegrationCallback(string key, ParserCallback callback)
		{
			if (m_ParserCallbacks.ContainsKey(key))
				m_ParserCallbacks[key].Remove(callback);
		}

		#endregion

		#region Protected Event Raisers

		protected void RaiseRoomControlsChangedEvent(int roomId)
		{
			OnRoomControlsChanged.Raise(this, new IntEventArgs(roomId));
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return m_ConnectionStateManager != null && m_ConnectionStateManager.IsOnline;
		}

		/// <summary>
		/// Initialize the device.
		/// </summary>
		private void Initialize()
		{
			Initialized = true;
		}

		/// <summary>
		/// Called periodically to send a new data string to the processor.
		/// </summary>
		private void CommandTimerCallback()
		{
			// If not logged in, don't send data
			if (!m_ConnectionIsReady)
				return;

			string data = null;

			m_CommandQueuesSection.Enter();
			{
				if (m_ExecuteQueue.Count > 0)
					data = m_ExecuteQueue.Dequeue();
				else if (m_QueryQueue.Count > 0)
					data = m_QueryQueue.Dequeue();
			}
			m_CommandQueuesSection.Leave();

			if (!string.IsNullOrEmpty(data))
				SendData(data);
		}

		#endregion

		#region ILutronRoomContainer Callbacks

		protected virtual void Subscribe(ILutronRoomContainer room)
		{
			if (room == null)
				return;

			room.OnSceneChange += RoomOnSceneChange;
			room.OnOccupancyStateChanged += RoomOnOccupancyStateChanged;
			room.OnZoneOutputLevelChanged += RoomOnZoneOutputLevelChanged;
		}

		protected virtual void Unsubscribe(ILutronRoomContainer room)
		{
			if (room == null)
				return;

			room.OnSceneChange -= RoomOnSceneChange;
			room.OnOccupancyStateChanged -= RoomOnOccupancyStateChanged;
			room.OnZoneOutputLevelChanged -= RoomOnZoneOutputLevelChanged;
		}

		private void RoomOnSceneChange(object sender, GenericEventArgs<int?> args)
		{
			ILutronRoomContainer room = sender as ILutronRoomContainer;
			if (room != null)
				OnRoomPresetChanged.Raise(this, new RoomPresetChangeEventArgs(room.Room, args.Data));
		}

		private void RoomOnOccupancyStateChanged(object sender, OccupancyStateEventArgs args)
		{
			ILutronRoomContainer room = sender as ILutronRoomContainer;
			if (room != null)
				OnRoomOccupancyChanged.Raise(this, new RoomOccupancyEventArgs(room.Room, GetOccupancyState(args.Data)));
		}

		private void RoomOnZoneOutputLevelChanged(object sender, ZoneOutputLevelEventArgs args)
		{
			ILutronRoomContainer room = sender as ILutronRoomContainer;
			if (room != null)
				OnRoomLoadLevelChanged.Raise(this, new RoomLoadLevelEventArgs(room.Room, args.IntegrationId, args.Percentage));
		}
		
		#endregion

		#region Port Callbacks

		/// <summary>
		/// Called when serial data is recieved from the port.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void PortOnSerialDataReceived(object sender, StringEventArgs args)
		{
			m_SerialBuffer.Enqueue(args.Data);
		}

		/// <summary>
		/// Called when the port connection status changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void PortOnConnectionStatusChanged(object sender, BoolEventArgs args)
		{
			if (args.Data)
				Initialize();
			else
			{
				Logger.Log(eSeverity.Critical, "Lost connection");
				m_ConnectionIsReady = false;
				Initialized = false;
			}

			OnConnectedStateChanged.Raise(this, new BoolEventArgs(args.Data));
		}

		/// <summary>
		/// Called when the port online status changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void PortOnIsOnlineStateChanged(object sender, BoolEventArgs eventArgs)
		{
			UpdateCachedOnlineStatus();
		}

		#endregion

		#region Buffer Callbacks

		/// <summary>
		/// Subscribes to the buffer events.
		/// </summary>
		/// <param name="buffer"></param>
		private void Subscribe(ISerialBuffer buffer)
		{
			buffer.OnCompletedSerial += SerialBufferCompletedSerial;
		}

		/// <summary>
		/// Called when the buffer completes a string.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void SerialBufferCompletedSerial(object sender, StringEventArgs args)
		{
			string data = args.Data.Trim();
			if (string.IsNullOrEmpty(data))
				return;

			if (data == LutronUtils.QNET || data == LutronUtils.LOGIN_SUCCESS)
			{
				m_ConnectionIsReady = true;
				return;
			}

			if (data == LutronUtils.CRLF)
				return;

			if (data.ToLower().Contains(LutronUtils.LOGIN_PROMPT))
			{
				SendData((Username ?? string.Empty) + LutronUtils.CRLF);
				return;
			}

			string command = LutronUtils.GetCommand(data);
			if (command == LutronUtils.COMMAND_ERROR)
			{
				ParseError(data);
				return;
			}

			ParseResponse(data);
		}

		/// <summary>
		/// Handles error responses.
		/// </summary>
		/// <param name="data"></param>
		private void ParseError(string data)
		{
			int id;
			string message;
			try
			{
				 id = int.Parse(LutronUtils.GetIntegrationId(data));
			}
			catch (FormatException)
			{
				message = String.Format("Couldn't get error code for  data {0}", data);
				Logger.Log(eSeverity.Error, message);
				return;
			}
			

			switch (id)
			{
				case LutronUtils.ERROR_PARAMETER_COUNT:
					message = "Unexpected parameter count";
					break;

				case LutronUtils.ERROR_OBJECT_DOES_NOT_EXIST:
					message = "Object does not exist";
					break;

				case LutronUtils.ERROR_INVALID_ACTION_NUMBER:
					message = "Invalid action number";
					break;

				case LutronUtils.ERROR_PARAMETER_OUT_OF_RANGE:
					message = "Parameter out of range";
					break;

				case LutronUtils.ERROR_PARAMETER_MALFORMED:
					message = "Parameter malformed";                    
					break;

				case LutronUtils.ERROR_UNSUPPORTED_COMMAND:
					message = "Unsupported command";
					break;

				default:
					message = string.Format("Unknown error code {0}", id);
					break;
			}

			Logger.Log(eSeverity.Error, message);
		}

		/// <summary>
		/// Executes the callbacks that have been registered with the given key.
		/// </summary>
		/// <param name="data"></param>
		private void ParseResponse(string data)
		{
			string key = LutronUtils.GetKeyFromData(data);
			if (!m_ParserCallbacks.ContainsKey(key))
				return;

			foreach (ParserCallback callback in m_ParserCallbacks[key])
			{
				try
				{
					callback(this, data);
				}
				catch (Exception e)
				{
					IcdErrorLog.Exception(e, "Exception calling parser delegate - {0}", e.Message);
				}
			}
		}

		#endregion

		#region Parse Xml

		/// <summary>
		/// Parses the xml document for areas, zones, devices, etc.
		/// </summary>
		/// <param name="xml"></param>
		protected abstract void ParseXml(string xml);

		/// <summary>
		/// Removes all of the existing integrations from the device.
		/// </summary>
		protected abstract void ClearIntegrations();

		#endregion

		#region Settings

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(T settings)
		{
			base.CopySettingsFinal(settings);

			settings.IntegrationConfig = m_Config;
			settings.Username = Username;

			settings.Port = m_ConnectionStateManager.PortNumber;

			settings.Copy(m_ComSpecProperties);
			settings.Copy(m_NetworkProperties);
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			m_Config = null;
			Username = null;

			m_ComSpecProperties.ClearComSpecProperties();
			m_NetworkProperties.ClearNetworkProperties();

			SetPort(null);
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(T settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			m_ComSpecProperties.Copy(settings);
			m_NetworkProperties.Copy(settings);

			Username = settings.Username;

			ISerialPort port = null;

			if (settings.Port != null)
			{
				try
				{
					port = factory.GetPortById((int)settings.Port) as ISerialPort;
				}
				catch (KeyNotFoundException)
				{
					Logger.Log(eSeverity.Error, "No serial Port with id {0}", settings.Port);
				}
			}

			SetPort(port);

			// Load the integrations
			if (!string.IsNullOrEmpty(settings.IntegrationConfig))
				LoadIntegrationConfig(settings.IntegrationConfig);
		}

		/// <summary>
		/// Override to add actions on StartSettings
		/// This should be used to start communications with devices and perform initial actions
		/// </summary>
		protected override void StartSettingsFinal()
		{
			base.StartSettingsFinal();

			m_ConnectionStateManager.Start();
		}

		#endregion

		/// <summary>
		/// Converts a Lutron occupancy state to a lighting processor device occupancy state.
		/// </summary>
		/// <param name="occupancy"></param>
		/// <returns></returns>
		protected static Partitioning.Commercial.Controls.Occupancy.eOccupancyState GetOccupancyState(eOccupancyState occupancy)
		{
			switch (occupancy)
			{
				case eOccupancyState.Unknown:
					return Partitioning.Commercial.Controls.Occupancy.eOccupancyState.Unknown;
				case eOccupancyState.Inactive:
					return Partitioning.Commercial.Controls.Occupancy.eOccupancyState.Unknown;
				case eOccupancyState.Occupied:
					return Partitioning.Commercial.Controls.Occupancy.eOccupancyState.Occupied;
				case eOccupancyState.Unoccupied:
					return Partitioning.Commercial.Controls.Occupancy.eOccupancyState.Unoccupied;
				default:
					throw new ArgumentOutOfRangeException("occupancy", "Unexpected eOccupancyState " + occupancy);
			}
		}

		#region Console
		
		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			foreach (IConsoleNodeBase node in GetBaseConsoleNodes())
				yield return node;

			if (m_ConnectionStateManager != null)
				yield return m_ConnectionStateManager.Port;
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleNodeBase> GetBaseConsoleNodes()
		{
			return base.GetConsoleNodes();
		}
		#endregion
	}
}
