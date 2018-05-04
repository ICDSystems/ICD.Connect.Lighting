using System.Collections.Generic;
using ICD.Connect.API.Commands;
using ICD.Connect.Lighting.Environment;
using ICD.Connect.Lighting.Shades;

namespace ICD.Connect.Lighting.Mock.Controls
{
	public abstract class AbstractMockShadeLightingControl : AbstractMockLightingControl, IShadeEnvironmentPeripheral
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="room"></param>
		/// <param name="name"></param>
		/// <param name="shadeType"></param>
		protected AbstractMockShadeLightingControl(int id, int room, string name, eShadeType shadeType)
			: base(id, room, name)
		{
			ShadeType = shadeType;
		}

		public eShadeType ShadeType { get; private set; }

		public eShadeDirection LastDirection { get { return eShadeDirection.Neither; } }

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

		public override LightingProcessorControl ToLightingProcessorControl()
		{
			var control = base.ToLightingProcessorControl();
			return new LightingProcessorControl(control.PeripheralType, control.Id, control.Room, control.Name, ShadeType);
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
