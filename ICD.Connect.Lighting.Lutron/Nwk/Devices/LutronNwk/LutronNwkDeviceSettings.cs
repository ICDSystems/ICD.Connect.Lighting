using ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Lighting.Lutron.Nwk.Devices.LutronNwk
{
	[KrangSettings("LutronNwk", typeof(LutronNwkDevice))]
	public sealed class LutronNwkDeviceSettings : AbstractLutronNwkDeviceSettings
	{
	}
}
