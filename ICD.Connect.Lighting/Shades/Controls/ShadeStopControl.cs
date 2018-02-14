using System.Collections.Generic;
using ICD.Connect.API.Commands;
using ICD.Connect.Devices.Controls;

namespace ICD.Connect.Lighting.Shades.Controls
{
	public sealed class ShadeStopControl<T> : AbstractDeviceControl<T>, IShadeStopControl
		where T : IShadeWithStop
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public ShadeStopControl(T parent, int id) : base(parent, id)
		{
		}

		#region IShadeStopControl

		public void Stop()
		{
			Parent.Stop();
		}

		#endregion

		#region Console

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new ConsoleCommand("Stop", "Stops the current motion of the Shade", () => Stop());
		}

		/// <summary>
		/// Workaround for the "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		#endregion
	}
}