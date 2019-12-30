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

namespace ICD.Connect.Lighting.Lutron.Nwk.Integrations
{
	public sealed class AreaIntegration : AbstractIntegration
	{
		private const int ACTION_SCENE = 6;
		private const int ACTION_OCCUPANCY_STATE = 8;

		public delegate void SceneChangeCallback(AreaIntegration sender, int? preset);

		/// <summary>
		/// Raised when the scene for the area changes.
		/// </summary>
		[PublicAPI]
		public event SceneChangeCallback OnSceneChange;

		/// <summary>
		/// Raised when the occupancy state for the area changes.
		/// </summary>
		[PublicAPI]
		public event EventHandler<OccupancyStateEventArgs> OnOccupancyStateChanged;

		/// <summary>
		/// Raised when the output level for a zone changes.
		/// </summary>
		[PublicAPI]
		public event EventHandler<ZoneOutputLevelEventArgs> OnZoneOutputLevelChanged;

		// Maintain integration order
		private readonly List<int> m_Zones;
		private readonly List<int> m_ShadeGroups;
		private readonly List<int> m_Shades;
		private readonly List<int> m_Scenes; 

		private readonly Dictionary<int, ZoneIntegration> m_IdToZone;
		private readonly Dictionary<int, ShadeGroupIntegration> m_IdToShadeGroup;
		private readonly Dictionary<int, ShadeIntegration> m_IdToShade;
		private readonly Dictionary<int, SceneIntegration> m_IdToScene;

		private readonly int m_Room;

		private int? m_Scene;
		private eOccupancyState m_OccupancyState;

		#region Properties

		/// <summary>
		/// Gets the room associated with this area.
		/// </summary>
		[PublicAPI]
		public int Room { get { return m_Room; } }

		/// <summary>
		/// Gets the current scene for the area.
		/// </summary>
		[PublicAPI]
		public int? Scene
		{
			get { return m_Scene; }
			private set
			{
				if (value == m_Scene)
					return;

				m_Scene = value;

				SceneChangeCallback handler = OnSceneChange;
				if (handler != null)
					handler(this, m_Scene);
			}
		}

		/// <summary>
		/// Gets the current occupancy for the area.
		/// </summary>
		[PublicAPI]
		public eOccupancyState OccupancyState
		{
			get { return m_OccupancyState; }
			private set
			{
				if (value == m_OccupancyState)
					return;

				m_OccupancyState = value;

				OnOccupancyStateChanged.Raise(this, new OccupancyStateEventArgs(m_OccupancyState));
			}
		}

		/// <summary>
		/// The string prefix for communication with the lighting processor, e.g. SHADES.
		/// </summary>
		protected override string Command { get { return LutronUtils.COMMAND_AREA; } }

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="room">The room that this area is associated with.</param>
		/// <param name="integrationId"></param>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		private AreaIntegration(int room, int integrationId, string name, ILutronNwkDevice parent)
			: base(integrationId, name, parent)
		{
			m_OccupancyState = eOccupancyState.Unknown;

			m_Zones = new List<int>();
			m_ShadeGroups = new List<int>();
			m_Shades = new List<int>();
			m_Scenes = new List<int>();

			m_IdToZone = new Dictionary<int, ZoneIntegration>();
			m_IdToShadeGroup = new Dictionary<int, ShadeGroupIntegration>();
			m_IdToShade = new Dictionary<int, ShadeIntegration>();
			m_IdToScene = new Dictionary<int, SceneIntegration>();

			m_Room = room;
		}

		/// <summary>
		/// Instantiates an Area from xml.
		/// </summary>
		/// <param name="xml"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static AreaIntegration FromXml(string xml, ILutronNwkDevice parent)
		{
			int room = XmlUtils.GetAttributeAsInt(xml, "room");
			int integrationId = GetIntegrationIdFromXml(xml);
			string name = GetNameFromXml(xml);

			string zonesXml;
			string shadeGroupsXml;
			string shadesXml;
			string scenesXml;

			XmlUtils.TryGetChildElementAsString(xml, "Zones", out zonesXml);
			XmlUtils.TryGetChildElementAsString(xml, "ShadeGroups", out shadeGroupsXml);
			XmlUtils.TryGetChildElementAsString(xml, "Shades", out shadesXml);
			XmlUtils.TryGetChildElementAsString(xml, "Scenes", out scenesXml);

			AreaIntegration output = new AreaIntegration(room, integrationId, name, parent);

			if (!string.IsNullOrEmpty(zonesXml))
				output.ParseZones(zonesXml, parent);

			if (!string.IsNullOrEmpty(shadeGroupsXml))
				output.ParseShadeGroups(shadeGroupsXml, parent);

			if (!string.IsNullOrEmpty(shadesXml))
				output.ParseShades(shadesXml, parent);

			if (!string.IsNullOrEmpty(scenesXml))
				output.ParseScenes(scenesXml, parent);

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
					IcdErrorLog.Warn("{0} {1} already contains zone {2}, skipping", GetType().Name, IntegrationId, zone.IntegrationId);
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
					IcdErrorLog.Warn("{0} {1} already contains shade group {2}, skipping", GetType().Name, IntegrationId,
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
					IcdErrorLog.Warn("{0} {1} already contains shade {2}, skipping", GetType().Name, IntegrationId,
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
					IcdErrorLog.Warn("{0} {1} already contains scene {2}, skipping", GetType().Name, IntegrationId,
									 scene.IntegrationId);
					continue;
				}

				m_Scenes.Add(scene.IntegrationId);
				m_IdToScene[scene.IntegrationId] = scene;
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Release resources.
		/// </summary>
		public override void Dispose()
		{
			OnSceneChange = null;
			OnOccupancyStateChanged = null;
			OnZoneOutputLevelChanged = null;

			base.Dispose();

			ClearChildren();
		}

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
			Execute(ACTION_SCENE, scene);
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

		#region Private Methods

		/// <summary>
		/// Override to query the device when it goes online.
		/// </summary>
		protected override void Initialize()
		{
			base.Initialize();

			Query(ACTION_SCENE);
			Query(ACTION_OCCUPANCY_STATE);
		}

		/// <summary>
		/// Called when we receive a response from the lighting processor for this integration.
		/// </summary>
		/// <param name="action"></param>
		/// <param name="parameters"></param>
		protected override void ParentOnResponse(int action, string[] parameters)
		{
			base.ParentOnResponse(action, parameters);

			switch (action)
			{
				case ACTION_SCENE:
					Scene = String.Equals(parameters[0],LutronUtils.NULL_VALUE,StringComparison.OrdinalIgnoreCase)
						        ? (int?)null
						        : int.Parse(parameters[0]);
					break;

				case ACTION_OCCUPANCY_STATE:
					OccupancyState = (eOccupancyState)int.Parse(parameters[0]);
					break;
			}
		}

		/// <summary>
		/// Removes and disposes all of the child integrations.
		/// </summary>
		private void ClearChildren()
		{
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

		#endregion

		#region ZoneIntegration Callbacks

		/// <summary>
		/// Subscribes to the zone events.
		/// </summary>
		/// <param name="zone"></param>
		private void Subscribe(ZoneIntegration zone)
		{
			zone.OnOutputLevelChanged += ZoneOnOutputLevelChanged;
		}

		/// <summary>
		/// Unsubscribes from the zone events.
		/// </summary>
		/// <param name="zone"></param>
		private void Unsubscribe(ZoneIntegration zone)
		{
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
			Query(ACTION_SCENE);

			ZoneIntegration zone = sender as ZoneIntegration;
			if (zone != null)
				OnZoneOutputLevelChanged.Raise(this, new ZoneOutputLevelEventArgs(zone.IntegrationId, args.Data));
		}

		#endregion


		#region Console

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			foreach (IConsoleNodeBase node in GetBaseConsoleNodes())
				yield return node;

			yield return ConsoleNodeGroup.IndexNodeMap("Zones", GetZoneIntegrations());
			yield return ConsoleNodeGroup.IndexNodeMap("Shade Groups", GetShadeGroupIntegrations());
			yield return ConsoleNodeGroup.IndexNodeMap("Shades", GetShadeIntegrations());
			yield return ConsoleNodeGroup.IndexNodeMap("Scenes", GetSceneIntegrations());
		}

		private IEnumerable<IConsoleNodeBase> GetBaseConsoleNodes()
		{
			return base.GetConsoleNodes();
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new GenericConsoleCommand<int>("SetScene", "Set the scene for the area", s => SetScene(s));
		}

		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			addRow("Room Id", Room);
			addRow("Scene", Scene);
			addRow("Occupancy", OccupancyState);
		}

		#endregion
	}
}
