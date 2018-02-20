using System;
using System.Collections.Generic;
using ICD.Connect.Lighting.EventArguments;

namespace ICD.Connect.Lighting.Environment
{
	public interface IEnvironmentRoom
	{
		event EventHandler<RoomPresetChangeEventArgs> OnActivePresetChanged;
		event EventHandler<RoomOccupancyEventArgs> OnOccupancyChanged;
		event EventHandler<RoomLoadLevelEventArgs> OnLoadLevelChanged;
		event EventHandler OnControlsChanged;

		/// <summary>
		/// Gets the room id.
		/// </summary>
		int Id { get; }

		/// <summary>
		/// Gets/sets the active preset.
		/// </summary>
		int? ActivePreset { get; set; }

		/// <summary>
		/// Gets/sets the room occupancy state.
		/// </summary>
		RoomOccupancyEventArgs.eOccupancyState Occupancy { get; set; }

		void AddLoad(ILightingLoadEnvironmentPeripheral load);
		void AddShade(IShadeEnvironmentPeripheral shade);
		void AddShadeGroup(IShadeEnvironmentPeripheral shadeGroup);
		void AddPreset(IPresetEnvironmentPeripheral preset);

		/// <summary>
		/// Convenience method for adding a LightingProcessorControl to the room.
		/// </summary>
		/// <param name="control"></param>
		void AddControl(LightingProcessorControl control);

		ILightingLoadEnvironmentPeripheral GetLoad(int load);
		IShadeEnvironmentPeripheral GetShade(int shade);
		IShadeEnvironmentPeripheral GetShadeGroup(int shadeGroup);
		IPresetEnvironmentPeripheral GetPreset(int preset);
		IEnumerable<LightingProcessorControl> GetLoads();
		IEnumerable<LightingProcessorControl> GetShades();
		IEnumerable<LightingProcessorControl> GetShadeGroups();
		IEnumerable<LightingProcessorControl> GetPresets();
	}
}