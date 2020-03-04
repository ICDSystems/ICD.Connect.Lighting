using ICD.Common.Utils.Xml;
using ICD.Connect.Misc.CrestronPro;
using ICD.Connect.Misc.CrestronPro.Utils;
using ICD.Connect.Settings.Attributes;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Lighting.CrestronPro.Shades.CsmQmt50Dccn
{
	[KrangSettings("CsmQmt50Dccn", typeof(CsmQmt50DccnAdapter))]
	public sealed class CsmQmt50DccnAdapterSettings : AbstractShadeWithBasicSettingsAdapterSettings, ICsmQmt50DccnAdapterSettings, ICresnetDeviceSettings
	{
		[CrestronByteSettingsProperty]
		public byte? CresnetId { get; set; }
		public int? ParentId { get; set; }
		public int? BranchId { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			CresnetSettingsUtils.WritePropertiesToXml(this, writer);
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			CresnetSettingsUtils.ReadPropertiesFromXml(this, xml);
		}
	}
}
