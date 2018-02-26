using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;

namespace ICD.Connect.Lighting.Shades
{
	public abstract class AbstractShadeDeviceSettings : AbstractDeviceSettings, IShadeDeviceSettings
	{
		private const string SHADE_TYPE_ELEMENT = "ShadeType";

		public eShadeType ShadeType { get; set; }

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			var shadeType = XmlUtils.TryReadChildElementContentAsEnum<eShadeType>(xml, SHADE_TYPE_ELEMENT, true);
			if (shadeType != null)
				ShadeType = shadeType.Value;
		}
	}
}