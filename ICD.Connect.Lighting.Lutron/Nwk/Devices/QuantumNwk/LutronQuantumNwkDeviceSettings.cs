using ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Lighting.Lutron.Nwk.Devices.QuantumNwk
{
	[KrangSettings("LutronQuantumNwk", typeof(LutronQuantumNwkDevice))]
	public sealed class LutronQuantumNwkDeviceSettings : AbstractLutronNwkDeviceSettings
	{
	}
}
