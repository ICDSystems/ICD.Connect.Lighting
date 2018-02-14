using ICD.Connect.Devices;

namespace ICD.Connect.Lighting.Shades
{
	public interface IShadeDevice : IDevice
	{
		void Open();
		void Close();
	}
}