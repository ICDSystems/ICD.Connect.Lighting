using System.Collections.Generic;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices.Controls;

namespace ICD.Connect.Lighting.Shades.Controls
{
	public sealed class ShadeInMotionFeedbackControl<T> : AbstractDeviceControl<T>, IShadeInMotionFeedbackControl
		where T : IShadeWithInMotionFeedback
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public ShadeInMotionFeedbackControl(T parent, int id) 
			: base(parent, id)
		{
		}

		#region IShadeInMotionFeedbackControl
		public bool GetInMotion()
		{
			return Parent.GetIsInMotion();
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

			addRow("In Motion", GetInMotion());
		}

		#endregion
	}
}