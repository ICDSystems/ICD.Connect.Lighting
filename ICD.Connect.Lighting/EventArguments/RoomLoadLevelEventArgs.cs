using System;

namespace ICD.Connect.Lighting.EventArguments
{
	public sealed class RoomLoadLevelEventArgs : EventArgs
	{
		private readonly int m_RoomId;
		private readonly int m_LoadId;
		private readonly float m_Percentage;

		/// <summary>
		/// Gets the id of the room.
		/// </summary>
		public int RoomId { get { return m_RoomId; } }

		/// <summary>
		/// Gets the id of the lighting load.
		/// </summary>
		public int LoadId { get { return m_LoadId; } }

		/// <summary>
		/// Gets the level of the lighting load as a percentage.
		/// </summary>
		public float Percentage { get { return m_Percentage; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="roomId"></param>
		/// <param name="loadId"></param>
		/// <param name="percentage"></param>
		public RoomLoadLevelEventArgs(int roomId, int loadId, float percentage)
		{
			m_RoomId = roomId;
			m_LoadId = loadId;
			m_Percentage = percentage;
		}
	}
}
