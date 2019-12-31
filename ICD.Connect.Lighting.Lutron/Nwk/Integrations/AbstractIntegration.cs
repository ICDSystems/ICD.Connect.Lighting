using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Xml;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice;

namespace ICD.Connect.Lighting.Lutron.Nwk.Integrations
{
	/// <summary>
	/// Base class for all Lutron QuantumNwk integrations that communicate with the device.
	/// </summary>
	public abstract class AbstractIntegration : IDisposable, IConsoleNode
	{
		private readonly int m_IntegrationId;
		private readonly string m_Name;
		private readonly ILutronNwkDevice m_Parent;

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

		protected ILutronNwkDevice Parent { get { return m_Parent; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="integrationId"></param>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		protected AbstractIntegration(int integrationId, string name, ILutronNwkDevice parent)
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
			string data = LutronUtils.BuildDataWithComponent(mode, Command, IntegrationId, component, action, parameters);
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
		private void Subscribe(ILutronNwkDevice parent)
		{
			parent.OnInitializedChanged += ParentOnInitializedChanged;
			parent.RegisterIntegrationCallback(GetKey(), ParentOnOutput);
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		private void Unsubscribe(ILutronNwkDevice parent)
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
		protected abstract void ParentOnOutput(ILutronNwkDevice sender, string data);

		#endregion

		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		public string ConsoleName { get { return Name; } }

		/// <summary>
		/// Gets the help information for the node.
		/// </summary>
		public string ConsoleHelp { get { return "Lutron Integration Component"; } }

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public virtual IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			yield break;
		}

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public virtual void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			addRow("Name", Name);
			addRow("Id", IntegrationId);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public virtual IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			yield break;
		}
	}
}
