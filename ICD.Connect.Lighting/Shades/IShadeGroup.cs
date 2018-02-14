using System.Collections.Generic;

namespace ICD.Connect.Lighting.Shades
{
	public interface IShadeGroup : IShadeDevice
	{
		IEnumerable<IShadeDevice> Shades { get; }
	}
}