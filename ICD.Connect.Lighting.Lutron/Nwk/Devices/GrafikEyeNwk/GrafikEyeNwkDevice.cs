using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Common.Utils.Xml;
using ICD.Connect.API.Nodes;

namespace ICD.Connect.Lighting.Lutron.Nwk.Devices.GrafikEyeNwk
{
	public sealed class GrafikEyeNwkDevice : AbstractLutronNwkDevice<GrafikEyeNwkDeviceSettings>
	{
		private readonly Dictionary<int, GrafikEyeRoom> m_Rooms;
		private readonly SafeCriticalSection m_RoomsSection;

		public GrafikEyeNwkDevice()
		{
			m_Rooms = new Dictionary<int, GrafikEyeRoom>();
			m_RoomsSection = new SafeCriticalSection();
		}

		/// <summary>
		/// Gets the available rooms.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<int> GetRooms()
		{
			return m_RoomsSection.Execute(() => m_Rooms.Keys.ToArray(m_Rooms.Count));
		}

		/// <summary>
		/// Returns true if the given room is available.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public override bool ContainsRoom(int room)
		{
			return m_RoomsSection.Execute(() => m_Rooms.ContainsKey(room));
		}

		/// <summary>
		/// Gets the area integrations for the given room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public override IEnumerable<ILutronRoomContainer> GetRoomContainersForRoom(int room)
		{
			m_RoomsSection.Enter();
			try
			{
				GrafikEyeRoom roomContainer;
				if (m_Rooms.TryGetValue(room, out roomContainer))
					yield return roomContainer;
			}
			finally
			{
				m_RoomsSection.Leave();
			}
		}

		/// <summary>
		/// Parses the xml document for areas, zones, devices, etc.
		/// </summary>
		/// <param name="xml"></param>
		protected override void ParseXml(string xml)
		{
			ClearIntegrations();

			IEnumerable<GrafikEyeRoom> items = XmlUtils.GetChildElementsAsString(xml)
			                                           .Select(x => GrafikEyeRoom.FromXml(x, this));

			foreach (GrafikEyeRoom room in items)
			{
				if (!AddRoom(room))
					Logger.Log(eSeverity.Warning, "Already contains room {0}, skipping", room.Room);
			}
		}

		private bool AddRoom(GrafikEyeRoom room)
		{
			m_RoomsSection.Enter();

			try
			{
				if (m_Rooms.ContainsKey(room.Room))
					return false;

				m_Rooms.Add(room.Room, room);

				Subscribe(room);
			}
			finally
			{
				m_RoomsSection.Leave();
			}

			RaiseRoomControlsChangedEvent(room.Room);
			return true;
		}

		/// <summary>
		/// Removes all of the existing integrations from the device.
		/// </summary>
		protected override void ClearIntegrations()
		{
			m_RoomsSection.Enter();
			try
			{
				foreach (KeyValuePair<int, GrafikEyeRoom> kvp in m_Rooms)
				{ 
					GrafikEyeRoom room = kvp.Value;
					Unsubscribe(room);
					room.Dispose();
				}

				m_Rooms.Clear();
			}
			finally
			{
				m_RoomsSection.Leave();
			}
		}

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			foreach (var node in GetBaseConsoleNodes())
				yield return node;

			yield return ConsoleNodeGroup.KeyNodeMap("Rooms",
			                                         m_RoomsSection.Execute(() => m_Rooms.Values.ToArray(m_Rooms.Count)),
			                                                                r => (uint)r.Room);
		}

		private IEnumerable<IConsoleNodeBase> GetBaseConsoleNodes()
		{
			return base.GetConsoleNodes();
		}
	}
}
