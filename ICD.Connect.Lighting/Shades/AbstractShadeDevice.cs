using ICD.Connect.Devices;

namespace ICD.Connect.Lighting.Shades
{
	public abstract class AbstractShadeDevice<TSettings> : AbstractDevice<TSettings>, IShadeDevice
		where TSettings : IShadeDeviceSettings, new()
	{
		public abstract void Open();
		public abstract void Close();
	}
}