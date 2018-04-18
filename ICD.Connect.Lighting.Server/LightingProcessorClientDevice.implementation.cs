using System.Collections.Generic;
using System.Linq;
using ICD.Connect.Lighting.Processors;
using ICD.Connect.Misc.Occupancy;

namespace ICD.Connect.Lighting.Server
{
	public sealed partial class LightingProcessorClientDevice : ILightingProcessorDevice
	{
		/// <summary>
		/// Gets the available rooms.
		/// </summary>
		/// <returns></returns>
		IEnumerable<int> ILightingProcessorDevice.GetRooms()
		{
			if (m_Room != null)
				yield return m_Room.Id;
		}

		/// <summary>
		/// Returns true if the given room is available.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		bool ILightingProcessorDevice.ContainsRoom(int room)
		{
			return m_Room != null;
		}

		/// <summary>
		/// Gets the available light loads for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		IEnumerable<LightingProcessorControl> ILightingProcessorDevice.GetLoadsForRoom(int room)
		{
			if (m_Room == null)
				return Enumerable.Empty<LightingProcessorControl>();
			return GetLoads();
		}

		/// <summary>
		/// Gets the available individual shades for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		IEnumerable<LightingProcessorControl> ILightingProcessorDevice.GetShadesForRoom(int room)
		{
			if (m_Room == null)
				return Enumerable.Empty<LightingProcessorControl>();
			return GetShades();
		}

		/// <summary>
		/// Gets the available shade groups for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		IEnumerable<LightingProcessorControl> ILightingProcessorDevice.GetShadeGroupsForRoom(int room)
		{
			if (m_Room == null)
				return Enumerable.Empty<LightingProcessorControl>();
			return GetShadeGroups();
		}

		/// <summary>
		/// Gets the available presets for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		IEnumerable<LightingProcessorControl> ILightingProcessorDevice.GetPresetsForRoom(int room)
		{
			if (m_Room == null)
				return Enumerable.Empty<LightingProcessorControl>();
			return GetPresets();
		}

		/// <summary>
		/// Gets the current occupancy state for the given room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		eOccupancyState ILightingProcessorDevice.GetOccupancyForRoom(int room)
		{
			if (m_Room == null)
				return eOccupancyState.Unknown;
			return GetOccupancy();
		}

		/// <summary>
		/// Sets the preset for the given room.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="preset"></param>
		void ILightingProcessorDevice.SetPresetForRoom(int room, int? preset)
		{
			if (m_Room == null)
				return;
			SetPreset(preset);
		}

		/// <summary>
		/// Gets the current preset for the given room.
		/// </summary>
		/// <param name="room"></param>
		int? ILightingProcessorDevice.GetPresetForRoom(int room)
		{
			if (m_Room == null)
				return null;
			return GetActivePreset();
		}

		/// <summary>
		/// Sets the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		/// <param name="percentage"></param>
		void ILightingProcessorDevice.SetLoadLevel(int room, int load, float percentage)
		{
			if (m_Room == null)
				return;
			SetLoadLevel(load, percentage);
		}

		/// <summary>
		/// Gets the current lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		float ILightingProcessorDevice.GetLoadLevel(int room, int load)
		{
			if (m_Room == null)
				return 0;
			return GetLoadLevel(load);
		}

		/// <summary>
		/// Starts raising the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		void ILightingProcessorDevice.StartRaisingLoadLevel(int room, int load)
		{
			if (m_Room == null)
				return;
			StartRaisingLoadLevel(load);
		}

		/// <summary>
		/// Starts lowering the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		void ILightingProcessorDevice.StartLoweringLoadLevel(int room, int load)
		{
			if (m_Room == null)
				return;
			StartLoweringLoadLevel(load);
		}

		/// <summary>
		/// Stops raising/lowering the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		void ILightingProcessorDevice.StopRampingLoadLevel(int room, int load)
		{
			if (m_Room == null)
				return;
			StopRampingLoadLevel(load);
		}

		/// <summary>
		/// Starts raising the shade.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shade"></param>
		void ILightingProcessorDevice.StartRaisingShade(int room, int shade)
		{
			if (m_Room == null)
				return;
			StartRaisingShade(shade);
		}

		/// <summary>
		/// Starts lowering the shade.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shade"></param>
		void ILightingProcessorDevice.StartLoweringShade(int room, int shade)
		{
			if (m_Room == null)
				return;
			StartLoweringShade(shade);
		}

		/// <summary>
		/// Stops moving the shade.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shade"></param>
		void ILightingProcessorDevice.StopMovingShade(int room, int shade)
		{
			if (m_Room == null)
				return;
			StopMovingShade(shade);
		}

		/// <summary>
		/// Starts raising the shade group.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shadeGroup"></param>
		void ILightingProcessorDevice.StartRaisingShadeGroup(int room, int shadeGroup)
		{
			if (m_Room == null)
				return;
			StartRaisingShadeGroup(shadeGroup);
		}

		/// <summary>
		/// Starts lowering the shade group.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shadeGroup"></param>
		void ILightingProcessorDevice.StartLoweringShadeGroup(int room, int shadeGroup)
		{
			if (m_Room == null)
				return;
			StartLoweringShadeGroup(shadeGroup);
		}

		/// <summary>
		/// Stops moving the shade group.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shadeGroup"></param>
		void ILightingProcessorDevice.StopMovingShadeGroup(int room, int shadeGroup)
		{
			if (m_Room == null)
				return;
			StopMovingShadeGroup(shadeGroup);
		}
	}
}
