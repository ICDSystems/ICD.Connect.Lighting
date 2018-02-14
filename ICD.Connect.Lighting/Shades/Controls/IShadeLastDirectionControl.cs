using System;
using ICD.Connect.Devices.Controls;

namespace ICD.Connect.Lighting.Shades.Controls
{
	public interface IShadeLastDirectionControl : IDeviceControl
	{
		event EventHandler OnDirectionChanged;
			
		eShadeDirection GetLastDirection();                    
	}
}