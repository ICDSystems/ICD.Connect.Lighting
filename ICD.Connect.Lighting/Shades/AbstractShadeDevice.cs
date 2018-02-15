using System.Collections.Generic;
using ICD.Connect.API.Commands;
using ICD.Connect.Devices;

namespace ICD.Connect.Lighting.Shades
{
	public abstract class AbstractShadeDevice<TSettings> : AbstractDevice<TSettings>, IShadeDevice
		where TSettings : IShadeDeviceSettings, new()
	{
		public abstract void Open();
		public abstract void Close();


		#region Console

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new ConsoleCommand("Open", "Opens the shade", () => Open());
			yield return new ConsoleCommand("Close", "Closes the shade", ()=> Close());
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		#endregion
	}
}