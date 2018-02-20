using System.Collections.Generic;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices.Controls;

namespace ICD.Connect.Lighting.Shades.Controls
{
	public sealed class ShadePositionFeedbackControl<T> : AbstractDeviceControl<T>, IShadePositionFeedbackControl
		where T : IShadeWithPositionFeedback
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public ShadePositionFeedbackControl(T parent, int id) : base(parent, id)
		{
		}

		#region IShadePositionFeedbackControl

		public float GetPosition()
		{
			return Parent.GetPosition();
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
		}

		/// <summary>
		/// Workaround for the "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			addRow("Shade Position", GetPosition().ToString("0.00"));
		}

		#endregion
	}
}