using ICD.Connect.Devices;

namespace ICD.Connect.Lighting.Shades
{
	public interface IShadeDeviceSettings : IDeviceSettings
	{
		 eShadeType ShadeType { get; set; }
	}
}