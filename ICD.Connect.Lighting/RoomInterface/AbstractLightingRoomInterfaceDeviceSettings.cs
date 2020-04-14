using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;

namespace ICD.Connect.Lighting.RoomInterface
{
	public abstract class AbstractLightingRoomInterfaceDeviceSettings : AbstractDeviceSettings, ILightingRoomInterfaceDeviceSettings
	{

		private const string OCCUPANCY_CONTROL_ELEMENT = "EnableOccupancyControl";

		/// <summary>
		/// If true, an occupancy sensor control will be added to the device
		/// Used for systems where occupancy sensors may not be  used,
		/// to prevent things like adding unnecessary Fusion Occupancy Sensor Assets.
		/// </summary>
		public bool EnableOccupancyControl { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(OCCUPANCY_CONTROL_ELEMENT, IcdXmlConvert.ToString(EnableOccupancyControl));
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			EnableOccupancyControl = XmlUtils.TryReadChildElementContentAsBoolean(xml, OCCUPANCY_CONTROL_ELEMENT) ?? false;
		}
	}
}