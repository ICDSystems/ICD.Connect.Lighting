using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;

namespace ICD.Connect.Lighting.Lutron.Nwk.EventArguments
{
	public enum eOccupancyState
	{
		[PublicAPI] Unknown = 1,
		[PublicAPI] Inactive = 2,
		[PublicAPI] Occupied = 3,
		[PublicAPI] Unoccupied = 4
	}

	public sealed class OccupancyStateEventArgs : GenericEventArgs<eOccupancyState>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="data"></param>
		public OccupancyStateEventArgs(eOccupancyState data)
			: base(data)
		{
		}
	}
}
