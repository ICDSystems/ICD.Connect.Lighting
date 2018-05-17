using ICD.Connect.Lighting.Processors;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Lighting.Mock
{
	[KrangSettings("MockLightingProcessor", typeof(MockLightingProcessorDevice))]
	public sealed class MockLightingProcessorDeviceSettings : AbstractLightingProcessorDeviceSettings
	{
	}
}
