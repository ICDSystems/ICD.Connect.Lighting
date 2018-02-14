using System.Collections.Generic;
using ICD.Connect.API.Commands;
using ICD.Connect.Devices.Controls;

namespace ICD.Connect.Lighting.Shades.Controls
{
	public sealed class ShadeSetPositionControl<T> : AbstractDeviceControl<T>, IShadeSetPositionControl
		where T : IShadeWithSettablePosition
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public ShadeSetPositionControl(T parent, int id) : base(parent, id)
		{
		}

		#region IShadeSetPositionControl

		public void SetPosition(float position)
		{
			Parent.SetPosition(position);
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

			yield return
				new GenericConsoleCommand<float>("SetPosition",
				                                 "Sets the position of this shade, with 0 being 'fully closed' and 1 being 'fully open'",
				                                 a => SetPosition(a));

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