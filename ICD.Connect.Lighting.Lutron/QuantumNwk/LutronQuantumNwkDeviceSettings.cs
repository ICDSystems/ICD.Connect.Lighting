using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Settings.Attributes;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Lighting.Lutron.QuantumNwk
{
	[KrangSettings("LutronQuantumNwk", typeof(LutronQuantumNwkDevice))]
	public sealed class LutronQuantumNwkDeviceSettings : AbstractDeviceSettings
	{
		private const string PORT_ELEMENT = "Port";
		private const string USERNAME_ELEMENT = "Username";
		private const string INTEGRATION_CONFIG_ELEMENT = "IntegrationConfig";

		#region Properties

		[PathSettingsProperty("Lutron", ".xml")]
		public string IntegrationConfig { get; set; }

		[OriginatorIdSettingsProperty(typeof(ISerialPort))]
		public int? Port { get; set; }

		public string Username { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(PORT_ELEMENT, IcdXmlConvert.ToString(Port));
			writer.WriteElementString(USERNAME_ELEMENT, Username);
			writer.WriteElementString(INTEGRATION_CONFIG_ELEMENT, IntegrationConfig);
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			Port = XmlUtils.TryReadChildElementContentAsInt(xml, PORT_ELEMENT);
			Username = XmlUtils.TryReadChildElementContentAsString(xml, USERNAME_ELEMENT);
			IntegrationConfig = XmlUtils.TryReadChildElementContentAsString(xml, INTEGRATION_CONFIG_ELEMENT);
		}

		#endregion
	}
}
