using System;
using ICD.Common.Attributes.Properties;
using ICD.Common.Properties;
using ICD.Common.Utils.Xml;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Attributes.Factories;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Lighting.Lutron.QuantumNwk
{
	public sealed class LutronQuantumNwkDeviceSettings : AbstractDeviceSettings
	{
		private const string FACTORY_NAME = "LutronQuantumNwk";

		private const string PORT_ELEMENT = "Port";
		private const string USERNAME_ELEMENT = "Username";
		private const string INTEGRATION_CONFIG_ELEMENT = "IntegrationConfig";

		#region Properties

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(LutronQuantumNwkDevice); } }

		public string IntegrationConfig { get; set; }

		[SettingsProperty(SettingsProperty.ePropertyType.PortId)]
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

			if (Port != null)
				writer.WriteElementString(PORT_ELEMENT, IcdXmlConvert.ToString((int)Port));

			if (!string.IsNullOrEmpty(Username))
				writer.WriteElementString(USERNAME_ELEMENT, Username);

			if (!string.IsNullOrEmpty(IntegrationConfig))
				writer.WriteElementString(INTEGRATION_CONFIG_ELEMENT, IntegrationConfig);
		}

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlDeviceSettingsFactoryMethod(FACTORY_NAME)]
		public static LutronQuantumNwkDeviceSettings FromXml(string xml)
		{
			int? port = XmlUtils.TryReadChildElementContentAsInt(xml, PORT_ELEMENT);
			string username = XmlUtils.TryReadChildElementContentAsString(xml, USERNAME_ELEMENT);
			string integrationConfig = XmlUtils.TryReadChildElementContentAsString(xml, INTEGRATION_CONFIG_ELEMENT);

			LutronQuantumNwkDeviceSettings output = new LutronQuantumNwkDeviceSettings
			{
				Port = port,
				Username = username,
				IntegrationConfig = integrationConfig
			};

			ParseXml(output, xml);
			return output;
		}

		#endregion
	}
}
