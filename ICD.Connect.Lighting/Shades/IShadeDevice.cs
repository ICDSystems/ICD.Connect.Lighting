using ICD.Connect.Devices;

namespace ICD.Connect.Lighting.Shades
{
	public interface IShadeDevice : IDevice
	{
		eShadeType ShadeType { get; }
		void Open();
		void Close();
	}
}