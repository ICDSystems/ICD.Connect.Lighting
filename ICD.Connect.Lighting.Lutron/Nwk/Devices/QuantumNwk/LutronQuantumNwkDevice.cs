using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Xml;
using ICD.Connect.API.Nodes;
using ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice;
using ICD.Connect.Lighting.Lutron.Nwk.EventArguments;
using ICD.Connect.Lighting.Lutron.Nwk.Integrations;

namespace ICD.Connect.Lighting.Lutron.Nwk.Devices.QuantumNwk
{
	public sealed partial class LutronQuantumNwkDevice : AbstractLutronNwkDevice<LutronQuantumNwkDeviceSettings>
	{

		// Maintain integration order
		private readonly List<int> m_Areas;
		private readonly Dictionary<int, AreaIntegration> m_IdToArea;
		private readonly Dictionary<int, List<AreaIntegration>> m_RoomToAreas;

		public LutronQuantumNwkDevice()
		{
			m_Areas = new List<int>();
			m_IdToArea = new Dictionary<int, AreaIntegration>();
			m_RoomToAreas = new Dictionary<int, List<AreaIntegration>>();
		}

		/// <summary>
		/// Parses the xml document for areas, zones, devices, etc.
		/// </summary>
		/// <param name="xml"></param>
		protected override void ParseXml(string xml)
		{
			ClearIntegrations();

			IEnumerable<AreaIntegration> items = XmlUtils.GetChildElementsAsString(xml)
			                                             .Select(x => AreaIntegration.FromXml(x, this));

			foreach (AreaIntegration area in items)
			{
				if (m_IdToArea.ContainsKey(area.IntegrationId))
				{
					IcdErrorLog.Warn("{0} already contains area {1}, skipping", GetType().Name, area.IntegrationId);
					continue;
				}

				Subscribe(area);

				m_Areas.Add(area.IntegrationId);
				m_IdToArea[area.IntegrationId] = area;

				if (!m_RoomToAreas.ContainsKey(area.Room))
					m_RoomToAreas[area.Room] = new List<AreaIntegration>();
				m_RoomToAreas[area.Room].Add(area);

				RaiseRoomControlsChangedEvent(area.Room);
			}
		}

		/// <summary>
		/// Removes all of the existing integrations from the device.
		/// </summary>
		protected override void ClearIntegrations()
		{
			foreach (int id in m_IdToArea.Keys)
			{
				AreaIntegration area = m_IdToArea[id];
				Unsubscribe(area);
				area.Dispose();
			}

			m_Areas.Clear();
			m_IdToArea.Clear();
			m_RoomToAreas.Clear();
		}

		#region Methods

		/// <summary>
		/// Gets the available rooms.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<int> GetRooms()
		{
			return m_Areas.ToArray();
		}

		/// <summary>
		/// Returns true if the given room is available.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public override bool ContainsRoom(int room)
		{
			return m_IdToArea.ContainsKey(room);
		}

		/// <summary>
		/// Gets the area integration with the given integration id.
		/// </summary>
		/// <param name="integrationId"></param>
		/// <returns></returns>
		[PublicAPI]
		public AreaIntegration GetAreaIntegration(int integrationId)
		{
			return m_IdToArea[integrationId];
		}

		/// <summary>
		/// Gets the area integrations.
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		public IEnumerable<AreaIntegration> GetAreaIntegrations()
		{
			return m_Areas.Select(i => m_IdToArea[i]).ToArray();
		}

		/// <summary>
		/// Gets the area integrations for the given room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		[PublicAPI]
		public override IEnumerable<ILutronRoomContainer> GetRoomContainersForRoom(int room)
		{
			return m_RoomToAreas.GetDefault(room, new List<AreaIntegration>()).ToArray();
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

			yield return ConsoleNodeGroup.IndexNodeMap("Areas", GetAreaIntegrations());
		}

		private IEnumerable<IConsoleNodeBase> GetBaseConsoleNodes()
		{
			return base.GetConsoleNodes();
		}

		#endregion
	}
}
