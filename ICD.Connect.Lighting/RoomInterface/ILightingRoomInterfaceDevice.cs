using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Devices;
using ICD.Connect.Lighting.EventArguments;
using ICD.Connect.Misc.Occupancy;

namespace ICD.Connect.Lighting.RoomInterface
{
	public interface ILightingRoomInterfaceDevice : IDevice
	{
		/// <summary>
		/// Raised when occupancy state changes.
		/// </summary>
		[PublicAPI]
		event EventHandler<GenericEventArgs<eOccupancyState>> OnOccupancyChanged;

		/// <summary>
		/// Raised when preset changes.
		/// </summary>
		[PublicAPI]
		event EventHandler<GenericEventArgs<int?>> OnPresetChanged;

		/// <summary>
		/// Raised when a lighting load level changes.
		/// </summary>
		[PublicAPI]
		event EventHandler<LoadLevelEventArgs> OnLoadLevelChanged;

		/// <summary>
		/// Raised whenever a control is added or removed from the room.
		/// </summary>
		[PublicAPI]
		event EventHandler OnControlsChanged;

		#region Methods

		/// <summary>
		/// Gets the available light loads for the room.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		IEnumerable<LightingProcessorControl> GetLoads();

		/// <summary>
		/// Gets the available individual shades for the room.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		IEnumerable<LightingProcessorControl> GetShades();

		/// <summary>
		/// Gets the available shade groups for the room.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		IEnumerable<LightingProcessorControl> GetShadeGroups();

		/// <summary>
		/// Gets the available presets for the room.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		IEnumerable<LightingProcessorControl> GetPresets();

		#endregion

		#region Room

		/// <summary>
		/// Gets the current occupancy state for the given room.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		eOccupancyState GetOccupancy();

		/// <summary>
		/// Sets the preset for the given room.
		/// </summary>
		/// <param name="preset"></param>
		[PublicAPI]
		void SetPreset(int? preset);

		/// <summary>
		/// Gets the current preset for the given room.
		/// </summary>
		[PublicAPI]
		int? GetPreset();

		#endregion

		#region Load

		/// <summary>
		/// Sets the lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		/// <param name="percentage"></param>
		[PublicAPI]
		void SetLoadLevel(int load, float percentage);

		/// <summary>
		/// Gets the current lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		[PublicAPI]
		float GetLoadLevel(int load);

		/// <summary>
		/// Starts raising the lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		[PublicAPI]
		void StartRaisingLoadLevel(int load);

		/// <summary>
		/// Starts lowering the lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		[PublicAPI]
		void StartLoweringLoadLevel(int load);

		/// <summary>
		/// Stops raising/lowering the lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		[PublicAPI]
		void StopRampingLoadLevel(int load);

		#endregion

		#region Shade

		/// <summary>
		/// Starts raising the shade.
		/// </summary>
		/// <param name="shade"></param>
		[PublicAPI]
		void StartRaisingShade(int shade);

		/// <summary>
		/// Starts lowering the shade.
		/// </summary>
		/// <param name="shade"></param>
		[PublicAPI]
		void StartLoweringShade(int shade);

		/// <summary>
		/// Stops moving the shade.
		/// </summary>
		/// <param name="shade"></param>
		[PublicAPI]
		void StopMovingShade(int shade);

		#endregion

		#region Shade Group

		/// <summary>
		/// Starts raising the shade group.
		/// </summary>
		/// <param name="shadeGroup"></param>
		[PublicAPI]
		void StartRaisingShadeGroup(int shadeGroup);

		/// <summary>
		/// Starts lowering the shade group.
		/// </summary>
		/// <param name="shadeGroup"></param>
		[PublicAPI]
		void StartLoweringShadeGroup(int shadeGroup);

		/// <summary>
		/// Stops moving the shade group.
		/// </summary>
		/// <param name="shadeGroup"></param>
		[PublicAPI]
		void StopMovingShadeGroup(int shadeGroup);

		#endregion
	}

	/// <summary>
	/// Extension methods for the ILightingProcessorDevice.
	/// </summary>
	public static class LightingRoomInterfaceDeviceExtensions
	{
		/// <summary>
		/// Sets the preset for the given room.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="preset"></param>
		public static void SetPresetForRoom(this ILightingRoomInterfaceDevice extends,
											LightingProcessorControl preset)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			extends.SetPreset(preset.Id);
		}

		#region Load

		/// <summary>
		/// Sets the lighting level for the given load.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="load"></param>
		/// <param name="percentage"></param>
		public static void SetLoadLevel(this ILightingRoomInterfaceDevice extends,
										LightingProcessorControl load, float percentage)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			extends.SetLoadLevel(load.Id, percentage);
		}

		/// <summary>
		/// Gets the current lighting level for the given load.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="load"></param>
		public static float GetLoadLevel(this ILightingRoomInterfaceDevice extends,
										 LightingProcessorControl load)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.GetLoadLevel(load.Id);
		}

		/// <summary>
		/// Starts raising the lighting level for the given load.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="load"></param>
		public static void StartRaisingLoadLevel(this ILightingRoomInterfaceDevice extends,
												 LightingProcessorControl load)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			extends.StartRaisingLoadLevel(load.Id);
		}

		/// <summary>
		/// Starts lowering the lighting level for the given load.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="load"></param>
		public static void StartLoweringLoadLevel(this ILightingRoomInterfaceDevice extends,
												  LightingProcessorControl load)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			extends.StartLoweringLoadLevel(load.Id);
		}

		/// <summary>
		/// Stops raising/lowering the lighting level for the given load.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="load"></param>
		public static void StopRampingLoadLevel(this ILightingRoomInterfaceDevice extends,
												LightingProcessorControl load)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			extends.StopRampingLoadLevel(load.Id);
		}

		#endregion

		#region Shade

		/// <summary>
		/// Starts raising the shade.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="shade"></param>
		public static void StartRaisingShade(this ILightingRoomInterfaceDevice extends,
											 LightingProcessorControl shade)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			extends.StartRaisingShade(shade.Id);
		}

		/// <summary>
		/// Starts lowering the shade.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="shade"></param>
		public static void StartLoweringShade(this ILightingRoomInterfaceDevice extends,
											  LightingProcessorControl shade)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			extends.StartLoweringShade(shade.Id);
		}

		/// <summary>
		/// Stops moving the shade.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="shade"></param>
		public static void StopMovingShade(this ILightingRoomInterfaceDevice extends,
										   LightingProcessorControl shade)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			extends.StopMovingShade(shade.Id);
		}

		#endregion

		#region Shade Group

		/// <summary>
		/// Starts raising the shade group.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="shadeGroup"></param>
		public static void StartRaisingShadeGroup(this ILightingRoomInterfaceDevice extends,
												  LightingProcessorControl shadeGroup)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			extends.StartRaisingShadeGroup(shadeGroup.Id);
		}

		/// <summary>
		/// Starts lowering the shade group.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="shadeGroup"></param>
		public static void StartLoweringShadeGroup(this ILightingRoomInterfaceDevice extends,
												   LightingProcessorControl shadeGroup)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			extends.StartLoweringShadeGroup(shadeGroup.Id);
		}

		/// <summary>
		/// Stops moving the shade group.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="shadeGroup"></param>
		public static void StopMovingShadeGroup(this ILightingRoomInterfaceDevice extends,
												LightingProcessorControl shadeGroup)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			extends.StopMovingShadeGroup(shadeGroup.Id);
		}

		#endregion
	}
}
