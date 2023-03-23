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

			ShadeType = XmlUtils.TryReadChildElementContentAsEnum<eShadeType>(xml, SHADE_TYPE_ELEMENT, true) ?? eShadeType.None;
		}

	    /// <summary>
	    /// Writes property elements to xml.
	    /// </summary>
	    /// <param name="writer"></param>
	    protected override void WriteElements(IcdXmlTextWriter writer)
	    {
	        base.WriteElements(writer);

            writer.WriteElementString(SHADE_TYPE_ELEMENT, IcdXmlConvert.ToString(ShadeType));
	    }
	}
}