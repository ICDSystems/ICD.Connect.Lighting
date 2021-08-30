using System;
using ICD.Common.Logging.LoggingContexts;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Nodes;
using ICD.Connect.Misc.CrestronPro;
using ICD.Connect.Misc.CrestronPro.Cresnet;
using ICD.Connect.Misc.CrestronPro.Utils;
using ICD.Connect.Settings;

namespace ICD.Connect.Lighting.CrestronPro.Shades.CsmQmt50Dccn
{
#if !NETSTANDARD
	public sealed class CsmQmt50DccnAdapter :
		AbstractShadeWithBasicSettingsAdapter<Crestron.SimplSharpPro.Shades.CsmQmt50Dccn, CsmQmt50DccnAdapterSettings>,
		ICresnetDevice
#else
	public sealed class CsmQmt50DccnAdapter :
		AbstractShadeWithBasicSettingsAdapter<CsmQmt50DccnAdapterSettings>,
		ICresnetDevice
#endif
	{
		private readonly CresnetInfo m_CresnetInfo;

		#region Properties

		public CresnetInfo CresnetInfo { get { return m_CresnetInfo; } }

		#endregion

		public CsmQmt50DccnAdapter()
		{
			m_CresnetInfo = new CresnetInfo();
		}

		#region Settings

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(CsmQmt50DccnAdapterSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			CresnetInfo.ApplySettings(settings);

#if !NETSTANDARD
			if (m_CresnetInfo.CresnetId == null || !CresnetUtils.IsValidId(m_CresnetInfo.CresnetId.Value))
			{
				Logger.Log(eSeverity.Error, "Failed to instantiate {0} - CresnetId {1} is out of range",
						   typeof(Crestron.SimplSharpPro.Shades.CsmQmt50Dccn).Name, m_CresnetInfo.CresnetId);
				return;
			}

			Crestron.SimplSharpPro.Shades.CsmQmt50Dccn shade = null;
			try
			{
				shade = CresnetUtils.InstantiateCresnetDevice(m_CresnetInfo.CresnetId.Value,
															  m_CresnetInfo.BranchId,
															  m_CresnetInfo.ParentId,
															  factory,
															  cresnet => new Crestron.SimplSharpPro.Shades.CsmQmt50Dccn(cresnet, ProgramInfo.ControlSystem),
															  (cresnet, branch) => new Crestron.SimplSharpPro.Shades.CsmQmt50Dccn(cresnet, branch));
			}
			catch (ArgumentException e)
			{
				Logger.Log(eSeverity.Error, e, "Failed to instantiate {0} with Cresnet ID {1} - {2}",
						   typeof(Crestron.SimplSharpPro.Shades.CsmQmt50Dccn).Name, m_CresnetInfo.CresnetId, e.Message);
			}

			SetShade(shade);
#endif
		}

		protected override void CopySettingsFinal(CsmQmt50DccnAdapterSettings settings)
		{
			base.CopySettingsFinal(settings);

			CresnetInfo.CopySettings(settings);
		}

		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			CresnetInfo.ClearSettings();

#if !NETSTANDARD
			SetShade(null);
#endif
		}

		#endregion

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			CresnetDeviceConsole.BuildConsoleStatus(this, addRow);
		}

		#endregion
	}
}