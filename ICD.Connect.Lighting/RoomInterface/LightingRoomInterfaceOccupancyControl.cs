using ICD.Common.Utils.EventArguments;
using ICD.Connect.Partitioning.Commercial.Controls.Occupancy;

namespace ICD.Connect.Lighting.RoomInterface
{
	public sealed class LightingRoomInterfaceOccupancyControl : AbstractOccupancySensorControl<ILightingRoomInterfaceDevice>
	{

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public LightingRoomInterfaceOccupancyControl(ILightingRoomInterfaceDevice parent, int id) : base(parent, id)
		{
			SupportedFeatures = eOccupancyFeatures.Occupancy;
			OccupancyState = parent.GetOccupancy();
		}

		/// <summary>
		/// Subscribe to the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected override void Subscribe(ILightingRoomInterfaceDevice parent)
		{
			base.Subscribe(parent);

			if (parent == null)
				return;

			parent.OnOccupancyChanged += ParentOnOccupancyChanged;
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected override void Unsubscribe(ILightingRoomInterfaceDevice parent)
		{
			base.Unsubscribe(parent);

			if (parent == null)
				return;

			parent.OnOccupancyChanged -= ParentOnOccupancyChanged;
		}

		private void ParentOnOccupancyChanged(object sender, GenericEventArgs<eOccupancyState> args)
		{
			OccupancyState = args.Data;
		}
	}
}