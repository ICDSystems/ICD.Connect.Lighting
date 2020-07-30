using ICD.Common.Utils.Xml;
using ICD.Connect.Misc.CrestronPro.Cresnet;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Lighting.CrestronPro.Shades.CsmQmt50Dccn
{
	[KrangSettings("CsmQmt50Dccn", typeof(CsmQmt50DccnAdapter))]
	public sealed class CsmQmt50DccnAdapterSettings : AbstractShadeWithBasicSettingsAdapterSettings, ICsmQmt50DccnAdapterSettings, ICresnetDeviceSettings
	{
		private readonly CresnetDeviceSettings m_CresnetDeviceSettings;

		public CresnetDeviceSettings CresnetDeviceSettings { get { return m_CresnetDeviceSettings; } }

		public CsmQmt50DccnAdapterSettings()
		{
			m_CresnetDeviceSettings = new CresnetDeviceSettings();
		}

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			m_CresnetDeviceSettings.WriteElements(writer);
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			m_CresnetDeviceSettings.ParseXml(xml);
		}
	}
}
