using ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Lighting.Lutron.Nwk.Devices.GrafikEyeNwk
{
	[KrangSettings("LutronGrafikEye", typeof(GrafikEyeNwkDevice))]
	public sealed class GrafikEyeNwkDeviceSettings : AbstractLutronNwkDeviceSettings
	{
	}
}
