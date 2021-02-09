using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils.Xml;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice;

namespace ICD.Connect.Lighting.Lutron.Nwk.Integrations.Abstracts
{
	/// <summary>
	/// Abstract integration, just a container for a name, integrationId, and a NWK device,
	/// plus some helpful methods for dealing with config, etc.
	/// </summary>
	/// <typeparam name="TIntegrationId"></typeparam>
	public abstract class AbstractIntegrationBase<TIntegrationId> : IDisposable, IConsoleNode
	{
		private readonly TIntegrationId m_IntegrationId;
		private readonly string m_Name;
		private readonly ILutronNwkDevice m_Parent;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="integrationId"></param>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		protected AbstractIntegrationBase(TIntegrationId integrationId, string name, ILutronNwkDevice parent)
		{
			m_IntegrationId = integrationId;
			m_Name = name;
			m_Parent = parent;

			Subscribe(m_Parent);
		}

		/// <summary>
		/// Gets the unique integration id.
		/// </summary>
		public TIntegrationId IntegrationId { get { return m_IntegrationId; } }

		/// <summary>
		/// Gets the name of the integration.
		/// </summary>
		[PublicAPI]
		public string Name { get { return m_Name; } }

		protected ILutronNwkDevice Parent { get { return m_Parent; } }

		/// <summary>
		/// Release resources.
		/// </summary>
		public virtual void Dispose()
		{
			Unsubscribe(m_Parent);
		}

		/// <summary>
		/// Gets the integration id from the element attributes as an int.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		protected static int GetIntegrationIdIntFromXml(string xml)
		{
			return XmlUtils.GetAttributeAsInt(xml, "integrationId");
		}

		/// <summary>
		/// Gets the integration id from the element attributes as a string.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		protected static string GetIntegrationIdStringFromXml(string xml)
		{
			return XmlUtils.GetAttributeAsString(xml, "integrationId");
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

		#region Console

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

		#endregion

		#region Device Callbacks

		/// <summary>
		/// Subscribe to the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected virtual void Subscribe(ILutronNwkDevice parent)
		{
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected virtual void Unsubscribe(ILutronNwkDevice parent)
		{
		}

		#endregion
	}
}