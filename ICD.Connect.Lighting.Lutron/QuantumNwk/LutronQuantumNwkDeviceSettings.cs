﻿using System;
using ICD.Common.Properties;
using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Settings.Attributes;
using ICD.Connect.Settings.Attributes.SettingsProperties;

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
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlFactoryMethod(FACTORY_NAME)]
		public static LutronQuantumNwkDeviceSettings FromXml(string xml)
		{
			LutronQuantumNwkDeviceSettings output = new LutronQuantumNwkDeviceSettings
			{
				Port = XmlUtils.TryReadChildElementContentAsInt(xml, PORT_ELEMENT),
				Username = XmlUtils.TryReadChildElementContentAsString(xml, USERNAME_ELEMENT),
				IntegrationConfig = XmlUtils.TryReadChildElementContentAsString(xml, INTEGRATION_CONFIG_ELEMENT)
			};

			output.ParseXml(xml);
			return output;
		}

		#endregion
	}
}
