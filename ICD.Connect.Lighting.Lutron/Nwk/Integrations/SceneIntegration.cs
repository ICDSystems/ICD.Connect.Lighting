using ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice;
using ICD.Connect.Lighting.Lutron.Nwk.Integrations.Abstracts;

namespace ICD.Connect.Lighting.Lutron.Nwk.Integrations
{
	public sealed class SceneIntegration : AbstractIntegrationBase<int>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="integrationId"></param>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		private SceneIntegration(int integrationId, string name, ILutronNwkDevice parent)
			: base(integrationId, name, parent)
		{
		}

		/// <summary>
		/// Instantiates a SceneIntegration from xml.
		/// </summary>
		/// <param name="xml"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static SceneIntegration FromXml(string xml, ILutronNwkDevice parent)
		{
			int integrationId = GetIntegrationIdIntFromXml(xml);
			string name = GetNameFromXml(xml);

			return new SceneIntegration(integrationId, name, parent);
		}
	}
}
