using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Lighting.EventArguments;
using ICD.Connect.Lighting.RoomInterface;
using ICD.Connect.Partitioning.Commercial.Controls.Occupancy;
using ICD.Connect.Protocol;
using ICD.Connect.Protocol.Network.Ports;
using ICD.Connect.Protocol.Network.Settings;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.Ports.ComPort;
using ICD.Connect.Protocol.Settings;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Originators;

namespace ICD.Connect.Lighting.SerialLightingRoomInterfaceDevice
{
	public sealed class SerialLightingRoomInterfaceDevice : AbstractLightingRoomInterfaceDevice<SerialLightingRoomInterfaceDeviceSettings>
	{
		#region Fields

		private readonly Dictionary<int, PresetSerialLightingControl> m_Presets;
		private readonly SafeCriticalSection m_PresetsSection;
		private readonly ConnectionStateManager m_ConnectionManager;
		private readonly ComSpecProperties m_ComSpecProperties;
		private readonly SecureNetworkProperties m_NetworkProperties;

		#endregion

		#region Properties

		/// <summary>
		/// Properties to use for Com ports
		/// </summary>
		[NotNull]
		private ComSpecProperties ComSpecProperties{get { return m_ComSpecProperties; }}

		/// <summary>
		/// Properties to use for Network ports
		/// </summary>
		[NotNull]
		private SecureNetworkProperties NetworkProperties {get { return m_NetworkProperties; }}

		/// <summary>
		/// Port to use for communciations
		/// </summary>
		[CanBeNull]
		private ISerialPort Port { get; set; }

		#endregion

		#region Events       

		public override event EventHandler<GenericEventArgs<eOccupancyState>> OnOccupancyChanged;
		public override event EventHandler<GenericEventArgs<int?>> OnPresetChanged;
		public override event EventHandler<LoadLevelEventArgs> OnLoadLevelChanged;
		public override event EventHandler OnControlsChanged;

		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		public SerialLightingRoomInterfaceDevice()
		{
			m_Presets = new Dictionary<int, PresetSerialLightingControl>();
			m_PresetsSection = new SafeCriticalSection();
			m_NetworkProperties = new SecureNetworkProperties();
			m_ComSpecProperties = new ComSpecProperties();
			m_ConnectionManager = new ConnectionStateManager(this)
			{
				ConfigurePort = ConfigurePort
			};

			m_ConnectionManager.OnConnectedStateChanged += ConnectionManagerOnConnectedStateChanged;
		}

		#region Methods

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return m_ConnectionManager.IsConnected;
		}

		/// <summary>
		/// Set the port to use for communications
		/// </summary>
		/// <param name="port"></param>
		private void SetPort(ISerialPort port)
		{
			Port = port;
			m_ConnectionManager.SetPort(port, LifecycleState == eLifecycleState.Started);
		}

		/// <summary>
		/// Send the serial string to the device
		/// </summary>
		/// <param name="data"></param>
		private void SendData(string data)
		{
			if (Port == null)
			{
				Logger.Log(eSeverity.Error, "Could not send lighting command, no port configured");
				return;
			}
			if (!Port.IsConnected)
			{
				Logger.Log(eSeverity.Error, "Could not send lighting command, port disconnected");
				return;
			}

			Port.Send(data);
		}

		#endregion

		#region Settings

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(SerialLightingRoomInterfaceDeviceSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			NetworkProperties.Copy(settings.NetworkProperties);
			ComSpecProperties.Copy(settings.ComSpecProperties);

			if (settings.Port.HasValue)
				SetPort(factory.GetOriginatorById<ISerialPort>(settings.Port.Value));
			else
			{
				SetPort(null);
			}

			m_PresetsSection.Enter();
			try
			{
				m_Presets.Clear();
				m_Presets.AddRange(settings.Presets, p => p.Index);
			}
			finally
			{
				m_PresetsSection.Leave();
			}
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			NetworkProperties.ClearNetworkProperties();
			ComSpecProperties.ClearComSpecProperties();

			SetPort(null);

			m_PresetsSection.Execute(() => m_Presets.Clear());
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(SerialLightingRoomInterfaceDeviceSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.NetworkProperties.Copy(NetworkProperties);
			settings.ComSpecProperties.Copy(ComSpecProperties);

			settings.Port = Port == null ? (int?)null : Port.Id;

			PresetSerialLightingControl[] presets = {};
			m_PresetsSection.Execute(() => presets = m_Presets.Values.ToArray(m_Presets.Count));
			
			settings.SetPresets(presets);
		}

		/// <summary>
		/// Override to add actions on StartSettings
		/// This should be used to start communications with devices and perform initial actions
		/// </summary>
		protected override void StartSettingsFinal()
		{
			base.StartSettingsFinal();

			m_ConnectionManager.Start();
		}


		#endregion

		#region Lighting Room Interface

		/// <summary>
		/// Gets the available light loads for the room.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<LightingProcessorControl> GetLoads()
		{
			return Enumerable.Empty<LightingProcessorControl>();
		}

		/// <summary>
		/// Gets the available individual shades for the room.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<LightingProcessorControl> GetShades()
		{
			return Enumerable.Empty<LightingProcessorControl>();
		}

		/// <summary>
		/// Gets the available shade groups for the room.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<LightingProcessorControl> GetShadeGroups()
		{
			return Enumerable.Empty<LightingProcessorControl>();
		}

		/// <summary>
		/// Gets the available presets for the room.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<LightingProcessorControl> GetPresets()
		{
			return
				m_PresetsSection.Execute(() => m_Presets.Values.Select(p => p.ToLightingProcessorControl()).ToArray(m_Presets.Count));
		}

		/// <summary>
		/// Gets the current occupancy state for the given room.
		/// </summary>
		/// <returns></returns>
		public override eOccupancyState GetOccupancy()
		{
			return eOccupancyState.Unknown;
		}

		/// <summary>
		/// Sets the preset for the given room.
		/// </summary>
		/// <param name="preset"></param>
		public override void SetPreset(int? preset)
		{
			if (!preset.HasValue)
				throw new ArgumentOutOfRangeException("preset", "This device doesn't support setting null presets");

			PresetSerialLightingControl lightingPreset;
			
			m_PresetsSection.Enter();
			try
			{
				if (!m_Presets.TryGetValue(preset.Value, out lightingPreset))
					throw new ArgumentOutOfRangeException("preset", string.Format("No lighting preset with ID {0}", preset.Value));
			}
			finally
			{
				m_PresetsSection.Leave();
			}

			SendData(lightingPreset.SerialCommand);
		}

		/// <summary>
		/// Gets the current preset for the given room.
		/// </summary>
		public override int? GetPreset()
		{
			// Doesn't support getting presets
			return null;
		}

		/// <summary>
		/// Sets the lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		/// <param name="percentage"></param>
		public override void SetLoadLevel(int load, float percentage)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Gets the current lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		public override float GetLoadLevel(int load)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Starts raising the lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		public override void StartRaisingLoadLevel(int load)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Starts lowering the lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		public override void StartLoweringLoadLevel(int load)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Stops raising/lowering the lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		public override void StopRampingLoadLevel(int load)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Starts raising the shade.
		/// </summary>
		/// <param name="shade"></param>
		public override void StartRaisingShade(int shade)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Starts lowering the shade.
		/// </summary>
		/// <param name="shade"></param>
		public override void StartLoweringShade(int shade)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Stops moving the shade.
		/// </summary>
		/// <param name="shade"></param>
		public override void StopMovingShade(int shade)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Starts raising the shade group.
		/// </summary>
		/// <param name="shadeGroup"></param>
		public override void StartRaisingShadeGroup(int shadeGroup)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Starts lowering the shade group.
		/// </summary>
		/// <param name="shadeGroup"></param>
		public override void StartLoweringShadeGroup(int shadeGroup)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Stops moving the shade group.
		/// </summary>
		/// <param name="shadeGroup"></param>
		public override void StopMovingShadeGroup(int shadeGroup)
		{
			throw new NotSupportedException();
		}

		#endregion

		

		#region Connection State Manager Callbacks

		/// <summary>
		/// Raised when the OnConnected state changes for the port
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="boolEventArgs"></param>
		private void ConnectionManagerOnConnectedStateChanged(object sender, BoolEventArgs boolEventArgs)
		{
			UpdateCachedOnlineStatus();
		}

		/// <summary>
		/// Configures the port based on the device's settings
		/// </summary>
		/// <param name="port"></param>
		private void ConfigurePort(IPort port)
		{
			// Com
			if (port is IComPort)
				(port as IComPort).ApplyDeviceConfiguration(ComSpecProperties);

			// Network (TCP, UDP, SSH)
			if (port is ISecureNetworkPort)
				(port as ISecureNetworkPort).ApplyDeviceConfiguration(NetworkProperties);
			else if (port is INetworkPort)
				(port as INetworkPort).ApplyDeviceConfiguration(NetworkProperties);
		}

		#endregion
	}
}