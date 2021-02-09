using ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice;
using ICD.Connect.Lighting.Lutron.Nwk.Devices.GrafikEyeNwk;
using ICD.Connect.Lighting.Lutron.Nwk.Integrations.Abstracts;

namespace ICD.Connect.Lighting.Lutron.Nwk.Integrations.GrafikEyeIntegrations
{
	public abstract class AbstractGrafikEyeIntegration : AbstractIntegrationBase<int>
	{
		private readonly GrafikEyeRoom m_Room;

		protected AbstractGrafikEyeIntegration(int integrationId, string name, ILutronNwkDevice parent, GrafikEyeRoom room) : base(integrationId, name, parent)
		{
			m_Room = room;
		}

		public GrafikEyeRoom Room { get { return m_Room; } }
	}
}