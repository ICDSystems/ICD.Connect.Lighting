using ICD.Common.Utils.Xml;
using ICD.Connect.Misc.CrestronPro.Cresnet;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Lighting.CrestronPro.Shades.CsmQmt50Dccn
{
	[KrangSettings("CsmQmt50Dccn", typeof(CsmQmt50DccnAdapter))]
	public sealed class CsmQmt50DccnAdapterSettings : AbstractShadeWithBasicSettingsAdapterSettings, ICsmQmt50DccnAdapterSettings, ICresnetDeviceSettings
	{
		private readonly CresnetSettings m_CresnetSettings;

		public CresnetSettings CresnetSettings { get { return m_CresnetSettings; } }

		public CsmQmt50DccnAdapterSettings()
		{
			m_CresnetSettings = new CresnetSettings();
		}

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			m_CresnetSettings.WriteElements(writer);
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			m_CresnetSettings.ParseXml(xml);
		}
	}
}
