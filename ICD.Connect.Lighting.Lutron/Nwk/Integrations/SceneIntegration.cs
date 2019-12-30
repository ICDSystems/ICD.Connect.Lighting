using ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice;

namespace ICD.Connect.Lighting.Lutron.Nwk.Integrations
{
	public sealed class SceneIntegration : AbstractIntegration
	{
		/// <summary>
		/// The string prefix for communication with the lighting processor, e.g. SHADES.
		/// </summary>
		protected override string Command
		{
			get
			{
				// This is a hack, we're not actually expecting any feedback from the device for scenes.
				return "SCENE";
			}
		}

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
			int integrationId = GetIntegrationIdFromXml(xml);
			string name = GetNameFromXml(xml);

			return new SceneIntegration(integrationId, name, parent);
		}
	}
}
