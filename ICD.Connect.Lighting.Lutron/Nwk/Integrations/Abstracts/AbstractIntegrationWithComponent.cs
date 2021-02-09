using ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice;

namespace ICD.Connect.Lighting.Lutron.Nwk.Integrations.Abstracts
{
	public abstract class AbstractIntegrationWithComponent<TIntegrationId> : AbstractIntegration<TIntegrationId>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="integrationId"></param>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		protected AbstractIntegrationWithComponent(TIntegrationId integrationId, string name, ILutronNwkDevice parent) : base(integrationId, name, parent)
		{
		}

		/// <summary>
		/// Called when we receive data from the lighting processor for this integration.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="data"></param>
		protected override void ParentOnOutput(ILutronNwkDevice sender, string data)
		{
			if (LutronUtils.GetMode(data) != LutronUtils.MODE_RESPONSE)
				return;

			int action = LutronUtils.GetIntegrationActionNumberWithComponent(data);
			int component = LutronUtils.GetIntegrationComponentNumber(data);
			string[] parameters = LutronUtils.GetIntegrationActionParametersWithComponent(data);

			ParentOnResponse(action, component, parameters);
		}

		/// <summary>
		/// Called when we receive a response from the lighting processor for this integration.
		/// </summary>
		/// <param name="action">The action number for the response.</param>
		/// <param name="component">The compoent number for the response</param>
		/// <param name="parameters">The collection of string parameters.</param>
		protected virtual void ParentOnResponse(int action, int component, string[] parameters)
		{
		}
	}
}
