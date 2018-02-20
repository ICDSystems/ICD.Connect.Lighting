using System;
using ICD.Connect.Lighting.Processors;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Lighting.Mock
{
	[KrangSettings(FACTORY_NAME)]
	public sealed class MockLightingProcessorDeviceSettings : AbstractLightingProcessorDeviceSettings
	{
		private const string FACTORY_NAME = "MockLightingProcessorDevice";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		public override Type OriginatorType { get { return typeof(MockLightingProcessorDevice); } }
	}
}
