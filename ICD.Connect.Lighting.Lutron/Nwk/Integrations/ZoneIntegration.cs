﻿using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice;
using ICD.Connect.Lighting.Lutron.Nwk.Integrations.Abstracts;
using ICD.Connect.Lighting.Lutron.Nwk.Integrations.Interfaces;

namespace ICD.Connect.Lighting.Lutron.Nwk.Integrations
{
	public sealed class ZoneIntegration : AbstractIntegrationWithoutComponent, IZoneIntegration
	{
		private const int ACTION_OUTPUT_LEVEL = 1;
		private const int ACTION_START_RAISING = 2;
		private const int ACTION_START_LOWERING = 3;
		private const int ACTION_STOP_RAISING_LOWERING = 4;

		/// <summary>
		/// Raised when the light output level changes.
		/// </summary>
		[PublicAPI]
		public event EventHandler<FloatEventArgs> OnOutputLevelChanged;

		private float m_OutputLevel;

		#region Properties

		/// <summary>
		/// Gets the light output level as a percentage 0.0f to 1.0f.
		/// </summary>
		[PublicAPI]
		public float OutputLevel
		{
			get { return m_OutputLevel; }
			private set
			{
				if (Math.Abs(value - m_OutputLevel) < LutronUtils.FLOAT_TOLERANCE)
					return;

				m_OutputLevel = value;

				OnOutputLevelChanged.Raise(this, new FloatEventArgs(m_OutputLevel));
			}
		}

		/// <summary>
		/// The string prefix for communication with the lighting processor, e.g. SHADES.
		/// </summary>
		protected override string Command { get { return LutronUtils.COMMAND_OUTPUT; } }

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="integrationId"></param>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		private ZoneIntegration(int integrationId, string name, ILutronNwkDevice parent)
			: base(integrationId, name, parent)
		{
		}

		/// <summary>
		/// Instantiates a ZoneIntegration from xml.
		/// </summary>
		/// <param name="xml"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static ZoneIntegration FromXml(string xml, ILutronNwkDevice parent)
		{
			int integrationId = GetIntegrationIdIntFromXml(xml);
			string name = GetNameFromXml(xml);

			return new ZoneIntegration(integrationId, name, parent);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Sets the light level for the zone.
		/// </summary>
		/// <param name="percentage">Percentage between 0.0f and 1.0f</param>
		[PublicAPI]
		public void SetOutputLevel(float percentage)
		{
			SetOutputLevel(percentage, new TimeSpan(), new TimeSpan());
		}

		/// <summary>
		/// Sets the light level for the zone.
		/// </summary>
		/// <param name="percentage">Percentage between 0.0f and 1.0f</param>
		/// <param name="fade">Hours, minutes and seconds for the duration of the ramp</param>
		/// <param name="delay">Hours, minutes and seconds before starting to ramp</param>
		[PublicAPI]
		public void SetOutputLevel(float percentage, TimeSpan fade, TimeSpan delay)
		{
			string percentageParam = LutronUtils.FloatToPercentageParameter(percentage);
			string fadeParam = LutronUtils.TimeSpanToParameter(fade);
			string delayParam = LutronUtils.TimeSpanToParameter(delay);

			Execute(ACTION_OUTPUT_LEVEL, percentageParam, fadeParam, delayParam);
		}

		/// <summary>
		/// Starts raising the zone output level.
		/// </summary>
		[PublicAPI]
		public void StartRaising()
		{
			Execute(ACTION_START_RAISING);
		}

		/// <summary>
		/// Starts lowering the zone output level.
		/// </summary>
		[PublicAPI]
		public void StartLowering()
		{
			Execute(ACTION_START_LOWERING);
		}

		/// <summary>
		/// Stops raising/lowering the zone output level.
		/// </summary>
		[PublicAPI]
		public void StopRaisingLowering()
		{
			Execute(ACTION_STOP_RAISING_LOWERING);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Override to query the device when it goes online.
		/// </summary>
		protected override void Initialize()
		{
			base.Initialize();

			Query(ACTION_OUTPUT_LEVEL);
		}

		/// <summary>
		/// Called when we receive a response from the lighting processor for this integration.
		/// </summary>
		/// <param name="action"></param>
		/// <param name="parameters"></param>
		protected override void ParentOnResponse(int action, string[] parameters)
		{
			base.ParentOnResponse(action, parameters);

			switch (action)
			{
				case ACTION_OUTPUT_LEVEL:
					OutputLevel = LutronUtils.PercentageParameterToFloat(parameters[0]);
					break;
			}
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

			yield return new GenericConsoleCommand<float>("SetLevel", "Set the level for this zone", l => SetOutputLevel(l));
			yield return new ConsoleCommand("StartRaising", "Start raising the level", () => StartRaising());
			yield return new ConsoleCommand("StartLowering", "Start lowering the level", () => StartLowering());
			yield return new ConsoleCommand("Stop", "Stop raise/lower operations", () => StopRaisingLowering());
		}

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
			addRow("Output Level", OutputLevel.ToString("n2"));
		}

		#endregion
	}
}
