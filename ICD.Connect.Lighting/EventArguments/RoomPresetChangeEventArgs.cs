using System;

namespace ICD.Connect.Lighting.EventArguments
{
	public sealed class RoomPresetChangeEventArgs : EventArgs
	{
		private readonly int m_RoomId;
		private readonly int? m_Preset;

		/// <summary>
		/// Gets the room id.
		/// </summary>
		public int RoomId { get { return m_RoomId; } }

		/// <summary>
		/// Gets the room preset.
		/// </summary>
		public int? Preset { get { return m_Preset; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="roomId"></param>
		/// <param name="preset"></param>
		public RoomPresetChangeEventArgs(int roomId, int? preset)
		{
			m_RoomId = roomId;
			m_Preset = preset;
		}
	}
}
