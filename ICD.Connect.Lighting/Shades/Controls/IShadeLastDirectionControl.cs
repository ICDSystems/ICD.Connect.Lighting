using System;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Devices.Controls;

namespace ICD.Connect.Lighting.Shades.Controls
{
	public interface IShadeLastDirectionControl : IDeviceControl
	{
		event EventHandler<GenericEventArgs<eShadeDirection>> OnLastDirectionChanged;
			
		eShadeDirection LastDirection { get; }
	}
}