using ICD.Common.Utils.Timers;
using ICD.Connect.Protocol.Ports.RelayPort;
using ICD.Connect.Settings;

namespace ICD.Connect.Lighting.Shades.RelayShadeDevice
{
	public sealed class RelayShadeDevice : AbstractShadeDevice<RelayShadeDeviceSettings>
	{
		private const long DEFAULT_RELAY_CLOSE_TIME = 250;

		private IRelayPort m_OpenRelay;
		private IRelayPort m_CloseRelay;
		private long? m_RelayCloseTime;

		private readonly SafeTimer m_ResetOpenRelayTimer;
		private readonly SafeTimer m_ResetCloseRelayTimer;

		private long RelayCloseTime
		{
			get
			{
				if (m_RelayCloseTime.HasValue)
					return m_RelayCloseTime.Value;
				return DEFAULT_RELAY_CLOSE_TIME;
			}
		}

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
			m_ResetOpenRelayTimer.Reset(RelayCloseTime);

		}

		public override void Close()
		{
			ResetOpenRelay();

			if (m_CloseRelay == null)
				return;

			m_CloseRelay.Close();
			m_ResetCloseRelayTimer.Reset(RelayCloseTime);
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

			if (settings.OpenRelay.HasValue)
				m_OpenRelay = factory.GetOriginatorById<IRelayPort>(settings.OpenRelay.Value);
			if (settings.CloseRelay.HasValue)
				m_CloseRelay = factory.GetOriginatorById<IRelayPort>(settings.CloseRelay.Value);
			
			m_RelayCloseTime = settings.RelayCloseTime;
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(RelayShadeDeviceSettings settings)
		{
			base.CopySettingsFinal(settings);
			settings.OpenRelay = m_OpenRelay != null ? m_OpenRelay.Id : (int?)null;
			settings.CloseRelay = m_CloseRelay != null ? m_CloseRelay.Id : (int?)null;
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
			m_RelayCloseTime = null;
		}

		#endregion
	}
}