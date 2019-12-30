using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Lighting.Lutron.Nwk.EventArguments;

namespace ICD.Connect.Lighting.Lutron.Nwk.Devices.QuantumNwk
{
	public sealed partial class LutronQuantumNwkDevice
	{
		#region ILightingProcessorDevice Implementation

		/// <summary>
		/// Gets the available light loads for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public override IEnumerable<LightingProcessorControl> GetLoadsForRoom(int room)
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
		public override IEnumerable<LightingProcessorControl> GetShadesForRoom(int room)
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
		public override IEnumerable<LightingProcessorControl> GetShadeGroupsForRoom(int room)
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
		public override IEnumerable<LightingProcessorControl> GetPresetsForRoom(int room)
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
		public override Misc.Occupancy.eOccupancyState GetOccupancyForRoom(int room)
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
		public override void SetPresetForRoom(int room, int? preset)
		{
			GetAreaIntegrationsForRoom(room).ForEach(a => a.SetScene(preset));
		}

		/// <summary>
		/// Gets the current preset for the given room.
		/// Returns 0 if multuple areas for the given room, and the areas return different results.
		/// </summary>
		/// <param name="room"></param>
		public override int? GetPresetForRoom(int room)
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
		public override void SetLoadLevel(int room, int load, float percentage)
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
		public override float GetLoadLevel(int room, int load)
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
		public override void StartRaisingLoadLevel(int room, int load)
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
		public override void StartLoweringLoadLevel(int room, int load)
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
		public override void StopRampingLoadLevel(int room, int load)
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
		public override void StartRaisingShade(int room, int shade)
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
		public override void StartLoweringShade(int room, int shade)
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
		public override void StopMovingShade(int room, int shade)
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
		public override void StartRaisingShadeGroup(int room, int shadeGroup)
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
		public override void StartLoweringShadeGroup(int room, int shadeGroup)
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
		public override void StopMovingShadeGroup(int room, int shadeGroup)
		{
			GetAreaIntegrationsForRoom(room)
				.Where(a => a.ContainsShadeGroupIntegration(shadeGroup))
				.Select(a => a.GetShadeGroupIntegration(shadeGroup))
				.ForEach(s => s.StopMoving());
		}

		#endregion
	}
}
