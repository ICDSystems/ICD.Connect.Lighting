using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Lighting.Processors;
using ICD.Connect.Misc.Occupancy;
using ICD.Common.Utils.Extensions;

namespace ICD.Connect.Lighting.CrestronPro.EiscLightingAdapter
{
	public sealed partial class EiscLightingAdapterDevice : ILightingProcessorDevice
	{
		/// <summary>
		/// Gets the available rooms.
		/// </summary>
		/// <returns></returns>
		IEnumerable<int> ILightingProcessorDevice.GetRooms()
		{
			return m_RoomsSection.Execute(() => m_Rooms.Keys.ToArray(m_Rooms.Count));
		}

		/// <summary>
		/// Returns true if the given room is available.
		/// </summary>
		/// <param name="roomId"></param>
		/// <returns></returns>
		bool ILightingProcessorDevice.ContainsRoom(int roomId)
		{
			return m_RoomsSection.Execute(() => m_Rooms.ContainsKey(roomId));
		}

		/// <summary>
		/// Gets the available light loads for the room.
		/// </summary>
		/// <param name="roomId"></param>
		/// <returns></returns>
		IEnumerable<LightingProcessorControl> ILightingProcessorDevice.GetLoadsForRoom(int roomId)
		{
			EiscLightingRoom room = null;

			if (m_RoomsSection.Execute(() => m_Rooms.TryGetValue(roomId, out room)))
				return room.GetLoads();

			Logger.Log(eSeverity.Error, "No room with id {0}", roomId);
			return Enumerable.Empty<LightingProcessorControl>();
		}

		/// <summary>
		/// Gets the available individual shades for the room.
		/// </summary>
		/// <param name="roomId"></param>
		/// <returns></returns>
		IEnumerable<LightingProcessorControl> ILightingProcessorDevice.GetShadesForRoom(int roomId)
		{
			EiscLightingRoom room = null;

			if (m_RoomsSection.Execute(() => m_Rooms.TryGetValue(roomId, out room)))
				return room.GetShades();

			Logger.Log(eSeverity.Error, "No room with id {0}", roomId);
			return Enumerable.Empty<LightingProcessorControl>();
		}

		/// <summary>
		/// Gets the available shade groups for the room.
		/// </summary>
		/// <param name="roomId"></param>
		/// <returns></returns>
		IEnumerable<LightingProcessorControl> ILightingProcessorDevice.GetShadeGroupsForRoom(int roomId)
		{
			return Enumerable.Empty<LightingProcessorControl>();
		}

		/// <summary>
		/// Gets the available presets for the room.
		/// </summary>
		/// <param name="roomId"></param>
		/// <returns></returns>
		IEnumerable<LightingProcessorControl> ILightingProcessorDevice.GetPresetsForRoom(int roomId)
		{
			EiscLightingRoom room = null;

			if (m_RoomsSection.Execute(() => m_Rooms.TryGetValue(roomId, out room)))
				return room.GetPresets();

			Logger.Log(eSeverity.Error, "No room with id {0}", roomId);
			return Enumerable.Empty<LightingProcessorControl>();
		}

		/// <summary>
		/// Gets the current occupancy state for the given room.
		/// </summary>
		/// <param name="roomId"></param>
		/// <returns></returns>
		eOccupancyState ILightingProcessorDevice.GetOccupancyForRoom(int roomId)
		{
			EiscLightingRoom room = null;
			if (m_RoomsSection.Execute(() => m_Rooms.TryGetValue(roomId, out room)))
				return room.Occupancy;

			Logger.Log(eSeverity.Error, "No room with id {0}", roomId);
			return eOccupancyState.Unknown;
		}

		/// <summary>
		/// Sets the preset for the given room.
		/// </summary>
		/// <param name="roomId"></param>
		/// <param name="preset"></param>
		void ILightingProcessorDevice.SetPresetForRoom(int roomId, int? preset)
		{
			EiscLightingRoom room = null;
			if (m_RoomsSection.Execute(() => m_Rooms.TryGetValue(roomId, out room)))
			{
				room.SetPreset(preset);
				return;
			}

			Logger.Log(eSeverity.Error, "No room with id {0}", roomId);
		}

		/// <summary>
		/// Gets the current preset for the given room.
		/// </summary>
		/// <param name="roomId"></param>
		int? ILightingProcessorDevice.GetPresetForRoom(int roomId)
		{
			EiscLightingRoom room = null;
			if (m_RoomsSection.Execute(() => m_Rooms.TryGetValue(roomId, out room)))
				return room.ActivePreset;

			Logger.Log(eSeverity.Error, "No room with id {0}", roomId);
			return null;
		}

		/// <summary>
		/// Sets the lighting level for the given load.
		/// </summary>
		/// <param name="roomId"></param>
		/// <param name="loadId"></param>
		/// <param name="percentage"></param>
		void ILightingProcessorDevice.SetLoadLevel(int roomId, int loadId, float percentage)
		{
			EiscLightingRoom room = null;
			if (m_RoomsSection.Execute(() => m_Rooms.TryGetValue(roomId, out room)))
			{
				room.SetLoadLevel(loadId, percentage);
				return;
			}

			Logger.Log(eSeverity.Error, "No room with id {0}", roomId);
		}

		/// <summary>
		/// Gets the current lighting level for the given load.
		/// </summary>
		/// <param name="roomId"></param>
		/// <param name="loadId"></param>
		float ILightingProcessorDevice.GetLoadLevel(int roomId, int loadId)
		{
			EiscLightingRoom room = null;
			if (m_RoomsSection.Execute(() => m_Rooms.TryGetValue(roomId, out room)))
				return room.GetLoadLevel(loadId);

			Logger.Log(eSeverity.Error, "No room with id {0}", roomId);
			return 0;
		}

		/// <summary>
		/// Starts raising the lighting level for the given load.
		/// </summary>
		/// <param name="roomId"></param>
		/// <param name="loadId"></param>
		void ILightingProcessorDevice.StartRaisingLoadLevel(int roomId, int loadId)
		{
			EiscLightingRoom room = null;
			if (m_RoomsSection.Execute(() => m_Rooms.TryGetValue(roomId, out room)))
			{
				room.StartRaisingLoadLevel(loadId);
				return;
			}

			Logger.Log(eSeverity.Error, "No room with id {0}", roomId);
		}

		/// <summary>
		/// Starts lowering the lighting level for the given load.
		/// </summary>
		/// <param name="roomId"></param>
		/// <param name="loadId"></param>
		void ILightingProcessorDevice.StartLoweringLoadLevel(int roomId, int loadId)
		{
			EiscLightingRoom room = null;
			if (m_RoomsSection.Execute(() => m_Rooms.TryGetValue(roomId, out room)))
			{
				room.StartLoweringLoadLevel(loadId);
				return;
			}

			Logger.Log(eSeverity.Error, "No room with id {0}", roomId);
		}

		/// <summary>
		/// Stops raising/lowering the lighting level for the given load.
		/// </summary>
		/// <param name="roomId"></param>
		/// <param name="loadId"></param>
		void ILightingProcessorDevice.StopRampingLoadLevel(int roomId, int loadId)
		{
			EiscLightingRoom room = null;
			if (m_RoomsSection.Execute(() => m_Rooms.TryGetValue(roomId, out room)))
			{
				room.StopRampingLoadLevel(loadId);
				return;
			}

			Logger.Log(eSeverity.Error, "No room with id {0}", roomId);
		}

		/// <summary>
		/// Starts raising the shade.
		/// </summary>
		/// <param name="roomId"></param>
		/// <param name="shadeId"></param>
		void ILightingProcessorDevice.StartRaisingShade(int roomId, int shadeId)
		{
			EiscLightingRoom room = null;
			if (m_RoomsSection.Execute(() => m_Rooms.TryGetValue(roomId, out room)))
			{
				room.OpenShade(shadeId);
				return;
			}

			Logger.Log(eSeverity.Error, "No room with id {0}", roomId);
		}

		/// <summary>
		/// Starts lowering the shade.
		/// </summary>
		/// <param name="roomId"></param>
		/// <param name="shadeId"></param>
		void ILightingProcessorDevice.StartLoweringShade(int roomId, int shadeId)
		{
			EiscLightingRoom room = null;
			if (m_RoomsSection.Execute(() => m_Rooms.TryGetValue(roomId, out room)))
			{
				room.CloseShade(shadeId);
				return;
			}

			Logger.Log(eSeverity.Error, "No room with id {0}", roomId);
		}

		/// <summary>
		/// Stops moving the shade.
		/// </summary>
		/// <param name="roomId"></param>
		/// <param name="shadeId"></param>
		void ILightingProcessorDevice.StopMovingShade(int roomId, int shadeId)
		{
			EiscLightingRoom room = null;
			if (m_RoomsSection.Execute(() => m_Rooms.TryGetValue(roomId, out room)))
			{
				room.StopShade(shadeId);
				return;
			}

			Logger.Log(eSeverity.Error, "No room with id {0}", roomId);
		}

		/// <summary>
		/// Starts raising the shade group.
		/// </summary>
		/// <param name="roomId"></param>
		/// <param name="shadeGroupId"></param>
		void ILightingProcessorDevice.StartRaisingShadeGroup(int roomId, int shadeGroupId)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Starts lowering the shade group.
		/// </summary>
		/// <param name="roomId"></param>
		/// <param name="shadeGroupId"></param>
		void ILightingProcessorDevice.StartLoweringShadeGroup(int roomId, int shadeGroupId)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Stops moving the shade group.
		/// </summary>
		/// <param name="roomId"></param>
		/// <param name="shadeGroupId"></param>
		void ILightingProcessorDevice.StopMovingShadeGroup(int roomId, int shadeGroupId)
		{
			throw new NotImplementedException();
		}
	}
}