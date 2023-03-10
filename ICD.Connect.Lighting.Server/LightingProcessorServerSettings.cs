using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Xml;
using ICD.Connect.Lighting.Processors;
using ICD.Connect.Settings.Attributes;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Lighting.Server
{
	[KrangSettings("LightingProcessorServer", typeof(LightingProcessorServer))]
	public sealed class LightingProcessorServerSettings : AbstractLightingProcessorDeviceSettings
	{
		private const int DEFAULT_MAX_CLIENTS = 32;
		private const bool DEFAULT_SECURE_SERVER = false;
		
		private const string LIGHTING_PROCESSOR_ID_ELEMENT = "LightingProcessorId";

		private const string SHADES_ELEMENT = "Shades";
		private const string SHADE_ELEMENT = "Shade";

		private const string SHADE_GROUPS_ELEMENT = "ShadeGroups";
		private const string SHADE_GROUP_ELEMENT = "ShadeGroup";

		private const string LOADS_ELEMENT = "Loads";
		private const string LOAD_ELEMENT = "Load";

		private const string PRESETS_ELEMENT = "Presets";
		private const string PRESET_ELEMENT = "Preset";

		private const string OCCUPANCY_SENSORS_ELEMENT = "OccupancySensors";
		private const string OCCUPANCY_SENSOR_ELEMENT = "OccupancySensor";

		private const string ROOM_ID_ELEMENT = "RoomId";

		private const string SERVER_USE_SECURE_ELEMENT = "UseSecureServer";
		private const string SERVER_PORT_ELEMENT = "ServerPort";
		private const string SERVER_CLIENTS_ELEMENT = "ServerMaxClients";

		private Dictionary<int, IcdHashSet<int>> m_PeripheralsByRoomId;

		public int? LightingProcessorId { get; set; }
		public ushort ServerPort { get; set; }
		public int ServerMaxClients { get; set; }
		public bool UseSecureServer { get; set; }

		[HiddenSettingsProperty]
		public IcdHashSet<int> ShadeIds { get; private set; }

		[HiddenSettingsProperty]
		public IcdHashSet<int> ShadeGroupIds { get; private set; }

		[HiddenSettingsProperty]
		public IcdHashSet<int> LoadIds { get; private set; }

		[HiddenSettingsProperty]
		public IcdHashSet<int> PresetIds { get; set; }

		[HiddenSettingsProperty]
		public IcdHashSet<int> OccupancySensorIds { get; set; }

		[HiddenSettingsProperty]
		public IEnumerable<int> RoomIds
		{
			get { return m_PeripheralsByRoomId.Keys; }
			set
			{
				m_PeripheralsByRoomId.Clear();
				foreach (int val in value)
					m_PeripheralsByRoomId[val] = new IcdHashSet<int>();
			}
		}

		public void AddLoad(int roomId, int id)
		{
			if (LoadIds == null)
			{
				LoadIds = new IcdHashSet<int>();
			}

			LoadIds.Add(id);

			AddPeripheralToPeripheralsByRoomIdCollection(roomId, id);
		}

		public void AddShade(int roomId, int id)
		{
			if (ShadeIds == null)
			{
				ShadeIds = new IcdHashSet<int>();
			}

			ShadeIds.Add(id);

			AddPeripheralToPeripheralsByRoomIdCollection(roomId, id);
		}
        
		public void AddShadeGroup(int roomId, int id)
		{
			if (ShadeGroupIds == null)
			{
				ShadeGroupIds = new IcdHashSet<int>();
			}

			ShadeGroupIds.Add(id);

			AddPeripheralToPeripheralsByRoomIdCollection(roomId, id);
		}

		public void AddPreset(int roomId, int id)
		{
			if (PresetIds == null)
			{
				PresetIds = new IcdHashSet<int>();
			}

			PresetIds.Add(id);
		}

		public void AddOccupancySensor(int roomId, int id)
		{
			if (OccupancySensorIds == null)
			{
				OccupancySensorIds = new IcdHashSet<int>();
			}

			OccupancySensorIds.Add(id);

			AddPeripheralToPeripheralsByRoomIdCollection(roomId, id);
		}

		public void ClearIdCollections()
		{
			m_PeripheralsByRoomId = new Dictionary<int, IcdHashSet<int>>();
			LoadIds = new IcdHashSet<int>();
			ShadeIds = new IcdHashSet<int>();
			ShadeGroupIds = new IcdHashSet<int>();
			PresetIds = new IcdHashSet<int>();
			OccupancySensorIds = new IcdHashSet<int>();
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			LightingProcessorId = XmlUtils.TryReadChildElementContentAsInt(xml, LIGHTING_PROCESSOR_ID_ELEMENT);

			ShadeIds = XmlUtils.ReadListFromXml(xml, SHADES_ELEMENT, SHADE_ELEMENT, 
												content => ParseChildPeripheral(content))
							   .ToIcdHashSet();
			
			ShadeGroupIds = XmlUtils.ReadListFromXml(xml, SHADE_GROUPS_ELEMENT, SHADE_GROUP_ELEMENT,
													 content => ParseChildPeripheral(content))
									.ToIcdHashSet();

			LoadIds = XmlUtils.ReadListFromXml(xml, LOADS_ELEMENT, LOAD_ELEMENT,
											   content => ParseChildPeripheral(content))
							  .ToIcdHashSet();

			PresetIds = XmlUtils.ReadListFromXml(xml, PRESETS_ELEMENT, PRESET_ELEMENT,
												 content => ParseChildPreset(content))
								.ToIcdHashSet();

			OccupancySensorIds = XmlUtils.ReadListFromXml(xml, OCCUPANCY_SENSORS_ELEMENT, OCCUPANCY_SENSOR_ELEMENT,
												content => ParseChildPeripheral(content)).
												ToIcdHashSet();


			ServerPort = XmlUtils.ReadChildElementContentAsUShort(xml, SERVER_PORT_ELEMENT);
			ServerMaxClients = XmlUtils.TryReadChildElementContentAsInt(xml, SERVER_CLIENTS_ELEMENT) ?? DEFAULT_MAX_CLIENTS;
			UseSecureServer = XmlUtils.TryReadChildElementContentAsBoolean(xml, SERVER_USE_SECURE_ELEMENT) ?? DEFAULT_SECURE_SERVER;
		}

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);
			
			writer.WriteElementString(LIGHTING_PROCESSOR_ID_ELEMENT, LightingProcessorId.ToString());

			writer.WriteElementString(SERVER_PORT_ELEMENT, IcdXmlConvert.ToString(ServerPort));
			writer.WriteElementString(SERVER_CLIENTS_ELEMENT, IcdXmlConvert.ToString(ServerMaxClients));
			writer.WriteElementString(SERVER_USE_SECURE_ELEMENT, IcdXmlConvert.ToString(UseSecureServer));

			WriteLoads(writer);

			WriteShadeAndShadeGroups(writer);

			WritePresets(writer);

			WriteOccupancySensors(writer);
		}

		/// <summary>
		/// Writes all presets to the xml with their room id as an attribute.
		/// </summary>
		/// <param name="writer"></param>
		private void WritePresets(IcdXmlTextWriter writer)
		{
			writer.WriteStartElement(PRESETS_ELEMENT);
			foreach (int preset in PresetIds)
			{
				writer.WriteStartElement(SHADE_ELEMENT);
				int? roomId = FindRoomIdForPeripheral(preset);
				if (roomId != null)
				{
					writer.WriteAttributeString(ROOM_ID_ELEMENT, roomId.Value.ToString());
				}
				writer.WriteValue(preset);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		/// <summary>
		/// Writes all shades and shade groups to the xml with their room id as an attribute.
		/// </summary>
		/// <param name="writer"></param>
		private void WriteShadeAndShadeGroups(IcdXmlTextWriter writer)
		{
			writer.WriteStartElement(SHADES_ELEMENT);
			foreach (int shade in ShadeIds)
			{
				writer.WriteStartElement(SHADE_ELEMENT);
				int? roomId = FindRoomIdForPeripheral(shade);
				if (roomId != null)
				{
					writer.WriteAttributeString(ROOM_ID_ELEMENT, roomId.Value.ToString());
				}
				writer.WriteValue(shade);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			writer.WriteStartElement(SHADE_GROUPS_ELEMENT);
			foreach (int shade in ShadeGroupIds)
			{
				writer.WriteStartElement(SHADE_GROUP_ELEMENT);
				int? roomId = FindRoomIdForPeripheral(shade);
				if (roomId != null)
				{
					writer.WriteAttributeString(ROOM_ID_ELEMENT, roomId.Value.ToString());
				}
				writer.WriteValue(shade);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		/// <summary>
		/// Writes all loads to the xml with their room id as an attribute.
		/// </summary>
		/// <param name="writer"></param>
		private void WriteLoads(IcdXmlTextWriter writer)
		{
			writer.WriteStartElement(LOADS_ELEMENT);
			foreach (int load in LoadIds)
			{
				writer.WriteStartElement(LOAD_ELEMENT);
				int? roomId = FindRoomIdForPeripheral(load);
				if (roomId != null)
				{
					writer.WriteAttributeString(ROOM_ID_ELEMENT, roomId.Value.ToString());
				}
				writer.WriteValue(load);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		private void WriteOccupancySensors(IcdXmlTextWriter writer)
		{
			writer.WriteStartElement(OCCUPANCY_SENSORS_ELEMENT);
			foreach (int sensor in OccupancySensorIds)
			{
				writer.WriteStartElement(OCCUPANCY_SENSOR_ELEMENT);
				int? roomId = FindRoomIdForPeripheral(sensor);
				if (roomId != null)
					writer.WriteAttributeString(ROOM_ID_ELEMENT, roomId.Value.ToString());
				writer.WriteValue(sensor);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		/// <summary>
		/// Adds the element to the m_PeripheralsByRoomId collection, then returns the ID.
		/// </summary>
		/// <param name="content">the xml to parse</param>
		private int ParseChildPeripheral(string content)
		{
			int id = XmlUtils.ReadElementContentAsInt(content);
			int roomId = XmlUtils.GetAttributeAsInt(content, ROOM_ID_ELEMENT);

			AddPeripheralToPeripheralsByRoomIdCollection(roomId, id);

			return id;
		}

		/// <summary>
		/// Parses a child preset
		/// </summary>
		/// <param name="content">the xml to parse</param>
		private int ParseChildPreset(string content)
		{
			int id = XmlUtils.ReadElementContentAsInt(content);
			int roomId = XmlUtils.GetAttributeAsInt(content, ROOM_ID_ELEMENT);

			if (!m_PeripheralsByRoomId.ContainsKey(roomId))
			{
				m_PeripheralsByRoomId[roomId] = new IcdHashSet<int>();
			}

			return id;
		}

		/// <summary>
		/// Returns the first room that a given peripheral belongs to, or null if the given id
		/// is not a member of any room.
		/// </summary>
		/// <param name="id"></param>
		public int? FindRoomIdForPeripheral(int id)
		{
			return m_PeripheralsByRoomId.Where(kvp => kvp.Value.Contains(id))
										.Select(kvp => kvp.Key)
										.FirstOrDefault();
		}

		/// <summary>
		/// Adds the peripheral to the room id collection.
		/// </summary>
		/// <param name="roomId"></param>
		/// <param name="id"></param>
		private void AddPeripheralToPeripheralsByRoomIdCollection(int roomId, int id)
		{
			if (m_PeripheralsByRoomId == null)
			{
				m_PeripheralsByRoomId = new Dictionary<int, IcdHashSet<int>>
				{
					{roomId, new IcdHashSet<int>()}
				};
			}

			if (!m_PeripheralsByRoomId.ContainsKey(roomId))
			{
				m_PeripheralsByRoomId[roomId] = new IcdHashSet<int>();
			}

			m_PeripheralsByRoomId[roomId].Add(id);
		}
	}
}