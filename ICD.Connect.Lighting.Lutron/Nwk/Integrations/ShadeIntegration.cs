using ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice;
using ICD.Connect.Lighting.Lutron.Nwk.Integrations.Abstracts;
using ICD.Connect.Lighting.Shades;

namespace ICD.Connect.Lighting.Lutron.Nwk.Integrations
{
	public sealed class ShadeIntegration : AbstractShadeIntegration
	{
		/// <summary>
		/// The string prefix for communication with the lighting processor, e.g. SHADES.
		/// </summary>
		protected override string Command { get { return LutronUtils.COMMAND_OUTPUT; } }

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="integrationId"></param>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		/// <param name="shadeType"></param>
		private ShadeIntegration(int integrationId, string name, ILutronNwkDevice parent, eShadeType shadeType)
			: base(integrationId, name, parent, shadeType)
		{
		}

		/// <summary>
		/// Instantiates a ShadeIntegration from xml.
		/// </summary>
		/// <param name="xml"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static ShadeIntegration FromXml(string xml, ILutronNwkDevice parent)
		{
			int integrationId = GetIntegrationIdIntFromXml(xml);
			string name = GetNameFromXml(xml);
			eShadeType type = GetShadeTypeFromXml(xml);

			return new ShadeIntegration(integrationId, name, parent, type);
		}

		#endregion
	}
}
