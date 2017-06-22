using System;

namespace ICD.Connect.Lighting.Lutron.QuantumNwk.EventArguments
{
	public sealed class ZoneOutputLevelEventArgs : EventArgs
	{
		private readonly int m_IntegrationId;
		private float m_Percentage;

		/// <summary>
		/// Gets the zone integration id.
		/// </summary>
		public int IntegrationId { get { return m_IntegrationId; } }

		/// <summary>
		/// Gets the percentage of the zone output level.
		/// </summary>
		public float Percentage { get { return m_Percentage; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="integrationId"></param>
		/// <param name="percentage"></param>
		public ZoneOutputLevelEventArgs(int integrationId, float percentage)
		{
			m_IntegrationId = integrationId;
			m_Percentage = percentage;
		}
	}
}
