using System;
using ICD.Connect.API.Nodes;

namespace ICD.Connect.Lighting.Environment
{
	public interface IEnvironmentPeripheral: IDisposable, IConsoleNode
	{
		LightingProcessorControl.ePeripheralType PeripheralType { get; }
		int Id { get; }
		int Room { get; }
		string Name { get; }

		LightingProcessorControl ToLightingProcessorControl();
	}
}