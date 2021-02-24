using System;
using System.Collections.Generic;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice;
using ICD.Connect.Lighting.Lutron.Nwk.Integrations.Abstracts;

namespace ICD.Connect.Lighting.Lutron.Nwk.Integrations.GrafikEyeIntegrations
{
	/// <summary>
	/// Integration for a GrafikEye Device
	/// All scene/load/occupancy data goes through the device integration, not individual integrations
	/// </summary>
	public sealed class GrafikEyeDeviceIntegration : AbstractIntegrationWithComponent<string>
	{
		private const int COMPONENT_SCENE_CONTROLLER = 141;

		private const int ACTION_PRESS = 3;
		private const int ACTION_RELEASE = 4;
		private const int ACTION_CURRENT_SCENE = 7;
		private const int ACTION_ZONE_LEVEL = 14;
		private const int ACTION_START_RAISING = 18;
		private const int ACTION_START_LOWERING = 19;
		private const int ACTION_STOP_RAISING_LOWERING = 20;

		private const int ZONE_MIN_RANGE = 1;
		private const int ZONE_MAX_RANGE = 24;
		private const int SHADE_MIN_RANGE = 1;
		private const int SHADE_MAX_RANGE = 3;

		private readonly Dictionary<int, SceneIntegration> m_Scenes;
		private readonly Dictionary<int, GrafikEyeZoneIntegration> m_Zones;
		private readonly Dictionary<int, GrafikEyeShadeIntegration> m_Shades;

		private int? m_CurrentScene;

		public event EventHandler<GenericEventArgs<int?>> OnCurrentSceneChanged;

		public int? CurrentScene
		{
			get { return m_CurrentScene; }
			private set
			{
				if (m_CurrentScene == value)
					return;

				m_CurrentScene = value;

				OnCurrentSceneChanged.Raise(this, value);
			}
		}

		/// <summary>
		/// The string prefix for communication with the lighting processor, e.g. SHADES.
		/// </summary>
		protected override string Command { get { return LutronUtils.COMMAND_DEVICE; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="integrationId"></param>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		/// <param name="scenes"></param>
		/// <param name="zones"></param>
		public GrafikEyeDeviceIntegration(string integrationId, string name, ILutronNwkDevice parent,
		                                  IEnumerable<SceneIntegration> scenes, IEnumerable<GrafikEyeZoneIntegration> zones, IEnumerable<GrafikEyeShadeIntegration> shades) : base(integrationId, name, parent)
		{
			m_Scenes = new Dictionary<int, SceneIntegration>();
			m_Zones = new Dictionary<int, GrafikEyeZoneIntegration>();
			m_Shades = new Dictionary<int, GrafikEyeShadeIntegration>();

			SetScenes(scenes);
			SetZones(zones);
			SetShades(shades);
		}

		/// <summary>
		/// Override to query the device when it goes online.
		/// </summary>
		protected override void Initialize()
		{
			base.Initialize();

			QueryActiveScene();

			foreach(int zone in m_Zones.Keys)
				ZoneQueryLevel(zone);
		}

		#region Response Handlers

		/// <summary>
		/// Called when we receive a response from the lighting processor for this integration.
		/// </summary>
		/// <param name="action">The action number for the response.</param>
		/// <param name="component">The component number for the response</param>
		/// <param name="parameters">The collection of string parameters.</param>
		protected override void ParentOnResponse(int action, int component, string[] parameters)
		{
			base.ParentOnResponse(action, component, parameters);

			switch (action)
			{
				case (ACTION_CURRENT_SCENE):
					if (component == COMPONENT_SCENE_CONTROLLER)
					{
						if(parameters.Length < 1)
							throw new FormatException("Current scene response requires 1 parameter, but none were sent");
						HandleCurrentSceneFeedback(parameters[0]);
					}
					break;
				case (ACTION_ZONE_LEVEL):
					if (parameters.Length < 1)
						throw new FormatException("Zone level response requires 1 parameter, but none were sent");
					HandleZoneLevelFeedback(component, parameters[0]);
					break;
			}
			//todo: add handlers for callback options
		}

		/// <summary>
		/// Handle Grafik Eye's response with zone level feedback
		/// </summary>
		/// <param name="zoneId"></param>
		/// <param name="level"></param>
		private void HandleZoneLevelFeedback(int zoneId, string level)
		{
			HandleZoneLevelFeedback(zoneId, LutronUtils.PercentageParameterToFloat(level));
		}

		/// <summary>
		/// Handle Grafik Eye's response with zone level feedback
		/// </summary>
		/// <param name="zoneId"></param>
		/// <param name="percentage"></param>
		private void HandleZoneLevelFeedback(int zoneId, float percentage)
		{
			GrafikEyeZoneIntegration zone;
			if (!m_Zones.TryGetValue(zoneId, out zone))
				return;
			zone.OutputLevel = percentage;
		}

		/// <summary>
		/// Handles Grafik Eye's response to the current scene
		/// </summary>
		/// <param name="scene"></param>
		private void HandleCurrentSceneFeedback(string scene)
		{
			int sceneId;
			if (!StringUtils.TryParse(scene, out sceneId))
				throw new FormatException(string.Format("Scene ID couldn't be parsed to an int - received {0}",
				                                        scene));
			
			// Check to make sure there's an integration for the current scene
			if (!m_Scenes.ContainsKey(sceneId))
			{
				CurrentScene = null;
				return;
			}

			CurrentScene = sceneId;
		}

		#endregion

		#region Scenes

		/// <summary>
		/// Setup the scenes for this integration
		/// </summary>
		/// <param name="scenes"></param>
		private void SetScenes(IEnumerable<SceneIntegration> scenes)
		{
			m_Scenes.Clear();

			m_Scenes.AddRange(scenes, s => s.IntegrationId);

			QueryActiveScene();
		}

		/// <summary>
		/// Query the device for the active scene
		/// </summary>
		private void QueryActiveScene()
		{
			QueryComponent(COMPONENT_SCENE_CONTROLLER, ACTION_CURRENT_SCENE);

		}

		/// <summary>
		/// Set the scene on the device
		/// </summary>
		/// <param name="sceneId"></param>
		public void SetScene(int? sceneId)
		{
			if (!sceneId.HasValue)
				return;

			ExecuteComponent(COMPONENT_SCENE_CONTROLLER, ACTION_CURRENT_SCENE, sceneId.Value);
		}

		#endregion

		#region Zone Integration

		/// <summary>
		/// Setup the zones for this integration
		/// </summary>
		/// <param name="zones"></param>
		private void SetZones(IEnumerable<GrafikEyeZoneIntegration> zones)
		{
			m_Zones.Clear();

			m_Zones.AddRange(zones, z => z.IntegrationId);
		}

		/// <summary>
		/// Tell the device to set specified zone to specified level
		/// </summary>
		/// <param name="zoneId"></param>
		/// <param name="percentage"></param>
		internal void ZoneSetLevel(int zoneId, float percentage)
		{
			// Lutron defaults to 1 second fades, so we will too
			ZoneSetLevel(zoneId, percentage, TimeSpan.FromSeconds(1), new TimeSpan());
		}

		/// <summary>
		/// Tell the device to set specified zone to specified level with fade and delay times
		/// </summary>
		/// <param name="zoneId"></param>
		/// <param name="percentage"></param>
		/// <param name="fade"></param>
		/// <param name="delay"></param>
		internal void ZoneSetLevel(int zoneId, float percentage, TimeSpan fade, TimeSpan delay)
		{
			// zone id must be between 1 and 24
			if (zoneId < ZONE_MIN_RANGE || zoneId > ZONE_MAX_RANGE)
				throw new ArgumentOutOfRangeException("zoneId");

			string percentageParam = LutronUtils.FloatToPercentageParameter(percentage);
			string fadeParam = LutronUtils.TimeSpanToParameter(fade);
			string delayParam = LutronUtils.TimeSpanToParameter(delay);

			ExecuteComponent(zoneId, ACTION_ZONE_LEVEL, percentageParam, fadeParam, delayParam);
		}

		/// <summary>
		/// Query the device for the specified zone's level
		/// </summary>
		/// <param name="zoneId"></param>
		private void ZoneQueryLevel(int zoneId)
		{
			// zone id must be between 1 and 24
			if (zoneId < ZONE_MIN_RANGE || zoneId > ZONE_MAX_RANGE)
				throw new ArgumentOutOfRangeException("zoneId");

			QueryComponent(zoneId, ACTION_ZONE_LEVEL);
		}

		/// <summary>
		/// Start raising the specified zone
		/// </summary>
		/// <param name="zoneId"></param>
		internal void ZoneStartRaising(int zoneId)
		{
			// zone id must be between 1 and 24
			if (zoneId < ZONE_MIN_RANGE || zoneId > ZONE_MAX_RANGE)
				throw new ArgumentOutOfRangeException("zoneId");

			ExecuteComponent(zoneId, ACTION_START_RAISING);
		}

		/// <summary>
		/// Start lowering the specified zone
		/// </summary>
		/// <param name="zoneId"></param>
		internal void ZoneStartLowering(int zoneId)
		{
			// zone id must be between 1 and 24
			if (zoneId < ZONE_MIN_RANGE || zoneId > ZONE_MAX_RANGE)
				throw new ArgumentOutOfRangeException("zoneId");

			ExecuteComponent(zoneId, ACTION_START_LOWERING);
		}

		/// <summary>
		/// Stop raising or lowering the specified zone
		/// </summary>
		/// <param name="zoneId"></param>
		internal void ZoneStopRaisingLowering(int zoneId)
		{
			// zone id must be between 1 and 24
			if (zoneId < ZONE_MIN_RANGE || zoneId > ZONE_MAX_RANGE)
				throw new ArgumentOutOfRangeException("zoneId");

			ExecuteComponent(zoneId, ACTION_STOP_RAISING_LOWERING);
		}
		#endregion

		#region Shade Integration

		/// <summary>
		/// Setup the shades for this device
		/// </summary>
		/// <param name="shades"></param>
		private void SetShades(IEnumerable<GrafikEyeShadeIntegration> shades)
		{
			m_Shades.Clear();

			m_Shades.AddRange(shades, s => s.IntegrationId);
		}

		internal void ShadeStartRaising(int shadeId)
		{
			if (shadeId < SHADE_MIN_RANGE || shadeId > SHADE_MAX_RANGE)
				throw new ArgumentOutOfRangeException("shadeId");

			// Shades move on release
			ExecuteComponent(GetShadeRaiseComponentNumber(shadeId), ACTION_RELEASE);
		}

		internal void ShadeStartLowering(int shadeId)
		{
			if (shadeId < SHADE_MIN_RANGE || shadeId > SHADE_MAX_RANGE)
				throw new ArgumentOutOfRangeException("shadeId");

			// Shades move on release
			ExecuteComponent(GetShadeLowerComponentNumber(shadeId), ACTION_RELEASE);
		}

		private int GetShadeRaiseComponentNumber(int shadeId)
		{
			switch (shadeId)
			{
				case 1:
					return 47;
				case 2:
					return 53;
				case 3:
					return 58;
				default:
					throw new ArgumentOutOfRangeException("shadeId");
			}
		}

		private int GetShadeLowerComponentNumber(int shadeId)
		{
			switch (shadeId)
			{
				case 1:
					return 41;
				case 2:
					return 52;
				case 3:
					return 57;
				default:
					throw new ArgumentOutOfRangeException("shadeId");
			}
		}

		#endregion
	}
}
