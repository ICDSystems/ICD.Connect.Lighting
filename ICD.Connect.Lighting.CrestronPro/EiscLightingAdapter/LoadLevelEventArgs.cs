using System;

namespace ICD.Connect.Lighting.CrestronPro.EiscLightingAdapter
{
	public sealed class LoadLevelEventArgs : EventArgs
	{
		private readonly int m_LoadId;
		private readonly float m_Percentage;

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
		/// <param name="loadId"></param>
		/// <param name="percentage"></param>
		public LoadLevelEventArgs(int loadId, float percentage)
		{
			m_LoadId = loadId;
			m_Percentage = percentage;
		}
	}
}