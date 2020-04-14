using ICD.Connect.Devices;

namespace ICD.Connect.Lighting.RoomInterface
{
	public interface ILightingRoomInterfaceDeviceSettings : IDeviceSettings
	{

		/// <summary>
		/// If true, an occupancy sensor control will be added to the device
		/// Used for systems where occupancy sensors may not be  used,
		/// to prevent things like adding unnecessary Fusion Occupancy Sensor Assets.
		/// </summary>
		bool EnableOccupancyControl { get; set; }
	}
}