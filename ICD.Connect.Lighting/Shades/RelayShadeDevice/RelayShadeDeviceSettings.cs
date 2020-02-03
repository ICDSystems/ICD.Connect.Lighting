using ICD.Common.Utils.Xml;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Lighting.Shades.RelayShadeDevice
{
	[KrangSettings("RelayShadeDevice", typeof(RelayShadeDevice))]
	public sealed class RelayShadeDeviceSettings : AbstractShadeDeviceSettings
	{
		private const string OPEN_RELAY_ELEMENT = "OpenRelay";
		private const string CLOSE_RELAY_ELEMENT = "CloseRelay";
		private const string RELAY_CLOSE_TIME_ELEMENT = "RelayCloseTime";

		public int? OpenRelay { get; set; }
		public int? CloseRelay { get; set; }
		public long? RelayCloseTime { get; set; }

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			OpenRelay = XmlUtils.TryReadChildElementContentAsInt(xml, OPEN_RELAY_ELEMENT);
			CloseRelay = XmlUtils.TryReadChildElementContentAsInt(xml, CLOSE_RELAY_ELEMENT);
			RelayCloseTime = XmlUtils.TryReadChildElementContentAsLong(xml, RELAY_CLOSE_TIME_ELEMENT);
		}

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(OPEN_RELAY_ELEMENT, IcdXmlConvert.ToString(OpenRelay));
			writer.WriteElementString(CLOSE_RELAY_ELEMENT, IcdXmlConvert.ToString(CloseRelay));
			writer.WriteElementString(RELAY_CLOSE_TIME_ELEMENT, IcdXmlConvert.ToString(RelayCloseTime));
		}
	}
}