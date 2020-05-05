using ICD.Connect.Lighting.EventArguments;
using ICD.Connect.Misc.Occupancy;

namespace ICD.Connect.Lighting.Server
{
	public sealed class LightingProcessorClientOccupancyControl : AbstractOccupancySensorControl<LightingProcessorClientDevice>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public LightingProcessorClientOccupancyControl(LightingProcessorClientDevice parent, int id) : base(parent, id)
		{
			Subscribe(parent);
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			Unsubscribe(Parent);
		}

		private void Subscribe(LightingProcessorClientDevice parent)
		{
			if (parent == null)
				return;

			parent.OnRoomOccupancyChanged += ParentOnOnRoomOccupancyChanged;
		}

		private void Unsubscribe(LightingProcessorClientDevice parent)
		{
			if (parent == null)
				return;

			parent.OnRoomOccupancyChanged -= ParentOnOnRoomOccupancyChanged;
		}

		private void ParentOnOnRoomOccupancyChanged(object sender, RoomOccupancyEventArgs e)
		{
			OccupancyState = e.OccupancyState;
		}
	}
}
