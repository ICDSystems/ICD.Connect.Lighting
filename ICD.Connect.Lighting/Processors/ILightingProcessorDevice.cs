using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Devices;
using ICD.Connect.Lighting.EventArguments;
using ICD.Connect.Misc.Occupancy;

namespace ICD.Connect.Lighting.Processors
{
	/// <summary>
	/// ILightingProcessorDevice represents a lighting system with controls for
	/// one or more sub-systems, including lighting, shades and occupancy sensors.
	/// </summary>
	public interface ILightingProcessorDevice : IDevice
	{
		/// <summary>
		/// Raised when an room occupancy state changes.
		/// </summary>
		[PublicAPI]
		event EventHandler<RoomOccupancyEventArgs> OnRoomOccupancyChanged;

		/// <summary>
		/// Raised when an room preset changes.
		/// </summary>
		[PublicAPI]
		event EventHandler<RoomPresetChangeEventArgs> OnRoomPresetChanged;

		/// <summary>
		/// Raised when a lighting load level changes.
		/// </summary>
		[PublicAPI]
		event EventHandler<RoomLoadLevelEventArgs> OnRoomLoadLevelChanged;

		/// <summary>
		/// Raised whenever a control is added or removed from the processor.
		/// </summary>
		[PublicAPI]
		event EventHandler<IntEventArgs> OnRoomControlsChanged;

		#region Methods

		/// <summary>
		/// Gets the available rooms.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		IEnumerable<int> GetRooms();

		/// <summary>
		/// Returns true if the given room is available.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		[PublicAPI]
		bool ContainsRoom(int room);

		/// <summary>
		/// Gets the available light loads for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		[PublicAPI]
		IEnumerable<LightingProcessorControl> GetLoadsForRoom(int room);

		/// <summary>
		/// Gets the available individual shades for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		[PublicAPI]
		IEnumerable<LightingProcessorControl> GetShadesForRoom(int room);

		/// <summary>
		/// Gets the available shade groups for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		[PublicAPI]
		IEnumerable<LightingProcessorControl> GetShadeGroupsForRoom(int room);

		/// <summary>
		/// Gets the available presets for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		[PublicAPI]
		IEnumerable<LightingProcessorControl> GetPresetsForRoom(int room);

		#endregion

		#region Room

		/// <summary>
		/// Gets the current occupancy state for the given room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		[PublicAPI]
		eOccupancyState GetOccupancyForRoom(int room);

		/// <summary>
		/// Sets the preset for the given room.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="preset"></param>
		[PublicAPI]
		void SetPresetForRoom(int room, int? preset);

		/// <summary>
		/// Gets the current preset for the given room.
		/// </summary>
		/// <param name="room"></param>
		[PublicAPI]
		int? GetPresetForRoom(int room);

		#endregion

		#region Load

		/// <summary>
		/// Sets the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		/// <param name="percentage"></param>
		[PublicAPI]
		void SetLoadLevel(int room, int load, float percentage);

		/// <summary>
		/// Gets the current lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		[PublicAPI]
		float GetLoadLevel(int room, int load);

		/// <summary>
		/// Starts raising the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		[PublicAPI]
		void StartRaisingLoadLevel(int room, int load);

		/// <summary>
		/// Starts lowering the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		[PublicAPI]
		void StartLoweringLoadLevel(int room, int load);

		/// <summary>
		/// Stops raising/lowering the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		[PublicAPI]
		void StopRampingLoadLevel(int room, int load);

		#endregion

		#region Shade

		/// <summary>
		/// Starts raising the shade.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shade"></param>
		[PublicAPI]
		void StartRaisingShade(int room, int shade);

		/// <summary>
		/// Starts lowering the shade.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shade"></param>
		[PublicAPI]
		void StartLoweringShade(int room, int shade);

		/// <summary>
		/// Stops moving the shade.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shade"></param>
		[PublicAPI]
		void StopMovingShade(int room, int shade);

		#endregion

		#region Shade Group

		/// <summary>
		/// Starts raising the shade group.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shadeGroup"></param>
		[PublicAPI]
		void StartRaisingShadeGroup(int room, int shadeGroup);

		/// <summary>
		/// Starts lowering the shade group.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shadeGroup"></param>
		[PublicAPI]
		void StartLoweringShadeGroup(int room, int shadeGroup);

		/// <summary>
		/// Stops moving the shade group.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shadeGroup"></param>
		[PublicAPI]
		void StopMovingShadeGroup(int room, int shadeGroup);

		#endregion
	}

	/// <summary>
	/// Extension methods for the ILightingProcessorDevice.
	/// </summary>
	public static class LightingProcessorDeviceExtensions
	{
		/// <summary>
		/// Sets the preset for the given room.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="preset"></param>
		public static void SetPresetForRoom(this ILightingProcessorDevice extends,
		                                    LightingProcessorControl preset)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			extends.SetPresetForRoom(preset.Room, preset.Id);
		}

		#region Load

		/// <summary>
		/// Sets the lighting level for the given load.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="load"></param>
		/// <param name="percentage"></param>
		public static void SetLoadLevel(this ILightingProcessorDevice extends,
		                                LightingProcessorControl load, float percentage)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			extends.SetLoadLevel(load.Room, load.Id, percentage);
		}

		/// <summary>
		/// Gets the current lighting level for the given load.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="load"></param>
		public static float GetLoadLevel(this ILightingProcessorDevice extends,
		                                 LightingProcessorControl load)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.GetLoadLevel(load.Room, load.Id);
		}

		/// <summary>
		/// Starts raising the lighting level for the given load.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="load"></param>
		public static void StartRaisingLoadLevel(this ILightingProcessorDevice extends,
		                                         LightingProcessorControl load)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			extends.StartRaisingLoadLevel(load.Room, load.Id);
		}

		/// <summary>
		/// Starts lowering the lighting level for the given load.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="load"></param>
		public static void StartLoweringLoadLevel(this ILightingProcessorDevice extends,
		                                          LightingProcessorControl load)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			extends.StartLoweringLoadLevel(load.Room, load.Id);
		}

		/// <summary>
		/// Stops raising/lowering the lighting level for the given load.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="load"></param>
		public static void StopRampingLoadLevel(this ILightingProcessorDevice extends,
		                                        LightingProcessorControl load)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			extends.StopRampingLoadLevel(load.Room, load.Id);
		}

		#endregion

		#region Shade

		/// <summary>
		/// Starts raising the shade.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="shade"></param>
		public static void StartRaisingShade(this ILightingProcessorDevice extends,
		                                     LightingProcessorControl shade)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			extends.StartRaisingShade(shade.Room, shade.Id);
		}

		/// <summary>
		/// Starts lowering the shade.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="shade"></param>
		public static void StartLoweringShade(this ILightingProcessorDevice extends,
		                                      LightingProcessorControl shade)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			extends.StartLoweringShade(shade.Room, shade.Id);
		}

		/// <summary>
		/// Stops moving the shade.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="shade"></param>
		public static void StopMovingShade(this ILightingProcessorDevice extends,
		                                   LightingProcessorControl shade)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			extends.StopMovingShade(shade.Room, shade.Id);
		}

		#endregion

		#region Shade Group

		/// <summary>
		/// Starts raising the shade group.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="shadeGroup"></param>
		public static void StartRaisingShadeGroup(this ILightingProcessorDevice extends,
		                                          LightingProcessorControl shadeGroup)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			extends.StartRaisingShadeGroup(shadeGroup.Room, shadeGroup.Id);
		}

		/// <summary>
		/// Starts lowering the shade group.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="shadeGroup"></param>
		public static void StartLoweringShadeGroup(this ILightingProcessorDevice extends,
		                                           LightingProcessorControl shadeGroup)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			extends.StartLoweringShadeGroup(shadeGroup.Room, shadeGroup.Id);
		}

		/// <summary>
		/// Stops moving the shade group.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="shadeGroup"></param>
		public static void StopMovingShadeGroup(this ILightingProcessorDevice extends,
		                                        LightingProcessorControl shadeGroup)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			extends.StopMovingShadeGroup(shadeGroup.Room, shadeGroup.Id);
		}

		#endregion
	}
}
