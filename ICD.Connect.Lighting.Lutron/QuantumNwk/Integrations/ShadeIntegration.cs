namespace ICD.Connect.Lighting.Lutron.QuantumNwk.Integrations
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
		private ShadeIntegration(int integrationId, string name, LutronQuantumNwkDevice parent)
			: base(integrationId, name, parent)
		{
		}

		/// <summary>
		/// Instantiates a ShadeIntegration from xml.
		/// </summary>
		/// <param name="xml"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static ShadeIntegration FromXml(string xml, LutronQuantumNwkDevice parent)
		{
			int integrationId = GetIntegrationIdFromXml(xml);
			string name = GetNameFromXml(xml);

			return new ShadeIntegration(integrationId, name, parent);
		}

		#endregion
	}
}
