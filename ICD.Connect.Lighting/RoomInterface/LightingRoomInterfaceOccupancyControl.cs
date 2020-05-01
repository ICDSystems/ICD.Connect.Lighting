using System;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Misc.Occupancy;

namespace ICD.Connect.Lighting.RoomInterface
{
	public sealed class LightingRoomInterfaceOccupancyControl : AbstractDeviceControl<ILightingRoomInterfaceDevice>, IOccupancySensorControl
	{

		private eOccupancyState m_OccupancyState;

		public event EventHandler<GenericEventArgs<eOccupancyState>> OnOccupancyStateChanged;

		/// <summary>
		/// State of the occupancy sensor
		/// True = occupied
		/// False = unoccupied/vacant
		/// </summary>
		public eOccupancyState OccupancyState
		{
			get { return m_OccupancyState; }
			private set
			{
				if (m_OccupancyState == value)
					return;

				m_OccupancyState = value;

				OnOccupancyStateChanged.Raise(this, new GenericEventArgs<eOccupancyState>(value));
			}
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public LightingRoomInterfaceOccupancyControl(ILightingRoomInterfaceDevice parent, int id) : base(parent, id)
		{
			OccupancyState = parent.GetOccupancy();
		}

		protected override void Subscribe(ILightingRoomInterfaceDevice parent)
		{
			if (parent == null)
				return;

			parent.OnOccupancyChanged += ParentOnOccupancyChanged;
		}

		private void ParentOnOccupancyChanged(object sender, GenericEventArgs<eOccupancyState> args)
		{
			OccupancyState = args.Data;
		}

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			addRow("Occupancy State", OccupancyState);
		}

		#endregion
	}
}