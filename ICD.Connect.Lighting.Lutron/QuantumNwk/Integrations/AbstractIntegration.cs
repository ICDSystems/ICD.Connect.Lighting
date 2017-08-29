using System;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Properties;
using ICD.Common.Utils.Xml;

namespace ICD.Connect.Lighting.Lutron.QuantumNwk.Integrations
{
	/// <summary>
	/// Base class for all Lutron QuantumNwk integrations that communicate with the device.
	/// </summary>
	public abstract class AbstractIntegration : IDisposable
	{
		private readonly int m_IntegrationId;
		private readonly string m_Name;
		private readonly LutronQuantumNwkDevice m_Parent;

		#region Properties

		/// <summary>
		/// Gets the unique integration id.
		/// </summary>
		public int IntegrationId { get { return m_IntegrationId; } }

		/// <summary>
		/// Gets the name of the integration.
		/// </summary>
		[PublicAPI]
		public string Name { get { return m_Name; } }

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
		protected AbstractIntegration(int integrationId, string name, LutronQuantumNwkDevice parent)
		{
			m_IntegrationId = integrationId;
			m_Name = name;
			m_Parent = parent;

			Subscribe(m_Parent);
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		public virtual void Dispose()
		{
			Unsubscribe(m_Parent);
		}

		#region Private Methods

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
			m_Parent.EnqueueData(data);
		}

		/// <summary>
		/// Gets the integration id from the element attributes.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		protected static int GetIntegrationIdFromXml(string xml)
		{
			return XmlUtils.GetAttributeAsInt(xml, "integrationId");
		}

		/// <summary>
		/// Gets the integration name from the element attributes.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		protected static string GetNameFromXml(string xml)
		{
			return XmlUtils.GetAttributeAsString(xml, "name");
		}

		/// <summary>
		/// Returns the key for registration with device callbacks.
		/// e.g. DEVICE,1 will give feedback for the device with integration 1.
		/// </summary>
		/// <returns></returns>
		private string GetKey()
		{
			return LutronUtils.GetKey(Command, IntegrationId);
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
		private void Subscribe(LutronQuantumNwkDevice parent)
		{
			parent.OnInitializedChanged += ParentOnInitializedChanged;
			parent.RegisterIntegrationCallback(GetKey(), ParentOnOutput);
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		private void Unsubscribe(LutronQuantumNwkDevice parent)
		{
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
		private void ParentOnOutput(LutronQuantumNwkDevice sender, string data)
		{
			if (LutronUtils.GetMode(data) != LutronUtils.MODE_RESPONSE)
				return;

			int action = LutronUtils.GetIntegrationActionNumber(data);
			string[] parameters = LutronUtils.GetIntegrationActionParameters(data);

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
