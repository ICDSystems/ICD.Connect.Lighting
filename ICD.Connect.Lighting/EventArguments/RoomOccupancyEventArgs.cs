using System;
using ICD.Connect.Misc.Occupancy;

namespace ICD.Connect.Lighting.EventArguments
{
	public sealed class RoomOccupancyEventArgs : EventArgs
	{
		private readonly int m_RoomId;
		private readonly eOccupancyState m_OccupancyState;

		/// <summary>
		/// Gets the room id.
		/// </summary>
		public int RoomId { get { return m_RoomId; } }

		/// <summary>
		/// Gets the room occupancy state.
		/// </summary>
		public eOccupancyState OccupancyState { get { return m_OccupancyState; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="roomId"></param>
		/// <param name="occupancyState"></param>
		public RoomOccupancyEventArgs(int roomId, eOccupancyState occupancyState)
		{
			m_RoomId = roomId;
			m_OccupancyState = occupancyState;
		}
	}
}
