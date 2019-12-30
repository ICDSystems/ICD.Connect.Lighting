using ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice;
using ICD.Connect.Lighting.Shades;

namespace ICD.Connect.Lighting.Lutron.Nwk.Integrations
{
	public sealed class ShadeGroupIntegration : AbstractShadeIntegration
	{
		/// <summary>
		/// The string prefix for communication with the lighting processor, e.g. SHADES.
		/// </summary>
		protected override string Command { get { return LutronUtils.COMMAND_SHADEGROUP; } }

		public eShadeType ShadeType { get; private set; }

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="integrationId"></param>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		/// <param name="shadeType"></param>
		private ShadeGroupIntegration(int integrationId, string name, ILutronNwkDevice parent, eShadeType shadeType)
			: base(integrationId, name, parent)
		{
			ShadeType = shadeType;
		}

		/// <summary>
		/// Instantiates a ZoneIntegration from xml.
		/// </summary>
		/// <param name="xml"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static ShadeGroupIntegration FromXml(string xml, ILutronNwkDevice parent)
		{
			int integrationId = GetIntegrationIdFromXml(xml);
			string name = GetNameFromXml(xml);
			eShadeType type = GetShadeTypeFromXml(xml);

			return new ShadeGroupIntegration(integrationId, name, parent, type);
		}

		#endregion
	}
}
