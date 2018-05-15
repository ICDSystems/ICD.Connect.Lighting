using ICD.Common.Utils.Xml;
using ICD.Connect.Lighting.Processors;
using ICD.Connect.Protocol.Network.Settings;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Settings.Attributes;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Lighting.Server
{
	/// <summary>
	/// Settings for the BmsLightingProcessorClientDevice.
	/// </summary>
	[KrangSettings("BmsLightingProcessorClient", typeof(LightingProcessorClientDevice))]
	public sealed class LightingProcessorClientDeviceSettings : AbstractLightingProcessorDeviceSettings, INetworkSettings
	{
		private const string PORT_ELEMENT = "Port";
		private const string ROOM_ID_ELEMENT = "RoomId";

		private readonly NetworkProperties m_NetworkProperties;

		#region Properties

		[OriginatorIdSettingsProperty(typeof(ISerialPort))]
		public int? Port { get; set; }

		/// <summary>
		/// This will be removed once we figure out the scope of the room framework.
		/// </summary>
		public int RoomId { get; set; }

		/// <summary>
		/// Gets the configurable network properties.
		/// </summary>
		public INetworkProperties NetworkProperties { get { return m_NetworkProperties; } }

		#endregion

		#region Network

		/// <summary>
		/// Gets/sets the configurable network username.
		/// </summary>
		public string NetworkUsername
		{
			get { return m_NetworkProperties.NetworkUsername; }
			set { m_NetworkProperties.NetworkUsername = value; }
		}

		/// <summary>
		/// Gets/sets the configurable network password.
		/// </summary>
		public string NetworkPassword
		{
			get { return m_NetworkProperties.NetworkPassword; }
			set { m_NetworkProperties.NetworkPassword = value; }
		}

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

		/// <summary>
		/// Constructor.
		/// </summary>
		public LightingProcessorClientDeviceSettings()
		{
			m_NetworkProperties = new NetworkProperties();
		}

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(PORT_ELEMENT, IcdXmlConvert.ToString(Port));
			writer.WriteElementString(ROOM_ID_ELEMENT, IcdXmlConvert.ToString(RoomId));

			m_NetworkProperties.WriteElements(writer);
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			Port = XmlUtils.TryReadChildElementContentAsInt(xml, PORT_ELEMENT);
			RoomId = XmlUtils.TryReadChildElementContentAsInt(xml, ROOM_ID_ELEMENT) ?? 0;

			m_NetworkProperties.ParseXml(xml);
		}
	}
}
