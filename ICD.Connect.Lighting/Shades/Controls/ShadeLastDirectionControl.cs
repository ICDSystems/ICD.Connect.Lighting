﻿using System;
using System.Collections.Generic;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices.Controls;

namespace ICD.Connect.Lighting.Shades.Controls
{
	public sealed class ShadeLastDirectionControl<T> : AbstractDeviceControl<T>, IShadeLastDirectionControl
		where T : IShadeWithLastDirectionFeedback
	{
		public event EventHandler OnDirectionChanged;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public ShadeLastDirectionControl(T parent, int id) : base(parent, id)
		{
			Parent.OnDirectionChanged += ParentOnDirectionChanged;
		}

		private void ParentOnDirectionChanged(object sender, EventArgs eventArgs)
		{
			OnDirectionChanged.Raise(this);
		}

		#region IShadeLastDirectionControl

		public eShadeDirection GetLastDirection()
		{
			return Parent.GetLastDirection();
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

			addRow("Last Direction", GetLastDirection());
		}

		#endregion
	}
}