using System;
using ICD.Common.Utils.EventArguments;

namespace ICD.Connect.Lighting.Lutron.Nwk.Integrations.Interfaces
{
	public interface IZoneIntegration : IIntegration<int>
	{
		/// <summary>
		/// Raised when the light output level changes.
		/// </summary>
		event EventHandler<FloatEventArgs> OnOutputLevelChanged;

		/// <summary>
		/// Gets the light output level as a percentage 0.0f to 1.0f.
		/// </summary>
		float OutputLevel { get; }

		/// <summary>
		/// Sets the light level for the zone.
		/// </summary>
		/// <param name="percentage">Percentage between 0.0f and 1.0f</param>
		void SetOutputLevel(float percentage);

		/// <summary>
		/// Sets the light level for the zone.
		/// </summary>
		/// <param name="percentage">Percentage between 0.0f and 1.0f</param>
		/// <param name="fade">Hours, minutes and seconds for the duration of the ramp</param>
		/// <param name="delay">Hours, minutes and seconds before starting to ramp</param>
		void SetOutputLevel(float percentage, TimeSpan fade, TimeSpan delay);

		/// <summary>
		/// Starts raising the zone output level.
		/// </summary>
		void StartRaising();

		/// <summary>
		/// Starts lowering the zone output level.
		/// </summary>
		void StartLowering();

		/// <summary>
		/// Stops raising/lowering the zone output level.
		/// </summary>
		void StopRaisingLowering();
	}
}