using System;
using System.Collections.Generic;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice;
using ICD.Connect.Lighting.Lutron.Nwk.Devices.GrafikEyeNwk;
using ICD.Connect.Lighting.Lutron.Nwk.Integrations.Interfaces;

namespace ICD.Connect.Lighting.Lutron.Nwk.Integrations.GrafikEyeIntegrations
{
	public sealed class GrafikEyeZoneIntegration : AbstractGrafikEyeIntegration, IZoneIntegration
	{
		private const double TOLERANCE = .1f;

		/// <summary>
		/// Raised when the light output level changes.
		/// </summary>
		public event EventHandler<FloatEventArgs> OnOutputLevelChanged;

		private float m_OutputLevel;

		/// <summary>
		/// Gets the light output level as a percentage 0.0f to 1.0f.
		/// </summary>
		public float OutputLevel
		{
			get { return m_OutputLevel; }
			internal set
			{
				if (Math.Abs(m_OutputLevel - value) < TOLERANCE)
					return;

				m_OutputLevel = value;

				OnOutputLevelChanged.Raise(this, value);
			}
		}

		private GrafikEyeZoneIntegration(int integrationId, string name, GrafikEyeRoom room, ILutronNwkDevice parent) :
			base(integrationId, name, parent, room)
		{
		}

		/// <summary>
		/// Instantiates a ZoneIntegration from xml.
		/// </summary>
		/// <param name="xml"></param>
		/// <param name="room"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static GrafikEyeZoneIntegration FromXml(string xml, GrafikEyeRoom room, ILutronNwkDevice parent)
		{
			int integrationId = GetIntegrationIdIntFromXml(xml);
			string name = GetNameFromXml(xml);

			return new GrafikEyeZoneIntegration(integrationId, name, room, parent);
		}

		/// <summary>
		/// Sets the light level for the zone.
		/// </summary>
		/// <param name="percentage">Percentage between 0.0f and 1.0f</param>
		public void SetOutputLevel(float percentage)
		{
			Room.GrafikEye.ZoneSetLevel(IntegrationId, percentage);
		}

		/// <summary>
		/// Sets the light level for the zone.
		/// </summary>
		/// <param name="percentage">Percentage between 0.0f and 1.0f</param>
		/// <param name="fade">Hours, minutes and seconds for the duration of the ramp</param>
		/// <param name="delay">Hours, minutes and seconds before starting to ramp</param>
		public void SetOutputLevel(float percentage, TimeSpan fade, TimeSpan delay)
		{
			Room.GrafikEye.ZoneSetLevel(IntegrationId, percentage, fade, delay);
		}

		/// <summary>
		/// Starts raising the zone output level.
		/// </summary>
		public void StartRaising()
		{
			Room.GrafikEye.ZoneStartRaising(IntegrationId);
		}

		/// <summary>
		/// Starts lowering the zone output level.
		/// </summary>
		public void StartLowering()
		{
			Room.GrafikEye.ZoneStartLowering(IntegrationId);
		}

		/// <summary>
		/// Stops raising/lowering the zone output level.
		/// </summary>
		public void StopRaisingLowering()
		{
			Room.GrafikEye.ZoneStopRaisingLowering(IntegrationId);
		}

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
