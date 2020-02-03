using ICD.Connect.API.Nodes;

namespace ICD.Connect.Lighting.CrestronPro.EiscLightingAdapter.Components
{
	public interface IEiscLightingComponent: IConsoleNode
	{
		int Id { get; }
		string Name { get; }

		LightingProcessorControl ToLightingProcessorControl();
	}
}