using System;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;

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

	public static class OccupancyStateEventArgsExtensions
	{
		/// <summary>
		/// Raises the event safely. Simply skips if the handler is null.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="sender"></param>
		/// <param name="data"></param>
		public static void Raise([CanBeNull] this EventHandler<OccupancyStateEventArgs> extends, object sender, eOccupancyState data)
		{
			extends.Raise(sender, new OccupancyStateEventArgs(data));
		}
	}
}
