using ICD.Connect.Devices.Controls;

namespace ICD.Connect.Lighting.Shades.Controls
{
	public interface IShadeSetPositionControl : IDeviceControl
	{
		void SetPosition(float position);
	}
}