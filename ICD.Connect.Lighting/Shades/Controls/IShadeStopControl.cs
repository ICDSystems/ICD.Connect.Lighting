using ICD.Connect.Devices.Controls;

namespace ICD.Connect.Lighting.Shades.Controls
{
	public interface IShadeStopControl : IDeviceControl
	{
		void Stop();
	}
}