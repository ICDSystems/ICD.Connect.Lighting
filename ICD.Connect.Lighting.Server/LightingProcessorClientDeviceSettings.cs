using System;
using ICD.Common.Properties;
using ICD.Common.Utils.Xml;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Settings.Attributes;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Lighting.Server
{
	/// <summary>
	/// Settings for the BmsLightingProcessorClientDevice.
	/// </summary>
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

			if (Port != null)
				writer.WriteElementString(PORT_ELEMENT, IcdXmlConvert.ToString((int)Port));

			writer.WriteElementString(ROOM_ID_ELEMENT, IcdXmlConvert.ToString(RoomId));
		}

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlFactoryMethod(FACTORY_NAME)]
		public static LightingProcessorClientDeviceSettings FromXml(string xml)
		{
			int? port = XmlUtils.TryReadChildElementContentAsInt(xml, PORT_ELEMENT);
			int roomId = XmlUtils.TryReadChildElementContentAsInt(xml, ROOM_ID_ELEMENT) ?? 0;

			LightingProcessorClientDeviceSettings output = new LightingProcessorClientDeviceSettings
			{
				Port = port,
				RoomId = roomId
			};

			ParseXml(output, xml);
			return output;
		}
	}
}
