using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Xml;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice;
using ICD.Connect.Lighting.Lutron.Nwk.EventArguments;
using ICD.Connect.Lighting.Lutron.Nwk.Integrations;
using ICD.Connect.Lighting.Lutron.Nwk.Integrations.GrafikEyeIntegrations;
using ICD.Connect.Lighting.Lutron.Nwk.Integrations.Interfaces;

namespace ICD.Connect.Lighting.Lutron.Nwk.Devices.GrafikEyeNwk
{
	public sealed class GrafikEyeRoom : ILutronRoomContainer, IConsoleNode
	{
		/// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
		public void Dispose()
		{
			OnOccupancyStateChanged = null;
			OnSceneChange = null;
			OnZoneOutputLevelChanged = null;

			ClearChildren();
		}

		private readonly int m_Room;
		private readonly string m_Name;

		private readonly List<int> m_Zones;
		private readonly List<int> m_Shades;
		private readonly List<int> m_Scenes;

		private readonly Dictionary<int, GrafikEyeZoneIntegration> m_IdToZone;
		private readonly Dictionary<int, GrafikEyeShadeIntegration> m_IdToShade;
		private readonly Dictionary<int, SceneIntegration> m_IdToScene;

		private GrafikEyeDeviceIntegration m_GrafikEye;
		private int? m_Scene;

		private eOccupancyState m_OccupancyState;

		/// <summary>
		/// Raised when the occupancy state for the area changes.
		/// </summary>
		public event EventHandler<OccupancyStateEventArgs> OnOccupancyStateChanged;

		/// <summary>
		/// Raised when the scene for the area changes.
		/// </summary>
		public event EventHandler<GenericEventArgs<int?>> OnSceneChange;

		/// <summary>
		/// Raised when the output level for a zone changes.
		/// </summary>
		public event EventHandler<ZoneOutputLevelEventArgs> OnZoneOutputLevelChanged;

		public GrafikEyeDeviceIntegration GrafikEye
		{
			get { return m_GrafikEye; }
			private set
			{
				if (m_GrafikEye == value)
					return;

				Unsubscribe(m_GrafikEye);
				m_GrafikEye = value;
				Subscribe(m_GrafikEye);

			}
		}

		/// <summary>
		/// Room ID
		/// </summary>
		public int Room { get { return m_Room; } }

		public int? Scene
		{
			get { return m_Scene; }
			private set
			{
				if (m_Scene == value)
					return;

				m_Scene = value;

				OnSceneChange.Raise(this, value);
			}
		}

		public eOccupancyState OccupancyState
		{
			get { return m_OccupancyState; }
			private set
			{
				if (m_OccupancyState == value)
					return;

				m_OccupancyState = value;

				OnOccupancyStateChanged.Raise(this, value);
			}
		}

		public GrafikEyeRoom(int roomId, string name)
		{
			m_Zones = new List<int>();
			m_Shades = new List<int>();
			m_Scenes = new List<int>();
			m_IdToZone = new Dictionary<int, GrafikEyeZoneIntegration>();
			m_IdToShade = new Dictionary<int, GrafikEyeShadeIntegration>();
			m_IdToScene = new Dictionary<int, SceneIntegration>();

			m_Room = roomId;
			m_Name = name;
		}

		/// <summary>
		/// Removes and disposes all of the child integrations.
		/// </summary>
		private void ClearChildren()
		{
			GrafikEye = null;

			ClearZones();
			ClearShades();
			ClearScenes();
		}

		/// <summary>
		/// Removes and disposes all of the child zone integrations.
		/// </summary>
		private void ClearZones()
		{
			foreach (GrafikEyeZoneIntegration zone in m_IdToZone.Values)
				zone.Dispose();

			m_Zones.Clear();
			m_IdToZone.Clear();
		}

		/// <summary>
		/// Removes and disposes all of the child shade integrations.
		/// </summary>
		private void ClearShades()
		{
			foreach (GrafikEyeShadeIntegration shade in m_IdToShade.Values)
				shade.Dispose();

			m_Shades.Clear();
			m_IdToShade.Clear();
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

		/// <summary>
		/// Gets the zone with the given integration id.
		/// </summary>
		/// <param name="integrationId"></param>
		/// <returns></returns>
		IZoneIntegration ILutronRoomContainer.GetZoneIntegration(int integrationId)
		{
			return GetZoneIntegration(integrationId);
		}

		/// <summary>
		/// Gets the zone with the given integration id.
		/// </summary>
		/// <param name="integrationId"></param>
		/// <returns></returns>
		public GrafikEyeZoneIntegration GetZoneIntegration(int integrationId)
		{
			return m_IdToZone[integrationId];
		}

		/// <summary>
		/// Gets the zones.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IZoneIntegration> ILutronRoomContainer.GetZoneIntegrations()
		{
			return GetZoneIntegrations().Cast<IZoneIntegration>();
		}

		/// <summary>
		/// Gets the zones.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<GrafikEyeZoneIntegration> GetZoneIntegrations()
		{
			return m_Zones.Select(i => m_IdToZone[i]).ToArray(m_Zones.Count);
		}

		/// <summary>
		/// Gets the shade group with the given integration id.
		/// </summary>
		/// <param name="integrationId"></param>
		/// <returns></returns>
		public IShadeIntegration GetShadeGroupIntegration(int integrationId)
		{
			// No shade groups are supported
			throw new NotSupportedException();
		}

		/// <summary>
		/// Gets the shade groups.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IShadeIntegration> GetShadeGroupIntegrations()
		{
			// No shade groups are supported
			return Enumerable.Empty<IShadeIntegration>();
		}

		/// <summary>
		/// Gets the shade with the given integration id.
		/// </summary>
		/// <param name="integrationId"></param>
		/// <returns></returns>
		IShadeIntegration ILutronRoomContainer.GetShadeIntegration(int integrationId)
		{
			return GetShadeIntegration(integrationId);
		}

		/// <summary>
		/// Gets the shade with the given integration id.
		/// </summary>
		/// <param name="integrationId"></param>
		/// <returns></returns>
		public GrafikEyeShadeIntegration GetShadeIntegration(int integrationId)
		{
			return m_IdToShade[integrationId];
		}

		/// <summary>
		/// Gets the shades.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IShadeIntegration> ILutronRoomContainer.GetShadeIntegrations()
		{
			return GetShadeIntegrations().Cast<IShadeIntegration>();
		}

		/// <summary>
		/// Gets the shades.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<GrafikEyeShadeIntegration> GetShadeIntegrations()
		{
			return m_Shades.Select(i => m_IdToShade[i]).ToArray(m_Shades.Count);
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
			return m_Scenes.Select(i => m_IdToScene[i]).ToArray(m_Scenes.Count);
		}

		/// <summary>
		/// Sets the current scene for the area.
		/// </summary>
		/// <param name="scene"></param>
		public void SetScene(int? scene)
		{
			GrafikEye.SetScene(scene);
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
			return false;
		}

		#endregion

		#region Grafik Eye Integration Callbacks

		private void Subscribe(GrafikEyeDeviceIntegration grafikEye)
		{
			if (grafikEye == null)
				return;

			grafikEye.OnCurrentSceneChanged += GrafikEyeOnOnCurrentSceneChanged;
		}

		private void Unsubscribe(GrafikEyeDeviceIntegration grafikEye)
		{
			if (grafikEye == null)
				return;

			grafikEye.OnCurrentSceneChanged -= GrafikEyeOnOnCurrentSceneChanged;
		}

		private void GrafikEyeOnOnCurrentSceneChanged(object sender, GenericEventArgs<int?> e)
		{
			Scene = e.Data;
		}

		#endregion

		#region XML

		public static GrafikEyeRoom FromXml(string xml, GrafikEyeNwkDevice parent)
		{
			int roomId = XmlUtils.GetAttributeAsInt(xml, "roomId");
			string name = XmlUtils.GetAttributeAsString(xml, "name");

			string deviceId = XmlUtils.ReadChildElementContentAsString(xml, "IntegrationId");
			string deviceName = string.Format("{0} GrafikEye", name);

			string zonesXml;
			string shadesXml;
			string scenesXml;

			XmlUtils.TryGetChildElementAsString(xml, "Zones", out zonesXml);
			XmlUtils.TryGetChildElementAsString(xml, "Shades", out shadesXml);
			XmlUtils.TryGetChildElementAsString(xml, "Scenes", out scenesXml);

			GrafikEyeRoom output = new GrafikEyeRoom(roomId, name);

			if (!string.IsNullOrEmpty(zonesXml))
				output.ParseZones(zonesXml, parent);

			if (!string.IsNullOrEmpty(shadesXml))
				output.ParseShades(shadesXml, parent);

			if (!string.IsNullOrEmpty(scenesXml))
				output.ParseScenes(scenesXml, parent);

			GrafikEyeDeviceIntegration device = new GrafikEyeDeviceIntegration(deviceId,deviceName, parent, output.GetSceneIntegrations(), output.GetZoneIntegrations(), output.GetShadeIntegrations() );

			output.GrafikEye = device;

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

			IEnumerable<GrafikEyeZoneIntegration> items = XmlUtils.GetChildElementsAsString(zonesXml)
			                                                      .Select(x => GrafikEyeZoneIntegration.FromXml(x, this, parent));

			foreach (GrafikEyeZoneIntegration zone in items)
			{
				if (m_IdToZone.ContainsKey(zone.IntegrationId))
				{
					IcdErrorLog.Warn("{0} {1} already contains zone {2}, skipping", GetType().Name, Room,
					                 zone.IntegrationId);
					continue;
				}

				m_Zones.Add(zone.IntegrationId);
				m_IdToZone[zone.IntegrationId] = zone;
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

			IEnumerable<GrafikEyeShadeIntegration> items = XmlUtils.GetChildElementsAsString(shadesXml)
														  .Select(x => GrafikEyeShadeIntegration.FromXml(x, this, parent));

			foreach (GrafikEyeShadeIntegration shade in items)
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

		#endregion

		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		public string ConsoleName { get { return m_Name; } }

		/// <summary>
		/// Gets the help information for the node.
		/// </summary>
		public string ConsoleHelp { get { return string.Empty; } }

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			yield return ConsoleNodeGroup.KeyNodeMap("Zones", m_IdToZone.Values, z => (uint)z.IntegrationId);
		}

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			yield break;
		}
	}

}
