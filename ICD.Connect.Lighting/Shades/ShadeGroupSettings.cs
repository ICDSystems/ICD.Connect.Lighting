using System.Collections.Generic;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Xml;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Lighting.Shades
{
	[KrangSettings("ShadeGroup", typeof(ShadeGroup))]
	public sealed class ShadeGroupSettings : AbstractShadeDeviceSettings, IShadeGroupSettings
	{
		private const string SHADES_ELEMENT = "Shades";
		private const string SHADE_ELEMENT = "Shade";

		public IEnumerable<int?> ShadeIds { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			XmlUtils.WriteListToXml(writer, ShadeIds.Order(), SHADES_ELEMENT, SHADE_ELEMENT);
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			ShadeIds = XmlUtils.ReadListFromXml(xml, SHADES_ELEMENT, SHADE_ELEMENT, c => XmlUtils.TryReadElementContentAsInt(c));
		}
	}
}
