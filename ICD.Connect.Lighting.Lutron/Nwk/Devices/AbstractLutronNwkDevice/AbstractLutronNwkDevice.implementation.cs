using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Lighting.Lutron.Nwk.EventArguments;
using ICD.Connect.Lighting.Processors;

namespace ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice
{
	public abstract partial class AbstractLutronNwkDevice<T>
		where T : ILutronNwkDeviceSettings, new()
	{
		#region ILightingProcessorDevice Implementation

		/// <summary>
		/// Gets the available light loads for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		IEnumerable<LightingProcessorControl> ILightingProcessorDevice.GetLoadsForRoom(int room)
		{
			return GetRoomContainersForRoom(room)
				.SelectMany(a => a.GetZoneIntegrations())
				.Select(i => LightingProcessorControl.Load(i.IntegrationId, room, i.Name));
		}

		/// <summary>
		/// Gets the available individual shades for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		IEnumerable<LightingProcessorControl> ILightingProcessorDevice.GetShadesForRoom(int room)
		{
			return GetRoomContainersForRoom(room)
				.SelectMany(a => a.GetShadeIntegrations())
				.Select(i => LightingProcessorControl.Shade(i.IntegrationId, room, i.Name, i.ShadeType));
		}

		/// <summary>
		/// Gets the available shade groups for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		IEnumerable<LightingProcessorControl> ILightingProcessorDevice.GetShadeGroupsForRoom(int room)
		{
			return GetRoomContainersForRoom(room)
				.SelectMany(a => a.GetShadeGroupIntegrations())
				.Select(i => LightingProcessorControl.ShadeGroup(i.IntegrationId, room, i.Name, i.ShadeType));
		}

		/// <summary>
		/// Gets the available presets for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		IEnumerable<LightingProcessorControl> ILightingProcessorDevice.GetPresetsForRoom(int room)
		{
			return GetRoomContainersForRoom(room)
				.SelectMany(a => a.GetSceneIntegrations())
				.Select(i => LightingProcessorControl.Preset(i.IntegrationId, room, i.Name));
		}

		/// <summary>
		/// Gets the current occupancy state for the given room.
		/// Returns unknown if the room has multiple areas that have different occupancies.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		Partitioning.Commercial.Controls.Occupancy.eOccupancyState ILightingProcessorDevice.GetOccupancyForRoom(int room)
		{
			eOccupancyState state = GetRoomContainersForRoom(room).Select(a => a.OccupancyState)
			                                                        .Unanimous(eOccupancyState.Unknown);
			return GetOccupancyState(state);
		}

		/// <summary>
		/// Sets the preset for the given room.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="preset"></param>
		void ILightingProcessorDevice.SetPresetForRoom(int room, int? preset)
		{
			GetRoomContainersForRoom(room).ForEach(a => a.SetScene(preset));
		}

		/// <summary>
		/// Gets the current preset for the given room.
		/// Returns 0 if multuple areas for the given room, and the areas return different results.
		/// </summary>
		/// <param name="room"></param>
		int? ILightingProcessorDevice.GetPresetForRoom(int room)
		{
			return GetRoomContainersForRoom(room).Select(a => a.Scene)
			                                       .Unanimous(0);
		}

		/// <summary>
		/// Sets the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		/// <param name="percentage"></param>
		void ILightingProcessorDevice.SetLoadLevel(int room, int load, float percentage)
		{
			GetRoomContainersForRoom(room).Where(a => a.ContainsZoneIntegration(load))
			                                .Select(a => a.GetZoneIntegration(load))
			                                .ForEach(z => z.SetOutputLevel(percentage));
		}

		/// <summary>
		/// Gets the current lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		float ILightingProcessorDevice.GetLoadLevel(int room, int load)
		{
			return GetRoomContainersForRoom(room).First(a => a.ContainsZoneIntegration(load))
			                                       .GetZoneIntegration(load)
			                                       .OutputLevel;
		}

		/// <summary>
		/// Starts raising the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		void ILightingProcessorDevice.StartRaisingLoadLevel(int room, int load)
		{
			GetRoomContainersForRoom(room).First(a => a.ContainsZoneIntegration(load))
			                                .GetZoneIntegration(load)
			                                .StartRaising();
		}

		/// <summary>
		/// Starts lowering the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		void ILightingProcessorDevice.StartLoweringLoadLevel(int room, int load)
		{
			GetRoomContainersForRoom(room).First(a => a.ContainsZoneIntegration(load))
			                                .GetZoneIntegration(load)
			                                .StartLowering();
		}

		/// <summary>
		/// Stops raising/lowering the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		void ILightingProcessorDevice.StopRampingLoadLevel(int room, int load)
		{
			GetRoomContainersForRoom(room).First(a => a.ContainsZoneIntegration(load))
			                                .GetZoneIntegration(load)
			                                .StopRaisingLowering();
		}

		/// <summary>
		/// Starts raising the shade.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shade"></param>
		void ILightingProcessorDevice.StartRaisingShade(int room, int shade)
		{
			GetRoomContainersForRoom(room)
				.Where(a => a.ContainsShadeIntegration(shade))
				.Select(a => a.GetShadeIntegration(shade))
				.ForEach(s => s.StartRaising());
		}

		/// <summary>
		/// Starts lowering the shade.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shade"></param>
		void ILightingProcessorDevice.StartLoweringShade(int room, int shade)
		{
			GetRoomContainersForRoom(room)
				.Where(a => a.ContainsShadeIntegration(shade))
				.Select(a => a.GetShadeIntegration(shade))
				.ForEach(s => s.StartLowering());
		}

		/// <summary>
		/// Stops moving the shade.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shade"></param>
		void ILightingProcessorDevice.StopMovingShade(int room, int shade)
		{
			GetRoomContainersForRoom(room)
				.Where(a => a.ContainsShadeIntegration(shade))
				.Select(a => a.GetShadeIntegration(shade))
				.ForEach(s => s.StopMoving());
		}

		/// <summary>
		/// Starts raising the shade group.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shadeGroup"></param>
		void ILightingProcessorDevice.StartRaisingShadeGroup(int room, int shadeGroup)
		{
			GetRoomContainersForRoom(room)
				.Where(a => a.ContainsShadeGroupIntegration(shadeGroup))
				.Select(a => a.GetShadeGroupIntegration(shadeGroup))
				.ForEach(s => s.StartRaising());
		}

		/// <summary>
		/// Starts lowering the shade group.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shadeGroup"></param>
		void ILightingProcessorDevice.StartLoweringShadeGroup(int room, int shadeGroup)
		{
			GetRoomContainersForRoom(room)
				.Where(a => a.ContainsShadeGroupIntegration(shadeGroup))
				.Select(a => a.GetShadeGroupIntegration(shadeGroup))
				.ForEach(s => s.StartLowering());
		}

		/// <summary>
		/// Stops moving the shade group.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shadeGroup"></param>
		void ILightingProcessorDevice.StopMovingShadeGroup(int room, int shadeGroup)
		{
			GetRoomContainersForRoom(room)
				.Where(a => a.ContainsShadeGroupIntegration(shadeGroup))
				.Select(a => a.GetShadeGroupIntegration(shadeGroup))
				.ForEach(s => s.StopMoving());
		}

		#endregion
	}
}
