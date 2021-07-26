using System;
using ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice;
using ICD.Connect.Protocol.Ports.ComPort;
using ICD.Connect.Protocol.Settings;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Lighting.Lutron.Nwk.Devices.QuantumNwk
{
	[KrangSettings("LutronQuantumNwk", typeof(LutronQuantumNwkDevice))]
	public sealed class LutronQuantumNwkDeviceSettings : AbstractLutronNwkDeviceSettings
	{
		/// <summary>
		/// Sets the default comspec values if no other values have been set
		/// </summary>
		/// <remarks>Called from AbstractLutronNwkDeviceSettings constructor</remarks>
		/// <param name="comSpecProperties"></param>
		protected override void UpdateComSpecDefaults(ComSpecProperties comSpecProperties)
		{
			if (comSpecProperties == null)
				throw new ArgumentNullException("comSpecProperties");

			comSpecProperties.ApplyDefaultValues(eComBaudRates.BaudRate115200,
												 eComDataBits.DataBits8,
												 eComParityType.None,
												 eComStopBits.StopBits1,
												 eComProtocolType.Rs232,
												 eComHardwareHandshakeType.None,
												 eComSoftwareHandshakeType.None,
												 false);
		}


	}
}
