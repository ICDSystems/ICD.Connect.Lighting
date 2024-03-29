﻿using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Lighting.Lutron.Nwk.EventArguments;
using ICD.Connect.Lighting.Lutron.Nwk.Integrations;
using ICD.Connect.Lighting.Lutron.Nwk.Integrations.Interfaces;

namespace ICD.Connect.Lighting.Lutron.Nwk
{
	public interface ILutronRoomContainer : IDisposable
	{
		/// <summary>
		/// Raised when the occupancy state for the area changes.
		/// </summary>
		event EventHandler<OccupancyStateEventArgs> OnOccupancyStateChanged;

		/// <summary>
		/// Raised when the scene for the area changes.
		/// </summary>
		event EventHandler<GenericEventArgs<int?>> OnSceneChange;

		/// <summary>
		/// Raised when the output level for a zone changes.
		/// </summary>
		event EventHandler<ZoneOutputLevelEventArgs> OnZoneOutputLevelChanged;

		/// <summary>
		/// Room ID
		/// </summary>
		int Room { get; }

		int? Scene { get; }

		eOccupancyState OccupancyState { get; }

		/// <summary>
		/// Gets the zone with the given integration id.
		/// </summary>
		/// <param name="integrationId"></param>
		/// <returns></returns>
		[PublicAPI]
		IZoneIntegration GetZoneIntegration(int integrationId);

		/// <summary>
		/// Gets the zones.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		IEnumerable<IZoneIntegration> GetZoneIntegrations();

		/// <summary>
		/// Gets the shade group with the given integration id.
		/// </summary>
		/// <param name="integrationId"></param>
		/// <returns></returns>
		[PublicAPI]
		IShadeIntegration GetShadeGroupIntegration(int integrationId);

		/// <summary>
		/// Gets the shade groups.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		IEnumerable<IShadeIntegration> GetShadeGroupIntegrations();

		/// <summary>
		/// Gets the shade with the given integration id.
		/// </summary>
		/// <param name="integrationId"></param>
		/// <returns></returns>
		[PublicAPI]
		IShadeIntegration GetShadeIntegration(int integrationId);

		/// <summary>
		/// Gets the shades.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		IEnumerable<IShadeIntegration> GetShadeIntegrations();

		/// <summary>
		/// Gets the scene with the given integration id.
		/// </summary>
		/// <param name="integrationId"></param>
		/// <returns></returns>
		[PublicAPI]
		SceneIntegration GetSceneIntegration(int integrationId);

		/// <summary>
		/// Gets the scenes.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		IEnumerable<SceneIntegration> GetSceneIntegrations();

		/// <summary>
		/// Sets the current scene for the area.
		/// </summary>
		/// <param name="scene"></param>
		[PublicAPI]
		void SetScene(int? scene);

		/// <summary>
		/// Returns true if the area contains the given zone.
		/// </summary>
		/// <param name="zone"></param>
		/// <returns></returns>
		bool ContainsZoneIntegration(int zone);

		/// <summary>
		/// Returns true if the area contains the given shade.
		/// </summary>
		/// <param name="shade"></param>
		/// <returns></returns>
		bool ContainsShadeIntegration(int shade);

		/// <summary>
		/// Returns true if the area contains the given shade group.
		/// </summary>
		/// <param name="shadeGroup"></param>
		/// <returns></returns>
		bool ContainsShadeGroupIntegration(int shadeGroup);
	}
}
