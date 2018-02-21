using System.Collections.Generic;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Lighting.Environment;

namespace ICD.Connect.Lighting.Mock.Controls
{
	public abstract class AbstractMockLightingControl : IConsoleNode, IEnvironmentPeripheral
	{
		private readonly int m_Id;
		private readonly int m_Room;
		private readonly string m_Name;

		#region Properties

		public abstract LightingProcessorControl.ePeripheralType PeripheralType { get; }
		public int Id { get { return m_Id; } }
		public int Room { get { return m_Room; } }
		public string Name { get { return m_Name; } }

		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		public string ConsoleName { get { return GetType().Name; } }

		/// <summary>
		/// Gets the help information for the node.
		/// </summary>
		public string ConsoleHelp { get { return string.Empty; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="room"></param>
		/// <param name="name"></param>
		protected AbstractMockLightingControl(int id, int room, string name)
		{
			m_Id = id;
			m_Room = room;
			m_Name = name;
		}

		/// <summary>
		/// Returns the control data as a LightingProcessorControl struct.
		/// </summary>
		/// <returns></returns>
		public LightingProcessorControl ToLightingProcessorControl()
		{
			return new LightingProcessorControl(PeripheralType, m_Id, m_Room, m_Name);
		}

		public abstract void Dispose();

		#region Console Commands

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IConsoleNodeBase> GetConsoleNodes()
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
			addRow("Room", Room);
			addRow("Name", Name);
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
