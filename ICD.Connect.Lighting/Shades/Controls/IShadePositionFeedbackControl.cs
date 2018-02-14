using ICD.Connect.Devices.Controls;

namespace ICD.Connect.Lighting.Shades.Controls
{
	public interface IShadePositionFeedbackControl : IDeviceControl
	{
		float GetPosition();
	}
}