using System.Collections.Generic;
using ICD.Connect.API.Commands;
using ICD.Connect.Devices;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Lighting.Shades
{
	public abstract class AbstractShadeDevice<TSettings> : AbstractDevice<TSettings>, IShadeDevice
		where TSettings : IShadeDeviceSettings, new()
	{
		public eShadeType ShadeType { get; private set; }
		public abstract void Open();
		public abstract void Close();

		#region Settings
		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);
			ShadeType = settings.ShadeType;
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.ShadeType = ShadeType;
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