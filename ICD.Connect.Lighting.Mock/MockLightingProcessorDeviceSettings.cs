﻿using System;
using ICD.Common.Properties;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Attributes.Factories;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Lighting.Mock
{
	public sealed class MockLightingProcessorDeviceSettings : AbstractLightingProcessorDeviceSettings
	{
		private const string FACTORY_NAME = "MockLightingProcessorDevice";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		public override Type OriginatorType
		{
			get { return typeof(MockLightingProcessorDevice); }
		}

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlDeviceSettingsFactoryMethod(FACTORY_NAME)]
		public static MockLightingProcessorDeviceSettings FromXml(string xml)
		{
			MockLightingProcessorDeviceSettings output = new MockLightingProcessorDeviceSettings();
			ParseXml(output, xml);
			return output;
		}
	}
}
