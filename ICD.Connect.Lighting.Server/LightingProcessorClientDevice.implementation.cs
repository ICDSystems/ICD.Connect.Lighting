using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Lighting.EventArguments;
using ICD.Connect.Lighting.RoomInterface;
using ICD.Connect.Misc.Occupancy;

namespace ICD.Connect.Lighting.Server
{
	public sealed partial class LightingProcessorClientDevice : ILightingRoomInterfaceDevice
	{
		public event EventHandler<GenericEventArgs<eOccupancyState>> OnOccupancyChanged;
		public event EventHandler<GenericEventArgs<int?>> OnPresetChanged;
		public event EventHandler<LoadLevelEventArgs> OnLoadLevelChanged;
		public event EventHandler OnControlsChanged;

		/// <summary>
		/// Gets the available light loads for the room.
		/// </summary>
		/// <returns></returns>
		IEnumerable<LightingProcessorControl> ILightingRoomInterfaceDevice.GetLoads()
		{
			if (m_Room == null)
				return Enumerable.Empty<LightingProcessorControl>();
			return GetLoads();
		}

		/// <summary>
		/// Gets the available individual shades for the room.
		/// </summary>
		/// <returns></returns>
		IEnumerable<LightingProcessorControl> ILightingRoomInterfaceDevice.GetShades()
		{
			if (m_Room == null)
				return Enumerable.Empty<LightingProcessorControl>();
			return GetShades();
		}

		/// <summary>
		/// Gets the available shade groups for the room.
		/// </summary>
		/// <returns></returns>
		IEnumerable<LightingProcessorControl> ILightingRoomInterfaceDevice.GetShadeGroups()
		{
			if (m_Room == null)
				return Enumerable.Empty<LightingProcessorControl>();
			return GetShadeGroups();
		}

		/// <summary>
		/// Gets the available presets for the room.
		/// </summary>
		/// <returns></returns>
		IEnumerable<LightingProcessorControl> ILightingRoomInterfaceDevice.GetPresets()
		{
			if (m_Room == null)
				return Enumerable.Empty<LightingProcessorControl>();
			return GetPresets();
		}

		/// <summary>
		/// Gets the current occupancy state for the given room.
		/// </summary>
		/// <returns></returns>
		eOccupancyState ILightingRoomInterfaceDevice.GetOccupancy()
		{
			if (m_Room == null)
				return eOccupancyState.Unknown;
			return GetOccupancy();
		}

		/// <summary>
		/// Sets the preset for the given room.
		/// </summary>
		/// <param name="preset"></param>
		void ILightingRoomInterfaceDevice.SetPreset(int? preset)
		{
			if (m_Room == null)
				return;
			SetPreset(preset);
		}

		/// <summary>
		/// Gets the current preset for the given room.
		/// </summary>
		int? ILightingRoomInterfaceDevice.GetPreset()
		{
			if (m_Room == null)
				return null;
			return GetActivePreset();
		}

		/// <summary>
		/// Sets the lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		/// <param name="percentage"></param>
		void ILightingRoomInterfaceDevice.SetLoadLevel(int load, float percentage)
		{
			if (m_Room == null)
				return;
			SetLoadLevel(load, percentage);
		}

		/// <summary>
		/// Gets the current lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		float ILightingRoomInterfaceDevice.GetLoadLevel(int load)
		{
			if (m_Room == null)
				return 0;
			return GetLoadLevel(load);
		}

		/// <summary>
		/// Starts raising the lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		void ILightingRoomInterfaceDevice.StartRaisingLoadLevel(int load)
		{
			if (m_Room == null)
				return;
			StartRaisingLoadLevel(load);
		}

		/// <summary>
		/// Starts lowering the lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		void ILightingRoomInterfaceDevice.StartLoweringLoadLevel(int load)
		{
			if (m_Room == null)
				return;
			StartLoweringLoadLevel(load);
		}

		/// <summary>
		/// Stops raising/lowering the lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		void ILightingRoomInterfaceDevice.StopRampingLoadLevel(int load)
		{
			if (m_Room == null)
				return;
			StopRampingLoadLevel(load);
		}

		/// <summary>
		/// Starts raising the shade.
		/// </summary>
		/// <param name="shade"></param>
		void ILightingRoomInterfaceDevice.StartRaisingShade(int shade)
		{
			if (m_Room == null)
				return;
			StartRaisingShade(shade);
		}

		/// <summary>
		/// Starts lowering the shade.
		/// </summary>
		/// <param name="shade"></param>
		void ILightingRoomInterfaceDevice.StartLoweringShade(int shade)
		{
			if (m_Room == null)
				return;
			StartLoweringShade(shade);
		}

		/// <summary>
		/// Stops moving the shade.
		/// </summary>
		/// <param name="shade"></param>
		void ILightingRoomInterfaceDevice.StopMovingShade(int shade)
		{
			if (m_Room == null)
				return;
			StopMovingShade(shade);
		}

		/// <summary>
		/// Starts raising the shade group.
		/// </summary>
		/// <param name="shadeGroup"></param>
		void ILightingRoomInterfaceDevice.StartRaisingShadeGroup(int shadeGroup)
		{
			if (m_Room == null)
				return;
			StartRaisingShadeGroup(shadeGroup);
		}

		/// <summary>
		/// Starts lowering the shade group.
		/// </summary>
		/// <param name="shadeGroup"></param>
		void ILightingRoomInterfaceDevice.StartLoweringShadeGroup(int shadeGroup)
		{
			if (m_Room == null)
				return;
			StartLoweringShadeGroup(shadeGroup);
		}

		/// <summary>
		/// Stops moving the shade group.
		/// </summary>
		/// <param name="shadeGroup"></param>
		void ILightingRoomInterfaceDevice.StopMovingShadeGroup(int shadeGroup)
		{
			if (m_Room == null)
				return;
			StopMovingShadeGroup(shadeGroup);
		}
	}
}
