﻿using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Lighting.EventArguments;
using ICD.Connect.Lighting.Processors;
using ICD.Connect.Partitioning.Commercial.Controls.Occupancy;
using ICD.Connect.Settings;

namespace ICD.Connect.Lighting.RoomInterface
{
	public sealed class LightingRoomInterfaceDevice : AbstractLightingRoomInterfaceDevice<LightingRoomInterfaceDeviceSettings>
	{
		[CanBeNull]
		private ILightingProcessorDevice m_LightingProcessor;

		private int m_RoomId;

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return m_LightingProcessor != null && m_LightingProcessor.IsOnline;
		}

		#region Settings

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(LightingRoomInterfaceDeviceSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			m_RoomId = settings.LightingRoomId;

			if (settings.LightingProcessorDeviceId != null)
			{
				try
				{
					m_LightingProcessor =
						factory.GetOriginatorById<ILightingProcessorDevice>((int)settings.LightingProcessorDeviceId);
				}
				catch (KeyNotFoundException)
				{
					Logger.Log(eSeverity.Error, "No lighting processor device with id {0}", settings.LightingProcessorDeviceId);
				}
			}

			Subscribe(m_LightingProcessor);
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			m_RoomId = 0;

			Unsubscribe(m_LightingProcessor);

			m_LightingProcessor = null;
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(LightingRoomInterfaceDeviceSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.LightingRoomId = m_RoomId;
			settings.LightingProcessorDeviceId = m_LightingProcessor == null ? (int?)null : m_LightingProcessor.Id;
		}

		#endregion

		#region ILightingProcessorDevice Callbacks

		private void Subscribe(ILightingProcessorDevice lightingProcessor)
		{
			if (lightingProcessor == null)
				return;

			lightingProcessor.OnIsOnlineStateChanged += LightingProcessorOnIsOnlineStateChanged;
			lightingProcessor.OnRoomOccupancyChanged += LightingProcessorOnRoomOccupancyChanged;
			lightingProcessor.OnRoomPresetChanged += LightingProcessorOnRoomPresetChanged;
			lightingProcessor.OnRoomLoadLevelChanged += LightingProcessorOnRoomLoadLevelChanged;
			lightingProcessor.OnRoomControlsChanged += LightingProcessorOnRoomControlsChanged;
		}

		private void Unsubscribe(ILightingProcessorDevice lightingProcessor)
		{
			if (lightingProcessor == null)
				return;

			lightingProcessor.OnIsOnlineStateChanged -= LightingProcessorOnIsOnlineStateChanged;
			lightingProcessor.OnRoomOccupancyChanged -= LightingProcessorOnRoomOccupancyChanged;
			lightingProcessor.OnRoomPresetChanged -= LightingProcessorOnRoomPresetChanged;
			lightingProcessor.OnRoomLoadLevelChanged -= LightingProcessorOnRoomLoadLevelChanged;
			lightingProcessor.OnRoomControlsChanged -= LightingProcessorOnRoomControlsChanged;
		}

		private void LightingProcessorOnIsOnlineStateChanged(object sender, Devices.EventArguments.DeviceBaseOnlineStateApiEventArgs e)
		{
			UpdateCachedOnlineStatus();
		}

		private void LightingProcessorOnRoomOccupancyChanged(object sender, RoomOccupancyEventArgs args)
		{
			if (args.RoomId == m_RoomId)
				OnOccupancyChanged.Raise(this, new GenericEventArgs<eOccupancyState>(args.OccupancyState));
		}

		private void LightingProcessorOnRoomPresetChanged(object sender, RoomPresetChangeEventArgs args)
		{
			if (args.RoomId == m_RoomId)
				OnPresetChanged.Raise(this, new GenericEventArgs<int?>(args.Preset));
		}

		private void LightingProcessorOnRoomLoadLevelChanged(object sender, RoomLoadLevelEventArgs args)
		{
			if (args.RoomId == m_RoomId)
				OnLoadLevelChanged.Raise(this, new LoadLevelEventArgs(args.LoadId, args.Percentage));
		}

		private void LightingProcessorOnRoomControlsChanged(object sender, IntEventArgs args)
		{
			if (args.Data == m_RoomId)
				OnControlsChanged.Raise(this);
		}

		#endregion

		#region ILightingRoomInterfaceDevice Implementation

		public override event EventHandler<GenericEventArgs<eOccupancyState>> OnOccupancyChanged;
		public override event EventHandler<GenericEventArgs<int?>> OnPresetChanged;
		public override event EventHandler<LoadLevelEventArgs> OnLoadLevelChanged;
		public override event EventHandler OnControlsChanged;

		/// <summary>
		/// Gets the available light loads for the room.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<LightingProcessorControl> GetLoads()
		{
			return m_LightingProcessor == null
				? Enumerable.Empty<LightingProcessorControl>()
				: m_LightingProcessor.GetLoadsForRoom(m_RoomId);
		}

		/// <summary>
		/// Gets the available individual shades for the room.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<LightingProcessorControl> GetShades()
		{
			return m_LightingProcessor == null
				? Enumerable.Empty<LightingProcessorControl>()
				: m_LightingProcessor.GetShadesForRoom(m_RoomId);
		}

		/// <summary>
		/// Gets the available shade groups for the room.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<LightingProcessorControl> GetShadeGroups()
		{
			return m_LightingProcessor == null
				? Enumerable.Empty<LightingProcessorControl>()
				: m_LightingProcessor.GetShadeGroupsForRoom(m_RoomId);
		}

		/// <summary>
		/// Gets the available presets for the room.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<LightingProcessorControl> GetPresets()
		{
			return m_LightingProcessor == null
				? Enumerable.Empty<LightingProcessorControl>()
				: m_LightingProcessor.GetPresetsForRoom(m_RoomId);
		}

		/// <summary>
		/// Gets the current occupancy state for the given room.
		/// </summary>
		/// <returns></returns>
		public override eOccupancyState GetOccupancy()
		{
			return m_LightingProcessor == null
				? eOccupancyState.Unknown
				: m_LightingProcessor.GetOccupancyForRoom(m_RoomId);
		}

		/// <summary>
		/// Sets the preset for the given room.
		/// </summary>
		/// <param name="preset"></param>
		public override void SetPreset(int? preset)
		{
			if (m_LightingProcessor != null)
				m_LightingProcessor.SetPresetForRoom(m_RoomId, preset);
		}

		/// <summary>
		/// Gets the current preset for the given room.
		/// </summary>
		public override int? GetPreset()
		{
			return m_LightingProcessor == null
				? null
				: m_LightingProcessor.GetPresetForRoom(m_RoomId);
		}

		/// <summary>
		/// Sets the lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		/// <param name="percentage"></param>
		public override void SetLoadLevel(int load, float percentage)
		{
			if (m_LightingProcessor != null)
				m_LightingProcessor.SetLoadLevel(m_RoomId, load, percentage);
		}

		/// <summary>
		/// Gets the current lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		public override float GetLoadLevel(int load)
		{
			return m_LightingProcessor == null ? 0 : m_LightingProcessor.GetLoadLevel(m_RoomId, load);
		}

		/// <summary>
		/// Starts raising the lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		public override void StartRaisingLoadLevel(int load)
		{
			if (m_LightingProcessor != null)
				m_LightingProcessor.StartRaisingLoadLevel(m_RoomId, load);
		}

		/// <summary>
		/// Starts lowering the lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		public override void StartLoweringLoadLevel(int load)
		{
			if (m_LightingProcessor != null)
				m_LightingProcessor.StartLoweringLoadLevel(m_RoomId, load);
		}

		/// <summary>
		/// Stops raising/lowering the lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		public override void StopRampingLoadLevel(int load)
		{
			if (m_LightingProcessor != null)
				m_LightingProcessor.StopRampingLoadLevel(m_RoomId, load);
		}

		/// <summary>
		/// Starts raising the shade.
		/// </summary>
		/// <param name="shade"></param>
		public override void StartRaisingShade(int shade)
		{
			if (m_LightingProcessor != null)
				m_LightingProcessor.StartRaisingShade(m_RoomId, shade);
		}

		/// <summary>
		/// Starts lowering the shade.
		/// </summary>
		/// <param name="shade"></param>
		public override void StartLoweringShade(int shade)
		{
			if (m_LightingProcessor != null)
				m_LightingProcessor.StartLoweringShade(m_RoomId, shade);
		}

		/// <summary>
		/// Stops moving the shade.
		/// </summary>
		/// <param name="shade"></param>
		public override void StopMovingShade(int shade)
		{
			if (m_LightingProcessor != null)
				m_LightingProcessor.StopMovingShade(m_RoomId, shade);
		}

		/// <summary>
		/// Starts raising the shade group.
		/// </summary>
		/// <param name="shadeGroup"></param>
		public override void StartRaisingShadeGroup(int shadeGroup)
		{
			if (m_LightingProcessor != null)
				m_LightingProcessor.StartRaisingShadeGroup(m_RoomId, shadeGroup);
		}

		/// <summary>
		/// Starts lowering the shade group.
		/// </summary>
		/// <param name="shadeGroup"></param>
		public override void StartLoweringShadeGroup(int shadeGroup)
		{
			if (m_LightingProcessor != null)
				m_LightingProcessor.StartLoweringShadeGroup(m_RoomId, shadeGroup);
		}

		/// <summary>
		/// Stops moving the shade group.
		/// </summary>
		/// <param name="shadeGroup"></param>
		public override void StopMovingShadeGroup(int shadeGroup)
		{
			if (m_LightingProcessor != null)
				m_LightingProcessor.StopMovingShadeGroup(m_RoomId, shadeGroup);
		}

		#endregion

	}
}
