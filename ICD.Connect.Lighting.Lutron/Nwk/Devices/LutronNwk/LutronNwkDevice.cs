﻿using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Xml;
using ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice;

namespace ICD.Connect.Lighting.Lutron.Nwk.Devices.LutronNwk
{
	public sealed class LutronNwkDevice : AbstractLutronNwkDevice<LutronNwkDeviceSettings>
	{

		private readonly Dictionary<int, LutronNwkRoom> m_Rooms;

		public LutronNwkDevice()
		{
			m_Rooms = new Dictionary<int, LutronNwkRoom>();
		}

		#region Abstract Implementations
		/// <summary>
		/// Gets the available rooms.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<int> GetRooms()
		{
			return m_Rooms.Keys.ToArray(m_Rooms.Count);
		}

		/// <summary>
		/// Returns true if the given room is available.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public override bool ContainsRoom(int room)
		{
			return m_Rooms.ContainsKey(room);
		}

		/// <summary>
		/// Gets the area integrations for the given room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public override IEnumerable<ILutronRoomContainer> GetRoomContainersForRoom(int room)
		{
			LutronNwkRoom roomContainer;
			if (m_Rooms.TryGetValue(room, out roomContainer))
				yield return roomContainer;
		}

		/// <summary>
		/// Parses the xml document for areas, zones, devices, etc.
		/// </summary>
		/// <param name="xml"></param>
		protected override void ParseXml(string xml)
		{
			ClearIntegrations();

			IEnumerable<LutronNwkRoom> items = XmlUtils.GetChildElementsAsString(xml)
			                                             .Select(x => LutronNwkRoom.FromXml(x, this));

			foreach (LutronNwkRoom room in items)
			{
				if (m_Rooms.ContainsKey(room.Room))
				{
					IcdErrorLog.Warn("{0} already contains area {1}, skipping", GetType().Name, room.Room);
					continue;
				}

				Subscribe(room);

				m_Rooms.Add(room.Room, room);

				RaiseRoomControlsChangedEvent(room.Room);
			}
		}

		/// <summary>
		/// Removes all of the existing integrations from the device.
		/// </summary>
		protected override void ClearIntegrations()
		{
			foreach (KeyValuePair<int, LutronNwkRoom> kvp in m_Rooms)
			{
				LutronNwkRoom room = kvp.Value;
				Unsubscribe(room);
				room.Dispose();
			}

			m_Rooms.Clear();
		}

		#endregion

	}
}