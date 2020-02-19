using System;
using System.Collections.Generic;
using ICD.Common.Utils.Xml;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;

namespace ICD.Connect.Lighting.CrestronPro.EiscLightingAdapter.Components
{
	public abstract class AbstractEiscLightingComponent : IEiscLightingComponent, IDisposable
	{
		private const string ID_ATTRIBUTE = "id";
		private const string NAME_ELEMENT = "Name";

		protected abstract LightingProcessorControl.ePeripheralType ControlType { get; }

		public int Id { get; private set; }

		public string Name { get; private set; }

		protected EiscLightingRoom Room { get; private set; }

		protected AbstractEiscLightingComponent(EiscLightingRoom room, int id, string name)
		{
			Room = room;
			Id = id;
			Name = name;
		}

		public virtual LightingProcessorControl ToLightingProcessorControl()
		{
			return new LightingProcessorControl(ControlType,Id, Room.Id, Name);
		}

		protected static string GetName(string xml)
		{
			return XmlUtils.ReadChildElementContentAsString(xml, NAME_ELEMENT);
		}

		protected static int GetId(string xml)
		{
			return XmlUtils.GetAttributeAsInt(xml, ID_ATTRIBUTE);
		}

		public virtual void Dispose()
		{
			Unregister();
		}

		protected virtual void Register()
		{
		}

		protected virtual void Unregister()
		{
		}

		#region Console
		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		public string ConsoleName { get { return Name; } }

		/// <summary>
		/// Gets the help information for the node.
		/// </summary>
		public string ConsoleHelp { get { return ""; } }

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
			addRow("Id", Id);
			addRow("Name", Name);
			addRow("ControlType", ControlType);
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
	}
}