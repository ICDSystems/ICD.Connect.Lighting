using System;
using ICD.Connect.Lighting.Shades;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Lighting.Mock
{
	[KrangSettings(FACTORY_NAME)]
	public sealed class MockShadeDeviceSettings : AbstractShadeDeviceSettings
	{
		private const string FACTORY_NAME = "MockShade";

		public override string FactoryName { get { return FACTORY_NAME; } }

		public override Type OriginatorType { get { return typeof(MockShadeDevice); } }
	}
}
