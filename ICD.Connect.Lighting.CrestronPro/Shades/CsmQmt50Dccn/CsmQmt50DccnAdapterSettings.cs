﻿using System;
using ICD.Common.Utils;
using ICD.Common.Utils.Xml;
using ICD.Connect.Misc.CrestronPro;
using ICD.Connect.Misc.CrestronPro.Utils;
using ICD.Connect.Settings.Attributes;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Lighting.CrestronPro.Shades.CsmQmt50Dccn
{
	[KrangSettings(FACTORY_NAME)]
	public sealed class CsmQmt50DccnAdapterSettings : AbstractShadeWithBasicSettingsAdapterSettings, ICsmQmt50DccnAdapterSettings, ICresnetDeviceSettings
	{
		private const string FACTORY_NAME = "CsmQmt50Dccn";

		[CrestronByteSettingsProperty]
		public byte? CresnetId { get; set; }
		public int? ParentId { get; set; }
		public int? BranchId { get; set; }

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

			writer.WriteElementString(CresnetSettingsUtils.CRESNET_ID_ELEMENT, CresnetId == null ? null : StringUtils.ToIpIdString((byte)CresnetId));
			writer.WriteElementString(CresnetSettingsUtils.PARENT_ID_ELEMENT, ParentId == null ? null : ParentId.Value.ToString());
			writer.WriteElementString(CresnetSettingsUtils.BRANCH_ID_ELEMENT, BranchId == null ? null : BranchId.Value.ToString());
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			CresnetId = XmlUtils.TryReadChildElementContentAsByte(xml, CresnetSettingsUtils.CRESNET_ID_ELEMENT);
			ParentId = XmlUtils.TryReadChildElementContentAsInt(xml, CresnetSettingsUtils.PARENT_ID_ELEMENT);
			BranchId = XmlUtils.TryReadChildElementContentAsInt(xml, CresnetSettingsUtils.BRANCH_ID_ELEMENT);
		}
	}
}