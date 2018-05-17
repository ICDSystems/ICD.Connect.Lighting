using ICD.Connect.Lighting.Shades;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Lighting.Mock
{
	[KrangSettings("MockShade", typeof(MockShadeDevice))]
	public sealed class MockShadeDeviceSettings : AbstractShadeDeviceSettings
	{
	}
}
