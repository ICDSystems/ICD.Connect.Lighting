using System;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Lighting.Processors
{
	[KrangSettings(FACTORY_NAME)]
	public sealed class LightingProcessorDeviceSettings : AbstractLightingProcessorDeviceSettings
	{
		private const string FACTORY_NAME = "LightingProcessor";
		
		private const string SHADES_ELEMENT = "Shades";
		private const string SHADE_ELEMENT = "Shade";
		private const string SHADE_GROUP_ELEMENT = "ShadeGroup";

		private const string LOADS_ELEMENT = "Loads";
		private const string LOAD_ELEMENT = "Load";

		private const string PRESETS_ELEMENT = "Presets";
		private const string PRESET_ELEMENT = "Preset";



		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(LightingProcessorDevice); } }
	}
}