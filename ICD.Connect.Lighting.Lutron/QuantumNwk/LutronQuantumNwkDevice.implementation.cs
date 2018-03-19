using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Lighting.EventArguments;
using ICD.Connect.Lighting.Lutron.QuantumNwk.EventArguments;
using ICD.Connect.Lighting.Processors;

namespace ICD.Connect.Lighting.Lutron.QuantumNwk
{
	public sealed partial class LutronQuantumNwkDevice : ILightingProcessorDevice
	{
		#region ILightingProcessorDevice Implementation

		/// <summary>
		/// Gets the available light loads for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		IEnumerable<LightingProcessorControl> ILightingProcessorDevice.GetLoadsForRoom(int room)
		{
			return GetAreaIntegrationsForRoom(room)
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
			return GetAreaIntegrationsForRoom(room)
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
			return GetAreaIntegrationsForRoom(room)
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
			return GetAreaIntegrationsForRoom(room)
				.SelectMany(a => a.GetSceneIntegrations())
				.Select(i => LightingProcessorControl.Preset(i.IntegrationId, room, i.Name));
		}

		/// <summary>
		/// Gets the current occupancy state for the given room.
		/// Returns unknown if the room has multiple areas that have different occupancies.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		RoomOccupancyEventArgs.eOccupancyState ILightingProcessorDevice.GetOccupancyForRoom(int room)
		{
			eOccupancyState state = GetAreaIntegrationsForRoom(room).Select(a => a.OccupancyState)
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
			GetAreaIntegrationsForRoom(room).ForEach(a => a.SetScene(preset));
		}

		/// <summary>
		/// Gets the current preset for the given room.
		/// Returns 0 if multuple areas for the given room, and the areas return different results.
		/// </summary>
		/// <param name="room"></param>
		int? ILightingProcessorDevice.GetPresetForRoom(int room)
		{
			return GetAreaIntegrationsForRoom(room).Select(a => a.Scene)
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
			GetAreaIntegrationsForRoom(room).Where(a => a.ContainsZoneIntegration(load))
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
			return GetAreaIntegrationsForRoom(room).First(a => a.ContainsZoneIntegration(load))
			                                       .GetZoneIntegration(load)
			                                       .OutputLevel;
		}

		/// <summary>
		/// Starts raising the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		public void StartRaisingLoadLevel(int room, int load)
		{
			GetAreaIntegrationsForRoom(room).First(a => a.ContainsZoneIntegration(load))
			                                .GetZoneIntegration(load)
			                                .StartRaising();
		}

		/// <summary>
		/// Starts lowering the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		public void StartLoweringLoadLevel(int room, int load)
		{
			GetAreaIntegrationsForRoom(room).First(a => a.ContainsZoneIntegration(load))
			                                .GetZoneIntegration(load)
			                                .StartLowering();
		}

		/// <summary>
		/// Stops raising/lowering the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		public void StopRampingLoadLevel(int room, int load)
		{
			GetAreaIntegrationsForRoom(room).First(a => a.ContainsZoneIntegration(load))
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
			GetAreaIntegrationsForRoom(room)
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
			GetAreaIntegrationsForRoom(room)
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
			GetAreaIntegrationsForRoom(room)
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
			GetAreaIntegrationsForRoom(room)
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
			GetAreaIntegrationsForRoom(room)
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
			GetAreaIntegrationsForRoom(room)
				.Where(a => a.ContainsShadeGroupIntegration(shadeGroup))
				.Select(a => a.GetShadeGroupIntegration(shadeGroup))
				.ForEach(s => s.StopMoving());
		}

		/// <summary>
		/// Converts a Lutron occupancy state to a lighting processor device occupancy state.
		/// </summary>
		/// <param name="occupancy"></param>
		/// <returns></returns>
		private static RoomOccupancyEventArgs.eOccupancyState GetOccupancyState(eOccupancyState occupancy)
		{
			switch (occupancy)
			{
				case eOccupancyState.Unknown:
					return RoomOccupancyEventArgs.eOccupancyState.Unknown;
				case eOccupancyState.Inactive:
					return RoomOccupancyEventArgs.eOccupancyState.Inactive;
				case eOccupancyState.Occupied:
					return RoomOccupancyEventArgs.eOccupancyState.Occupied;
				case eOccupancyState.Unoccupied:
					return RoomOccupancyEventArgs.eOccupancyState.Unoccupied;
				default:
					throw new ArgumentOutOfRangeException("occupancy", "Unexpected eOccupancyState " + occupancy);
			}
		}

		#endregion
	}
}
