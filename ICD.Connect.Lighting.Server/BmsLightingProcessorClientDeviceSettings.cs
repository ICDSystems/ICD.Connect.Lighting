using System;
using ICD.Common.Properties;
using ICD.Common.Utils.Xml;
using ICD.Connect.Lighting;
using ICD.Connect.Settings.Attributes;
using ICD.Connect.Settings.Attributes.Factories;

namespace ICD.SimplSharp.BmsOS.Devices.Lighting
{
	/// <summary>
	/// Settings for the BmsLightingProcessorClientDevice.
	/// </summary>
	public sealed class BmsLightingProcessorClientDeviceSettings : AbstractLightingProcessorDeviceSettings
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
		public override Type OriginatorType { get { return typeof(BmsLightingProcessorClientDevice); } }

		[SettingsProperty(SettingsProperty.ePropertyType.PortId)]
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
		[PublicAPI, XmlDeviceSettingsFactoryMethod(FACTORY_NAME)]
		public static BmsLightingProcessorClientDeviceSettings FromXml(string xml)
		{
			int? port = XmlUtils.TryReadChildElementContentAsInt(xml, PORT_ELEMENT);
			int roomId = XmlUtils.TryReadChildElementContentAsInt(xml, ROOM_ID_ELEMENT) ?? 0;

			BmsLightingProcessorClientDeviceSettings output = new BmsLightingProcessorClientDeviceSettings
			{
				Port = port,
				RoomId = roomId
			};

			ParseXml(output, xml);
			return output;
		}
	}
}
