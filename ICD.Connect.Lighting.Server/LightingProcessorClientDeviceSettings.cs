using System;
using ICD.Common.Utils.Xml;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Settings.Attributes;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Lighting.Server
{
	/// <summary>
	/// Settings for the BmsLightingProcessorClientDevice.
	/// </summary>
	[KrangSettings(FACTORY_NAME)]
	public sealed class LightingProcessorClientDeviceSettings : AbstractLightingProcessorDeviceSettings
	{
		private const string FACTORY_NAME = "BmsLightingProcessorClient";

		private const string PORT_ELEMENT = "Port";
		private const string ROOM_ID_ELEMENT = "RoomId";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(LightingProcessorClientDevice); } }

		[OriginatorIdSettingsProperty(typeof(ISerialPort))]
		public int? Port { get; set; }

		/// <summary>
		/// This will be removed once we figure out the scope of the room framework.
		/// </summary>
		public int RoomId { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(PORT_ELEMENT, IcdXmlConvert.ToString(Port));
			writer.WriteElementString(ROOM_ID_ELEMENT, IcdXmlConvert.ToString(RoomId));
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
		}
	}
}
