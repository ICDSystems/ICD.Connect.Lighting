using System;
using ICD.Common.Logging.LoggingContexts;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Misc.CrestronPro;
using ICD.Connect.Misc.CrestronPro.Utils;
using ICD.Connect.Settings;

namespace ICD.Connect.Lighting.CrestronPro.Shades.CsmQmt50Dccn
{
#if SIMPLSHARP
	public sealed class CsmQmt50DccnAdapter : AbstractShadeWithBasicSettingsAdapter<Crestron.SimplSharpPro.Shades.CsmQmt50Dccn, 
																			 CsmQmt50DccnAdapterSettings>
#else
	public sealed class CsmQmt50DccnAdapter : AbstractShadeWithBasicSettingsAdapter<CsmQmt50DccnAdapterSettings>
#endif
	{
		#region Settings

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(CsmQmt50DccnAdapterSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

#if SIMPLSHARP
			if (settings.CresnetId == null || !CresnetUtils.IsValidId(settings.CresnetId.Value))
			{
				Logger.Log(eSeverity.Error, "Failed to instantiate {0} - CresnetId {1} is out of range",
				           typeof(Crestron.SimplSharpPro.Shades.CsmQmt50Dccn).Name, settings.CresnetId);
				return;
			}

			Crestron.SimplSharpPro.Shades.CsmQmt50Dccn shade = null;
			try
			{
				shade = CresnetUtils.InstantiateCresnetDevice(settings.CresnetId.Value,
															  settings.BranchId,
															  settings.ParentId,
															  factory,
															  cresnetId => new Crestron.SimplSharpPro.Shades.CsmQmt50Dccn(cresnetId, ProgramInfo.ControlSystem),
															  (cresnetId, branch) => new Crestron.SimplSharpPro.Shades.CsmQmt50Dccn(cresnetId, branch));
			}
			catch (ArgumentException e)
			{
				Logger.Log(eSeverity.Error, e, "Failed to instantiate {0} with Cresnet ID {1} - {2}",
				           typeof(Crestron.SimplSharpPro.Shades.CsmQmt50Dccn).Name, settings.CresnetId, e.Message);
			}

			SetShade(shade);
#endif
		}

		protected override void CopySettingsFinal(CsmQmt50DccnAdapterSettings settings)
		{
			base.CopySettingsFinal(settings);

#if SIMPLSHARP
			settings.CresnetId = Shade == null ? (byte)0 : (byte)Shade.ID;
#else
            settings.CresnetId = 0;
#endif
		}

		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

#if SIMPLSHARP
			SetShade(null);
#endif
		}

		#endregion
	}
}