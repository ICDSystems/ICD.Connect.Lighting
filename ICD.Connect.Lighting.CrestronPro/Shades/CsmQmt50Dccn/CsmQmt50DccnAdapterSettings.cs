﻿using System;
using ICD.Common.Utils;
using ICD.Common.Utils.Xml;
using ICD.Connect.Settings.Attributes;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Lighting.CrestronPro.Shades.CsmQmt50Dccn
{
	[KrangSettings(FACTORY_NAME)]
	public sealed class CsmQmt50DccnAdapterSettings : AbstractShadeWithBasicSettingsAdapterSettings, ICsmQmt50DccnAdapterSettings
	{
		private const string FACTORY_NAME = "CsmQmt50Dccn";
		private const string CRESNET_ID_ELEMENT = "CresnetID";

		[IpIdSettingsProperty]
		public byte CresnetId { get; set; }

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return (typeof(CsmQmt50DccnAdapter)); } }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(CRESNET_ID_ELEMENT, StringUtils.ToIpIdString(CresnetId));
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			CresnetId = XmlUtils.TryReadChildElementContentAsByte(xml, CRESNET_ID_ELEMENT) ?? 0;
		}
	}
}