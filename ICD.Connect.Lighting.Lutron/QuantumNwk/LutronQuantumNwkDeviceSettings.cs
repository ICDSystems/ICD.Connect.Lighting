﻿using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;
using ICD.Connect.Protocol.Network.Settings;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.Ports.ComPort;
using ICD.Connect.Protocol.Settings;
using ICD.Connect.Settings.Attributes;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Lighting.Lutron.QuantumNwk
{
	[KrangSettings("LutronQuantumNwk", typeof(LutronQuantumNwkDevice))]
	public sealed class LutronQuantumNwkDeviceSettings : AbstractDeviceSettings, INetworkProperties, IComSpecProperties
	{
		private const string PORT_ELEMENT = "Port";
		private const string INTEGRATION_CONFIG_ELEMENT = "IntegrationConfig";

		private readonly NetworkProperties m_NetworkProperties;
		private readonly ComSpecProperties m_ComSpecProperties;

		#region Properties

		[PathSettingsProperty("Lutron", ".xml")]
		public string IntegrationConfig { get; set; }

		[OriginatorIdSettingsProperty(typeof(ISerialPort))]
		public int? Port { get; set; }

		#endregion

		#region Network

		/// <summary>
		/// Gets/sets the configurable username.
		/// </summary>
		public string Username { get { return m_NetworkProperties.Username; } set { m_NetworkProperties.Username = value; } }

		/// <summary>
		/// Gets/sets the configurable password.
		/// </summary>
		public string Password { get { return m_NetworkProperties.Password; } set { m_NetworkProperties.Password = value; } }

		/// <summary>
		/// Gets/sets the configurable network address.
		/// </summary>
		public string NetworkAddress
		{
			get { return m_NetworkProperties.NetworkAddress; }
			set { m_NetworkProperties.NetworkAddress = value; }
		}

		/// <summary>
		/// Gets/sets the configurable network port.
		/// </summary>
		public ushort NetworkPort
		{
			get { return m_NetworkProperties.NetworkPort; }
			set { m_NetworkProperties.NetworkPort = value; }
		}

		#endregion

		#region Com Spec

		/// <summary>
		/// Gets/sets the configurable baud rate.
		/// </summary>
		public eComBaudRates ComSpecBaudRate
		{
			get { return m_ComSpecProperties.ComSpecBaudRate; }
			set { m_ComSpecProperties.ComSpecBaudRate = value; }
		}

		/// <summary>
		/// Gets/sets the configurable number of data bits.
		/// </summary>
		public eComDataBits ComSpecNumberOfDataBits
		{
			get { return m_ComSpecProperties.ComSpecNumberOfDataBits; }
			set { m_ComSpecProperties.ComSpecNumberOfDataBits = value; }
		}

		/// <summary>
		/// Gets/sets the configurable parity type.
		/// </summary>
		public eComParityType ComSpecParityType
		{
			get { return m_ComSpecProperties.ComSpecParityType; }
			set { m_ComSpecProperties.ComSpecParityType = value; }
		}

		/// <summary>
		/// Gets/sets the configurable number of stop bits.
		/// </summary>
		public eComStopBits ComSpecNumberOfStopBits
		{
			get { return m_ComSpecProperties.ComSpecNumberOfStopBits; }
			set { m_ComSpecProperties.ComSpecNumberOfStopBits = value; }
		}

		/// <summary>
		/// Gets/sets the configurable protocol type.
		/// </summary>
		public eComProtocolType ComSpecProtocolType
		{
			get { return m_ComSpecProperties.ComSpecProtocolType; }
			set { m_ComSpecProperties.ComSpecProtocolType = value; }
		}

		/// <summary>
		/// Gets/sets the configurable hardware handshake type.
		/// </summary>
		public eComHardwareHandshakeType ComSpecHardwareHandShake
		{
			get { return m_ComSpecProperties.ComSpecHardwareHandShake; }
			set { m_ComSpecProperties.ComSpecHardwareHandShake = value; }
		}

		/// <summary>
		/// Gets/sets the configurable software handshake type.
		/// </summary>
		public eComSoftwareHandshakeType ComSpecSoftwareHandshake
		{
			get { return m_ComSpecProperties.ComSpecSoftwareHandshake; }
			set { m_ComSpecProperties.ComSpecSoftwareHandshake = value; }
		}

		/// <summary>
		/// Gets/sets the configurable report CTS changes state.
		/// </summary>
		public bool ComSpecReportCtsChanges
		{
			get { return m_ComSpecProperties.ComSpecReportCtsChanges; }
			set { m_ComSpecProperties.ComSpecReportCtsChanges = value; }
		}

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		public LutronQuantumNwkDeviceSettings()
		{
			m_NetworkProperties = new NetworkProperties
			{
				Username = "nwk",
				NetworkPort = 23
			};

			m_ComSpecProperties = new ComSpecProperties
			{
				ComSpecBaudRate = eComBaudRates.ComspecBaudRate115200,
				ComSpecNumberOfDataBits = eComDataBits.ComspecDataBits8,
				ComSpecParityType = eComParityType.ComspecParityNone,
				ComSpecNumberOfStopBits = eComStopBits.ComspecStopBits1,
				ComSpecProtocolType = eComProtocolType.ComspecProtocolRS232,
				ComSpecHardwareHandShake = eComHardwareHandshakeType.ComspecHardwareHandshakeNone,
				ComSpecSoftwareHandshake = eComSoftwareHandshakeType.ComspecSoftwareHandshakeNone,
				ComSpecReportCtsChanges = false
			};
		}

		#region Methods

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(PORT_ELEMENT, IcdXmlConvert.ToString(Port));
			writer.WriteElementString(INTEGRATION_CONFIG_ELEMENT, IntegrationConfig);

			m_NetworkProperties.WriteElements(writer);
			m_ComSpecProperties.WriteElements(writer);
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			Port = XmlUtils.TryReadChildElementContentAsInt(xml, PORT_ELEMENT);
			IntegrationConfig = XmlUtils.TryReadChildElementContentAsString(xml, INTEGRATION_CONFIG_ELEMENT);

			m_NetworkProperties.ParseXml(xml);
			m_ComSpecProperties.ParseXml(xml);
		}

		#endregion
	}
}
