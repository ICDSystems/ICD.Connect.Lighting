using System.Collections.Generic;
using ICD.Connect.API.Commands;

namespace ICD.Connect.Lighting.Mock.Controls
{
	public abstract class AbstractMockShadeLightingControl : AbstractMockLightingControl
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="room"></param>
		/// <param name="name"></param>
		protected AbstractMockShadeLightingControl(int id, int room, string name)
			: base(id, room, name)
		{
		}

		/// <summary>
		/// Starts raising the shade.
		/// </summary>
		public void StartRaising()
		{
		}

		/// <summary>
		/// Starts lowering the shade.
		/// </summary>
		public void StartLowering()
		{
		}

		/// <summary>
		/// Stops moving the shade.
		/// </summary>
		public void StopMoving()
		{
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new ConsoleCommand("StartRaising", "Starts raising the shades", () => StartRaising());
			yield return new ConsoleCommand("StartLowering", "Starts lowering the shades", () => StartLowering());
			yield return new ConsoleCommand("StopMoving", "Stops moving the shades", () => StopMoving());
		}

		/// <summary>
		/// Workaround for unverifiable code warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}
	}
}
