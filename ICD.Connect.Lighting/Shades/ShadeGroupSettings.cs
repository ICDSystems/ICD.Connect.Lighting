using System;
using System.Collections.Generic;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Xml;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Lighting.Shades
{
	[KrangSettings(FACTORY_NAME)]
	public sealed class ShadeGroupSettings : AbstractShadeDeviceSettings, IShadeGroupSettings
	{
		private const string FACTORY_NAME = "ShadeGroup";
		private const string SHADES_ELEMENT = "Shades";
		private const string SHADE_ELEMENT = "Shade";

		public IEnumerable<int?> ShadesIds { get; set; }  

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(ShadeGroup); } }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			XmlUtils.WriteListToXml(writer, ShadesIds.Order(), SHADES_ELEMENT, SHADE_ELEMENT);
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			ShadesIds =  XmlUtils.ReadListFromXml(xml, SHADES_ELEMENT, SHADE_ELEMENT, c => XmlUtils.TryReadElementContentAsInt(c));
		}
	}
}