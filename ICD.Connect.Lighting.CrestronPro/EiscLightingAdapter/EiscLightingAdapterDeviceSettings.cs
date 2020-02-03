using ICD.Common.Utils;
using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Lighting.CrestronPro.EiscLightingAdapter
{
	[KrangSettings("EiscLightingAdapterDevice", typeof(EiscLightingAdapterDevice))]
	public sealed class EiscLightingAdapterDeviceSettings : AbstractDeviceSettings
	{
		private const string IPID_ELEMENT = "IPID";
		private const string ADDRESS_ELEMENT = "Address";
		private const string CONFIG_ELEMENT = "Config";

		private const string DEFAULT_CONFIG_FILE = "EiscConfig.xml";
		internal const string CONFIG_PATH = "EiscLighting";

		private string m_ConfigPath;

		[CrestronByteSettingsProperty]
		public byte? Ipid { get; set; }

		public string Address { get; set; }

		[PathSettingsProperty(CONFIG_PATH, ".xml")]
		public string Config
		{
			get
			{
				if (string.IsNullOrEmpty(m_ConfigPath))
					m_ConfigPath = DEFAULT_CONFIG_FILE;
				return m_ConfigPath;
			}
			set { m_ConfigPath = value; }
		}

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(IPID_ELEMENT, Ipid == null ? null : StringUtils.ToIpIdString(Ipid.Value));
			writer.WriteElementString(ADDRESS_ELEMENT, IcdXmlConvert.ToString(Address));
			writer.WriteElementString(CONFIG_ELEMENT, Config);
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			Ipid = XmlUtils.TryReadChildElementContentAsByte(xml, IPID_ELEMENT);
			Address = XmlUtils.TryReadChildElementContentAsString(xml, ADDRESS_ELEMENT);
			Config = XmlUtils.TryReadChildElementContentAsString(xml, CONFIG_ELEMENT);
		}
	}
}