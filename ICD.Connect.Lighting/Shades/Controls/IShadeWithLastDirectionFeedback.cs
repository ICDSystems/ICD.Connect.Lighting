using System;

namespace ICD.Connect.Lighting.Shades.Controls
{
	public interface IShadeWithLastDirectionFeedback : IShadeDevice
	{
		event EventHandler OnDirectionChanged;

		eShadeDirection GetLastDirection();
	}
}