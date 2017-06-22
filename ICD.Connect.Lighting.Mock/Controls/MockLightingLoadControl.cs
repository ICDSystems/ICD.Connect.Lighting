﻿using System;
using System.Collections.Generic;
using ICD.Common.EventArguments;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Timers;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;

namespace ICD.Connect.Lighting.Mock.Controls
{
	public sealed class MockLightingLoadControl : AbstractMockLightingControl, IDisposable
	{
		// The step size for each increment/decrement of a load level.
		private const float INCREMENT_DELTA = 0.05f;

		// How often to increment/decrement the load level.
		private const long BETWEEN_REPEAT = 500;

		public event EventHandler<FloatEventArgs> OnLoadLevelChanged;

		private readonly Repeater m_Repeater;
		private bool m_Up;
		private float m_LoadLevel;

		#region Properties

		public override LightingProcessorControl.eControlType ControlType
		{
			get { return LightingProcessorControl.eControlType.Load; }
		}

		public float LoadLevel
		{
			get { return m_LoadLevel; }
			set
			{
				if (Math.Abs(value - m_LoadLevel) < 0.001f)
					return;

				m_LoadLevel = value;

				OnLoadLevelChanged.Raise(this, new FloatEventArgs(m_LoadLevel));
			}
		}

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="room"></param>
		/// <param name="name"></param>
		public MockLightingLoadControl(int id, int room, string name)
			: base(id, room, name)
		{
			m_Repeater = new Repeater(BETWEEN_REPEAT, BETWEEN_REPEAT);
			m_Repeater.OnInitialRepeat += RepeaterOnRepeat;
			m_Repeater.OnRepeat += RepeaterOnRepeat;
		}

		#region Methods

		public void Dispose()
		{
			OnLoadLevelChanged = null;

			m_Repeater.OnInitialRepeat -= RepeaterOnRepeat;
			m_Repeater.OnRepeat -= RepeaterOnRepeat;

			m_Repeater.Dispose();
		}

		public void StartRaising()
		{
			m_Up = true;
			m_Repeater.Start();
		}

		public void StartLowering()
		{
			m_Up = false;
			m_Repeater.Start();
		}

		public void StopRamping()
		{
			m_Repeater.Stop();
		}

		#endregion

		#region Repeater Callbacks

		private void RepeaterOnRepeat(object sender, EventArgs eventArgs)
		{
			float scale = m_Up ? 1.0f : -1.0f;
			LoadLevel = MathUtils.Clamp(LoadLevel + INCREMENT_DELTA * scale, 0.0f, 1.0f);
		}

		#endregion

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			addRow("LoadLevel", LoadLevel);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new GenericConsoleCommand<float>("SetLevel", "SetLevel <LEVEL>", f =>
			                                                                              {
				                                                                              StopRamping();
				                                                                              LoadLevel = f;
			                                                                              });

			yield return new ConsoleCommand("StartRaising", "Starts increasing the load level", () => StartRaising());
			yield return new ConsoleCommand("StartLowering", "Starts lowering the load level", () => StartLowering());
			yield return new ConsoleCommand("StopRamping", "Stops ramping the load level", () => StopRamping());
		}

		/// <summary>
		/// Workaround for unverifiable code warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		#endregion
	}
}
