﻿using System;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Lighting.Processors;

namespace ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice
{
	public interface ILutronNwkDevice: ILightingProcessorDevice
	{

		/// <summary>
		/// Raised when the class initializes.
		/// </summary>
		event EventHandler<BoolEventArgs> OnInitializedChanged;

		/// <summary>
		/// Registers a callback for handling feedback from the lighting processor.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="callback"></param>
		void RegisterIntegrationCallback(string key, LutronNwkDevice.ParserCallback callback);

		/// <summary>
		/// Unregisters a callback delegate.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="callback"></param>
		void UnregisterIntegrationCallback(string key, LutronNwkDevice.ParserCallback callback);

		/// <summary>
		/// Enqueue the data string to be sent to the device.
		/// </summary>
		/// <param name="data"></param>
		void EnqueueData(string data);
	}
}
