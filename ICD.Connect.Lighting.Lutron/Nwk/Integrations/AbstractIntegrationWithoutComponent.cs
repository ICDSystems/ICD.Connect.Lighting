using System;
using System.Collections.Generic;
using System.Text;
using ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice;

namespace ICD.Connect.Lighting.Lutron.Nwk.Integrations
{
	public abstract class AbstractIntegrationWithoutComponent : AbstractIntegration
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="integrationId"></param>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		protected AbstractIntegrationWithoutComponent(int integrationId, string name, ILutronNwkDevice parent) : base(integrationId, name, parent)
		{
		}

		#region Private/Protected Methods

		/// <summary>
		/// Builds the execute data and sends it to the device.
		/// </summary>
		/// <param name="action"></param>
		/// <param name="parameters"></param>
		protected void Execute(int action, params object[] parameters)
		{
			SendData(LutronUtils.MODE_EXECUTE, action, parameters);
		}

		/// <summary>
		/// Builds the query data and sends it to the device.
		/// </summary>
		/// <param name="action"></param>
		protected void Query(int action)
		{
			SendData(LutronUtils.MODE_QUERY, action);
		}

		/// <summary>
		/// Builds the data for the current integration and sends it to the device.
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="action"></param>
		/// <param name="parameters"></param>
		private void SendData(char mode, int action, params object[] parameters)
		{
			string data = LutronUtils.BuildData(mode, Command, IntegrationId, action, parameters);
			Parent.EnqueueData(data);
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

			int action = LutronUtils.GetIntegrationActionNumberWithoutComponent(data);
			string[] parameters = LutronUtils.GetIntegrationActionParametersWithoutComponent(data);

			ParentOnResponse(action, parameters);
		}

		/// <summary>
		/// Called when we receive a response from the lighting processor for this integration.
		/// </summary>
		/// <param name="action">The action number for the response.</param>
		/// <param name="parameters">The collection of string parameters.</param>
		protected virtual void ParentOnResponse(int action, string[] parameters)
		{
		}

		#endregion

	}
}
