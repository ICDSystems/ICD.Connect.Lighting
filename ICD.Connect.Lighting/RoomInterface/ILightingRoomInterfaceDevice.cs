using System;
using System.Collections.Generic;
using System.Text;
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
		/// <param name="room"></param>
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
}
