using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Xml;
using ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice;
using ICD.Connect.Lighting.Lutron.Nwk.EventArguments;
using ICD.Connect.Lighting.Lutron.Nwk.Integrations;

namespace ICD.Connect.Lighting.Lutron.Nwk.Devices.LutronNwk
{
	internal sealed class LutronNwkRoom : ILutronRoomContainer
	{
		// Maintain integration order
		public event EventHandler<OccupancyStateEventArgs> OnOccupancyStateChanged;
		public event EventHandler<GenericEventArgs<int?>> OnSceneChange;
		public event EventHandler<ZoneOutputLevelEventArgs> OnZoneOutputLevelChanged;
		private readonly List<int> m_Zones;
		private readonly List<int> m_ShadeGroups;
		private readonly List<int> m_Shades;
		private readonly List<int> m_Scenes;

		private readonly Dictionary<int, ZoneIntegration> m_IdToZone;
		private readonly Dictionary<int, ShadeGroupIntegration> m_IdToShadeGroup;
		private readonly Dictionary<int, ShadeIntegration> m_IdToShade;
		private readonly Dictionary<int, SceneIntegration> m_IdToScene;

		private readonly int m_Room;
		private readonly string m_Name;
		private KeypadDeviceIntegration m_Keypad;

		private int? m_Scene;


		public int Room { get { return m_Room; } }
		public string Name { get { return m_Name; } }

		public KeypadDeviceIntegration Keypad
		{
			get { return m_Keypad; }
			private set
			{
				if (value == m_Keypad)
					return;

				Unsubscribe(m_Keypad);
				m_Keypad = value;
				Subscribe(value);
			}
		}

		public LutronNwkRoom(int room, string name)
		{
			m_Zones = new List<int>();
			m_ShadeGroups = new List<int>();
			m_Shades = new List<int>();
			m_Scenes = new List<int>();
			m_IdToZone = new Dictionary<int, ZoneIntegration>();
			m_IdToShadeGroup = new Dictionary<int, ShadeGroupIntegration>();
			m_IdToShade = new Dictionary<int, ShadeIntegration>();
			m_IdToScene = new Dictionary<int, SceneIntegration>();

			m_Room = room;
			m_Name = name;
		}

		public static LutronNwkRoom FromXml(string xml, LutronNwkDevice parent)
		{
			int roomId = XmlUtils.GetAttributeAsInt(xml, "roomId");
			string name = XmlUtils.GetAttributeAsString(xml, "name");

			int keypadId = XmlUtils.ReadChildElementContentAsInt(xml, "KeypadIntegrationId");
			string keypadName = string.Format("{0} Keypad", name);

			string zonesXml;
			string shadeGroupsXml;
			string shadesXml;
			string scenesXml;

			XmlUtils.TryGetChildElementAsString(xml, "Zones", out zonesXml);
			XmlUtils.TryGetChildElementAsString(xml, "ShadeGroups", out shadeGroupsXml);
			XmlUtils.TryGetChildElementAsString(xml, "Shades", out shadesXml);
			XmlUtils.TryGetChildElementAsString(xml, "Scenes", out scenesXml);

			LutronNwkRoom output = new LutronNwkRoom(roomId, name);

			if (!string.IsNullOrEmpty(zonesXml))
				output.ParseZones(zonesXml, parent);

			if (!string.IsNullOrEmpty(shadeGroupsXml))
				output.ParseShadeGroups(shadeGroupsXml, parent);

			if (!string.IsNullOrEmpty(shadesXml))
				output.ParseShades(shadesXml, parent);

			if (!string.IsNullOrEmpty(scenesXml))
				output.ParseScenes(scenesXml, parent);

			KeypadDeviceIntegration keypad = new KeypadDeviceIntegration(keypadId, keypadName, parent, output.GetSceneIntegrations());

			output.Keypad = keypad;

			return output;
		}

		/// <summary>
		/// Parses the Zones xml element.
		/// </summary>
		/// <param name="zonesXml"></param>
		/// <param name="parent"></param>
		private void ParseZones(string zonesXml, ILutronNwkDevice parent)
		{
			ClearZones();

			IEnumerable<ZoneIntegration> items = XmlUtils.GetChildElementsAsString(zonesXml)
			                                             .Select(x => ZoneIntegration.FromXml(x, parent));

			foreach (ZoneIntegration zone in items)
			{
				if (m_IdToZone.ContainsKey(zone.IntegrationId))
				{
					IcdErrorLog.Warn("{0} {1} already contains zone {2}, skipping", GetType().Name, Room, zone.IntegrationId);
					continue;
				}

				m_Zones.Add(zone.IntegrationId);
				m_IdToZone[zone.IntegrationId] = zone;
				Subscribe(zone);
			}
		}

		/// <summary>
		/// Parses the ShadeGroups xml element.
		/// </summary>
		/// <param name="shadeGroupsXml"></param>
		/// <param name="parent"></param>
		private void ParseShadeGroups(string shadeGroupsXml, ILutronNwkDevice parent)
		{
			ClearShadeGroups();

			IEnumerable<ShadeGroupIntegration> items = XmlUtils.GetChildElementsAsString(shadeGroupsXml)
			                                                   .Select(x => ShadeGroupIntegration.FromXml(x, parent));

			foreach (ShadeGroupIntegration shadeGroup in items)
			{
				if (m_IdToShadeGroup.ContainsKey(shadeGroup.IntegrationId))
				{
					IcdErrorLog.Warn("{0} {1} already contains shade group {2}, skipping", GetType().Name, Room,
					                 shadeGroup.IntegrationId);
					continue;
				}

				m_ShadeGroups.Add(shadeGroup.IntegrationId);
				m_IdToShadeGroup[shadeGroup.IntegrationId] = shadeGroup;
			}
		}

		/// <summary>
		/// Parses the Shades xml element.
		/// </summary>
		/// <param name="shadesXml"></param>
		/// <param name="parent"></param>
		private void ParseShades(string shadesXml, ILutronNwkDevice parent)
		{
			ClearShades();

			IEnumerable<ShadeIntegration> items = XmlUtils.GetChildElementsAsString(shadesXml)
			                                              .Select(x => ShadeIntegration.FromXml(x, parent));

			foreach (ShadeIntegration shade in items)
			{
				if (m_IdToShade.ContainsKey(shade.IntegrationId))
				{
					IcdErrorLog.Warn("{0} {1} already contains shade {2}, skipping", GetType().Name, Room,
					                 shade.IntegrationId);
					continue;
				}

				m_Shades.Add(shade.IntegrationId);
				m_IdToShade[shade.IntegrationId] = shade;
			}
		}

		/// <summary>
		/// Parses the Scenes xml element.
		/// </summary>
		/// <param name="scenesXml"></param>
		/// <param name="parent"></param>
		private void ParseScenes(string scenesXml, ILutronNwkDevice parent)
		{
			ClearScenes();

			IEnumerable<SceneIntegration> items = XmlUtils.GetChildElementsAsString(scenesXml)
			                                              .Select(x => SceneIntegration.FromXml(x, parent));

			foreach (SceneIntegration scene in items)
			{
				if (m_IdToScene.ContainsKey(scene.IntegrationId))
				{
					IcdErrorLog.Warn("{0} {1} already contains scene {2}, skipping", GetType().Name, Room,
					                 scene.IntegrationId);
					continue;
				}

				m_Scenes.Add(scene.IntegrationId);
				m_IdToScene[scene.IntegrationId] = scene;
			}
		}


		/// <summary>
		/// Removes and disposes all of the child integrations.
		/// </summary>
		private void ClearChildren()
		{
			Keypad = null;

			ClearZones();
			ClearShades();
			ClearShadeGroups();
			ClearScenes();
		}

		/// <summary>
		/// Removes and disposes all of the child zone integrations.
		/// </summary>
		private void ClearZones()
		{
			foreach (ZoneIntegration zone in m_IdToZone.Values)
			{
				Unsubscribe(zone);
				zone.Dispose();
			}

			m_Zones.Clear();
			m_IdToZone.Clear();
		}

		/// <summary>
		/// Removes and disposes all of the child shade integrations.
		/// </summary>
		private void ClearShades()
		{
			foreach (ShadeIntegration shade in m_IdToShade.Values)
				shade.Dispose();

			m_Shades.Clear();
			m_IdToShade.Clear();
		}

		/// <summary>
		/// Removes and disposes all of the child shade group integrations.
		/// </summary>
		private void ClearShadeGroups()
		{
			foreach (ShadeGroupIntegration shadeGroup in m_IdToShadeGroup.Values)
				shadeGroup.Dispose();

			m_ShadeGroups.Clear();
			m_IdToShadeGroup.Clear();
		}

		/// <summary>
		/// Removes and disposes all of the child scene integrations.
		/// </summary>
		private void ClearScenes()
		{
			foreach (SceneIntegration scene in m_IdToScene.Values)
				scene.Dispose();

			m_Scenes.Clear();
			m_IdToScene.Clear();
		}

		#region ILutronRoomContainer Implementation

		public int? Scene
		{
			get { return m_Scene; }
			private set
			{
				if (value == m_Scene)
					return;

				m_Scene = value;

				OnSceneChange.Raise(this, new GenericEventArgs<int?>(value));
			}
		}

		public eOccupancyState OccupancyState { get { return eOccupancyState.Unknown; } }

		/// <summary>
		/// Gets the zone with the given integration id.
		/// </summary>
		/// <param name="integrationId"></param>
		/// <returns></returns>
		[PublicAPI]
		public ZoneIntegration GetZoneIntegration(int integrationId)
		{
			return m_IdToZone[integrationId];
		}

		/// <summary>
		/// Gets the zones.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public IEnumerable<ZoneIntegration> GetZoneIntegrations()
		{
			return m_Zones.Select(i => m_IdToZone[i]).ToArray();
		}

		/// <summary>
		/// Gets the shade group with the given integration id.
		/// </summary>
		/// <param name="integrationId"></param>
		/// <returns></returns>
		[PublicAPI]
		public ShadeGroupIntegration GetShadeGroupIntegration(int integrationId)
		{
			return m_IdToShadeGroup[integrationId];
		}

		/// <summary>
		/// Gets the shade groups.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public IEnumerable<ShadeGroupIntegration> GetShadeGroupIntegrations()
		{
			return m_ShadeGroups.Select(i => m_IdToShadeGroup[i]).ToArray();
		}

		/// <summary>
		/// Gets the shade with the given integration id.
		/// </summary>
		/// <param name="integrationId"></param>
		/// <returns></returns>
		[PublicAPI]
		public ShadeIntegration GetShadeIntegration(int integrationId)
		{
			return m_IdToShade[integrationId];
		}

		/// <summary>
		/// Gets the shades.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public IEnumerable<ShadeIntegration> GetShadeIntegrations()
		{
			return m_Shades.Select(i => m_IdToShade[i]).ToArray();
		}

		/// <summary>
		/// Gets the scene with the given integration id.
		/// </summary>
		/// <param name="integrationId"></param>
		/// <returns></returns>
		[PublicAPI]
		public SceneIntegration GetSceneIntegration(int integrationId)
		{
			return m_IdToScene[integrationId];
		}

		/// <summary>
		/// Gets the scenes.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public IEnumerable<SceneIntegration> GetSceneIntegrations()
		{
			return m_Scenes.Select(i => m_IdToScene[i]).ToArray();
		}

		/// <summary>
		/// Sets the current scene for the area.
		/// </summary>
		/// <param name="scene"></param>
		[PublicAPI]
		public void SetScene(int? scene)
		{
			if (!scene.HasValue)
				return;

			// Scene id corresponds to the button number on the keypad
			KeypadDeviceIntegration keypad = Keypad;
			if (keypad != null)
				keypad.ButtonPressRelease(scene.Value);
		}

		/// <summary>
		/// Returns true if the area contains the given zone.
		/// </summary>
		/// <param name="zone"></param>
		/// <returns></returns>
		public bool ContainsZoneIntegration(int zone)
		{
			return m_IdToZone.ContainsKey(zone);
		}

		/// <summary>
		/// Returns true if the area contains the given shade.
		/// </summary>
		/// <param name="shade"></param>
		/// <returns></returns>
		public bool ContainsShadeIntegration(int shade)
		{
			return m_IdToShade.ContainsKey(shade);
		}

		/// <summary>
		/// Returns true if the area contains the given shade group.
		/// </summary>
		/// <param name="shadeGroup"></param>
		/// <returns></returns>
		public bool ContainsShadeGroupIntegration(int shadeGroup)
		{
			return m_IdToShadeGroup.ContainsKey(shadeGroup);
		}

		#endregion

		#region Zone Callbacks

		private void Subscribe(ZoneIntegration zone)
		{
			if (zone == null)
				return;

			zone.OnOutputLevelChanged += ZoneOnOutputLevelChanged;
		}

		private void Unsubscribe(ZoneIntegration zone)
		{
			if (zone == null)
				return;

			zone.OnOutputLevelChanged -= ZoneOnOutputLevelChanged;
		}

		/// <summary>
		/// Called when a zone output level changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void ZoneOnOutputLevelChanged(object sender, FloatEventArgs args)
		{
			// When the zone level changes the scene becomes null, but the device doesn't give feedback for this
			var keypad = Keypad;
			if (keypad != null)
				keypad.QueryActiveScene();

			ZoneIntegration zone = sender as ZoneIntegration;
			if (zone != null)
				OnZoneOutputLevelChanged.Raise(this, new ZoneOutputLevelEventArgs(zone.IntegrationId, args.Data));
		}

		#endregion

		#region Keypad Callbacks

		private void Subscribe(KeypadDeviceIntegration keypad)
		{
			if (keypad == null)
				return;

			keypad.OnActiveSceneChanged += KeypadOnActiveSceneChanged;

			Scene = keypad.ActiveScene;
		}

		private void Unsubscribe(KeypadDeviceIntegration keypad)
		{
			if (keypad == null)
				return;

			keypad.OnActiveSceneChanged -= KeypadOnActiveSceneChanged;

			Scene = null;
		}

		private void KeypadOnActiveSceneChanged(object sender, GenericEventArgs<int?> args)
		{
			Scene = args.Data;
		}

		#endregion

		/// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
		public void Dispose()
		{
			OnOccupancyStateChanged = null;
			OnSceneChange = null;
			OnZoneOutputLevelChanged = null;

			ClearChildren();
		}
	}
}
