using System.Collections.Generic;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices.Mock;
using ICD.Connect.Lighting.Shades;
using ICD.Connect.Lighting.Shades.Controls;
using ICD.Connect.Settings;

namespace ICD.Connect.Lighting.Mock
{
    public sealed class MockShadeDevice : AbstractShadeDevice<MockShadeDeviceSettings>,
		IShadeWithStop, IMockDevice
    {
	    private bool m_IsOnline;

	    public bool DefaultOffline { get; set; }

	    public MockShadeDevice()
	    {
			Controls.Add(new ShadeStopControl<MockShadeDevice>(this, 1));
		}

	    public void SetIsOnlineState(bool isOnline)
	    {
		    m_IsOnline = isOnline;
			UpdateCachedOnlineStatus();
	    }

	    protected override bool GetIsOnlineStatus()
	    {
		    return m_IsOnline;
	    }

	    public override void Open()
	    {
		    Logger.Log(eSeverity.Debug, "Mock Shade {0} Opening.", Id);
	    }

	    public override void Close()
	    {
			Logger.Log(eSeverity.Debug, "Mock Shade {0} Closing.", Id);
		}

	    public void Stop()
	    {
			Logger.Log(eSeverity.Debug, "Mock Shade {0} Stopped.", Id);
		}

	    #region Settings

	    /// <summary>
	    /// Override to apply settings to the instance.
	    /// </summary>
	    /// <param name="settings"></param>
	    /// <param name="factory"></param>
	    protected override void ApplySettingsFinal(MockShadeDeviceSettings settings, IDeviceFactory factory)
	    {
		    base.ApplySettingsFinal(settings, factory);

			MockDeviceHelper.ApplySettings(this, settings);
	    }

	    /// <summary>
	    /// Override to apply properties to the settings instance.
	    /// </summary>
	    /// <param name="settings"></param>
	    protected override void CopySettingsFinal(MockShadeDeviceSettings settings)
	    {
		    base.CopySettingsFinal(settings);

			MockDeviceHelper.CopySettings(this, settings);
	    }

	    /// <summary>
	    /// Override to clear the instance settings.
	    /// </summary>
	    protected override void ClearSettingsFinal()
	    {
		    base.ClearSettingsFinal();

			MockDeviceHelper.ClearSettings(this);
	    }

	    #endregion

	    #region Console

	    /// <summary>
	    /// Gets the child console commands.
	    /// </summary>
	    /// <returns></returns>
	    public override IEnumerable<IConsoleCommand> GetConsoleCommands()
	    {
		    foreach (IConsoleCommand command in GetBaseConsoleCommands())
			    yield return command;

		    foreach (IConsoleCommand command in MockDeviceHelper.GetConsoleCommands(this))
			    yield return command;
	    }

	    private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
	    {
			return base.GetConsoleCommands();
	    }

	    /// <summary>
	    /// Calls the delegate for each console status item.
	    /// </summary>
	    /// <param name="addRow"></param>
	    public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
	    {
		    base.BuildConsoleStatus(addRow);

			MockDeviceHelper.BuildConsoleStatus(this, addRow);
	    }

	    #endregion
    }
}
