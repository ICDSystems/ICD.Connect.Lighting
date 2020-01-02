using System;
using System.Collections.Generic;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice;

namespace ICD.Connect.Lighting.Lutron.Nwk.Devices.LutronNwk
{
	public sealed class LutronNwkDevice : AbstractLutronNwkDevice<LutronNwkDeviceSettings>
	{

		private Dictionary<int, LutronNwkRoom> m_Rooms;

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
			throw new NotImplementedException();
		}

		/// <summary>
		/// Removes all of the existing integrations from the device.
		/// </summary>
		protected override void ClearIntegrations()
		{
			throw new NotImplementedException();
		}

		#endregion


	}
}
