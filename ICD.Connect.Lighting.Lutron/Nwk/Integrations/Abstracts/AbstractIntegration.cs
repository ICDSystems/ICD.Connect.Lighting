using ICD.Common.Utils.EventArguments;
using ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice;

namespace ICD.Connect.Lighting.Lutron.Nwk.Integrations.Abstracts
{
	/// <summary>
	/// Integrations that communicate with the parent, and register for feedback, etc.
	/// </summary>
	public abstract class AbstractIntegration<TIntegrationId> : AbstractIntegrationBase<TIntegrationId>
	{
		#region Properties

		/// <summary>
		/// The string prefix for communication with the lighting processor, e.g. SHADES.
		/// </summary>
		protected abstract string Command { get; }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="integrationId"></param>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		protected AbstractIntegration(TIntegrationId integrationId, string name, ILutronNwkDevice parent) : base(integrationId, name, parent)
		{
		}

		

		#region Private Methods

		

		/// <summary>
		/// Builds the execute data with component number and sends it to the device.
		/// </summary>
		/// <param name="component"></param>
		/// <param name="action"></param>
		/// <param name="parameters"></param>
		protected void ExecuteComponent(int component, int action, params object[] parameters)
		{
			SendDataWithComponent(LutronUtils.MODE_EXECUTE, component, action, parameters);
		}

		/// <summary>
		/// Builds the query data and sends it to the device.
		/// </summary>
		/// <param name="component"></param>
		/// <param name="action"></param>
		protected void QueryComponent(int component, int action)
		{
			SendDataWithComponent(LutronUtils.MODE_QUERY, component, action);
		}



		/// <summary>
		/// Builds the data for the current integration and sends it to the device.
		/// </summary>
		/// <param name="mode"></param>
		/// <param name="component"></param>
		/// <param name="action"></param>
		/// <param name="parameters"></param>
		private void SendDataWithComponent(char mode, int component, int action, params object[] parameters)
		{
			string data = LutronUtils.BuildDataWithComponent(mode, Command, IntegrationId.ToString(), component, action, parameters);
			Parent.EnqueueData(data);
		}

		/// <summary>
		/// Returns the key for registration with device callbacks.
		/// e.g. DEVICE,1 will give feedback for the device with integration 1.
		/// </summary>
		/// <returns></returns>
		private string GetKey()
		{
			return LutronUtils.GetKey(Command, IntegrationId.ToString());
		}

		/// <summary>
		/// Override to query the device when it goes online.
		/// </summary>
		protected virtual void Initialize()
		{
		}

		#endregion

		#region Device Callbacks

		/// <summary>
		/// Subscribe to the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected override void Subscribe(ILutronNwkDevice parent)
		{
			base.Subscribe(parent);

			if (parent == null)
				return;

			parent.OnInitializedChanged += ParentOnInitializedChanged;
			parent.RegisterIntegrationCallback(GetKey(), ParentOnOutput);
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected override void Unsubscribe(ILutronNwkDevice parent)
		{
			base.Unsubscribe(parent);

			if (parent == null)
				return;

			parent.OnInitializedChanged -= ParentOnInitializedChanged;
			parent.UnregisterIntegrationCallback(GetKey(), ParentOnOutput);
		}

		/// <summary>
		/// Called when the device initialization state changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="boolEventArgs"></param>
		private void ParentOnInitializedChanged(object sender, BoolEventArgs boolEventArgs)
		{
			Initialize();
		}

		/// <summary>
		/// Called when we receive data from the lighting processor for this integration.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="data"></param>
		protected abstract void ParentOnOutput(ILutronNwkDevice sender, string data);

		#endregion
	}
}
