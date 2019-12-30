using ICD.Connect.Devices;
using ICD.Connect.Protocol.Network.Settings;
using ICD.Connect.Protocol.Settings;

namespace ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice
{
	public interface ILutronNwkDeviceSettings : IDeviceSettings, ISecureNetworkSettings, IComSpecSettings
	{
		string IntegrationConfig { get; set; }
		int? Port { get; set; }
		string Username { get; set; }
	}
}
