namespace ICD.Connect.Lighting.Lutron.QuantumNwk.Integrations
{
	public sealed class ShadeGroupIntegration : AbstractShadeIntegration
	{
		/// <summary>
		/// The string prefix for communication with the lighting processor, e.g. SHADES.
		/// </summary>
		protected override string Command { get { return LutronUtils.COMMAND_SHADEGROUP; } }

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="integrationId"></param>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		private ShadeGroupIntegration(int integrationId, string name, LutronQuantumNwkDevice parent)
			: base(integrationId, name, parent)
		{
		}

		/// <summary>
		/// Instantiates a ZoneIntegration from xml.
		/// </summary>
		/// <param name="xml"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static ShadeGroupIntegration FromXml(string xml, LutronQuantumNwkDevice parent)
		{
			int integrationId = GetIntegrationIdFromXml(xml);
			string name = GetNameFromXml(xml);

			return new ShadeGroupIntegration(integrationId, name, parent);
		}

		#endregion
	}
}
