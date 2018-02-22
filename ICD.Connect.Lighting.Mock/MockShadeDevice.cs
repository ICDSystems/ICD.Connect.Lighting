using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Lighting.Shades;
using ICD.Connect.Lighting.Shades.Controls;

namespace ICD.Connect.Lighting.Mock
{
    public sealed class MockShadeDevice : AbstractShadeDevice<MockShadeDeviceSettings>,
		IShadeWithStop
    {
	    public MockShadeDevice()
	    {
			Controls.Add(new ShadeStopControl<MockShadeDevice>(this, 1));
		}

	    protected override bool GetIsOnlineStatus()
	    {
		    return true;
	    }

	    public override void Open()
	    {
		    Logger.AddEntry(eSeverity.Debug, "Mock Shade {0} Opening.", Id);
	    }

	    public override void Close()
	    {
			Logger.AddEntry(eSeverity.Debug, "Mock Shade {0} Closing.", Id);
		}

	    public void Stop()
	    {
			Logger.AddEntry(eSeverity.Debug, "Mock Shade {0} Stopped.", Id);
		}
    }
}
