using System;
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
				Logger.AddEntry(eSeverity.Error, "{0} failed to instantiate {1} - CresnetId {2} is out of range",
				                this, typeof(Crestron.SimplSharpPro.Shades.CsmQmt50Dccn).Name, settings.CresnetId);
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
				string message = string.Format("{0} failed to instantiate {1} with Cresnet ID {2} - {3}",
											   this, typeof(Crestron.SimplSharpPro.Shades.CsmQmt50Dccn).Name, settings.CresnetId, e.Message);
				Logger.AddEntry(eSeverity.Error, e, message);
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