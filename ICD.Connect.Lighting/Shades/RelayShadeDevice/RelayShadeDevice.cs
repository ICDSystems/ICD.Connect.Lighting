using ICD.Common.Utils.Timers;
using ICD.Connect.Protocol.Ports.RelayPort;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Lighting.Shades.RelayShadeDevice
{
	public sealed class RelayShadeDevice : AbstractShadeDevice<RelayShadeDeviceSettings>
	{
		private IRelayPort m_OpenRelay;
		private IRelayPort m_CloseRelay;
		private long m_RelayCloseTime;

		private readonly SafeTimer m_ResetOpenRelayTimer;
		private readonly SafeTimer m_ResetCloseRelayTimer;

		#region IShadeDevice
		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return m_OpenRelay != null &&
			       m_OpenRelay.IsOnline &&
			       m_CloseRelay != null &&
			       m_CloseRelay.IsOnline;
		}

		public override void Open()
		{
			ResetCloseRelay();

			if (m_OpenRelay == null)
				return;

			m_OpenRelay.Close();
			m_ResetOpenRelayTimer.Reset(m_RelayCloseTime);

		}

		public override void Close()
		{
			ResetOpenRelay();

			if (m_CloseRelay == null)
				return;

			m_CloseRelay.Close();
			m_ResetCloseRelayTimer.Reset(m_RelayCloseTime);
		}

		#endregion

		public RelayShadeDevice()
		{
			m_ResetOpenRelayTimer = SafeTimer.Stopped(ResetOpenRelay);
			m_ResetCloseRelayTimer = SafeTimer.Stopped(ResetCloseRelay);
		}

		private void ResetOpenRelay()
		{
			if (m_OpenRelay != null)
				m_OpenRelay.Open();
		}

		private void ResetCloseRelay()
		{
			if (m_CloseRelay != null)
				m_CloseRelay.Open();
		}

		#region Settings

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(RelayShadeDeviceSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			m_OpenRelay = factory.GetOriginatorById<IRelayPort>(settings.OpenRelay);
			m_CloseRelay = factory.GetOriginatorById<IRelayPort>(settings.CloseRelay);
			m_RelayCloseTime = settings.RelayCloseTime;
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(RelayShadeDeviceSettings settings)
		{
			base.CopySettingsFinal(settings);
			settings.OpenRelay = m_OpenRelay != null ? m_OpenRelay.Id : 0;
			settings.CloseRelay = m_CloseRelay != null ? m_CloseRelay.Id : 0;
			settings.RelayCloseTime = m_RelayCloseTime;
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			m_OpenRelay = null;
			m_CloseRelay = null;
			m_RelayCloseTime = 0;
		}

		#endregion
	}
}