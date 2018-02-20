using System;
using ICD.Common.Utils.EventArguments;

namespace ICD.Connect.Lighting.Environment
{
	public interface ILightingLoadEnvironmentPeripheral : IEnvironmentPeripheral
	{
		event EventHandler<FloatEventArgs> OnLoadLevelChanged;
		float LoadLevel { get; set; }
		void StartRaising();
		void StartLowering();
		void StopRamping();
	}
}