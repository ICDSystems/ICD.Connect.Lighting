using System;
using ICD.Common.Utils.EventArguments;

namespace ICD.Connect.Lighting.Shades.Controls
{
	public interface IShadeWithLastDirectionFeedback : IShadeDevice
	{
		/// <summary>
		/// Raised when the last direction changes
		/// </summary>
		event EventHandler<GenericEventArgs<eShadeDirection>> OnLastDirectionChanged;
		
		/// <summary>
		/// Last direction the shade moved
		/// </summary>
		eShadeDirection LastDirection { get; }
	}
}