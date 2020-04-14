using ICD.Common.Utils.Xml;
using ICD.Connect.Lighting.Processors;
using ICD.Connect.Settings.Attributes;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Lighting.RoomInterface
{
	[KrangSettings("LightingRoomInterface", typeof(LightingRoomInterfaceDevice))]
	public sealed class LightingRoomInterfaceDeviceSettings : AbstractLightingRoomInterfaceDeviceSettings
	{
		private const string LIGHTING_PROCESSOR_ELEMENT = "LightingProcessor";
		private const string ROOM_ID_ELEMENT = "RoomId";
		

		[OriginatorIdSettingsProperty(typeof(ILightingProcessorDevice))]
		public int LightingProcessorDeviceId { get; set; }

		public int LightingRoomId { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(LIGHTING_PROCESSOR_ELEMENT, IcdXmlConvert.ToString(LightingProcessorDeviceId));
			writer.WriteElementString(ROOM_ID_ELEMENT, IcdXmlConvert.ToString(LightingRoomId));
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			LightingProcessorDeviceId = XmlUtils.ReadChildElementContentAsInt(xml, LIGHTING_PROCESSOR_ELEMENT);
			LightingRoomId = XmlUtils.ReadChildElementContentAsInt(xml, ROOM_ID_ELEMENT);
		}
	}
}
