using System.Collections.Generic;
using ICD.Common.Utils;
using ICD.Connect.Lighting.Environment;
using ICD.Connect.Misc.Occupancy;

namespace ICD.Connect.Lighting.Processors
{
	public sealed class LightingProcessorDevice : AbstractLightingProcessorDevice<LightingProcessorDeviceSettings>
	{
		#region Private Members
		private readonly Dictionary<int, IEnvironmentRoom> m_Rooms;

		private readonly SafeCriticalSection m_CacheSection;
		#endregion

		public LightingProcessorDevice()
		{
			m_Rooms = new Dictionary<int, IEnvironmentRoom>();
			m_CacheSection = new SafeCriticalSection();
		}

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return true;
		}

		#region Room
		/// <summary>
		/// Gets the available rooms.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<int> GetRooms()
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Returns true if the given room is available.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public override bool ContainsRoom(int room)
		{
			throw new System.NotImplementedException();
		}
		#endregion

		#region Occupancy
		/// <summary>
		/// Gets the current occupancy state for the given room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public override eOccupancyState GetOccupancyForRoom(int room)
		{
			throw new System.NotImplementedException();
		}
		#endregion

		#region Preset
		/// <summary>
		/// Gets the available presets for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public override IEnumerable<LightingProcessorControl> GetPresetsForRoom(int room)
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Sets the preset for the given room.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="preset"></param>
		public override void SetPresetForRoom(int room, int? preset)
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Gets the current preset for the given room.
		/// </summary>
		/// <param name="room"></param>
		public override int? GetPresetForRoom(int room)
		{
			throw new System.NotImplementedException();
		}
		#endregion

		#region Load
		/// <summary>
		/// Gets the available light loads for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public override IEnumerable<LightingProcessorControl> GetLoadsForRoom(int room)
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Sets the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		/// <param name="percentage"></param>
		public override void SetLoadLevel(int room, int load, float percentage)
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Gets the current lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		public override float GetLoadLevel(int room, int load)
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Starts raising the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		public override void StartRaisingLoadLevel(int room, int load)
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Starts lowering the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		public override void StartLoweringLoadLevel(int room, int load)
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Stops raising/lowering the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		public override void StopRampingLoadLevel(int room, int load)
		{
			throw new System.NotImplementedException();
		}

		#endregion

		#region Shade
		/// <summary>
		/// Gets the available individual shades for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public override IEnumerable<LightingProcessorControl> GetShadesForRoom(int room)
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Starts raising the shade.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shade"></param>
		public override void StartRaisingShade(int room, int shade)
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Starts lowering the shade.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shade"></param>
		public override void StartLoweringShade(int room, int shade)
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Stops moving the shade.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shade"></param>
		public override void StopMovingShade(int room, int shade)
		{
			throw new System.NotImplementedException();
		}
		#endregion

		#region Shade Group
		/// <summary>
		/// Gets the available shade groups for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public override IEnumerable<LightingProcessorControl> GetShadeGroupsForRoom(int room)
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Starts raising the shade group.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shadeGroup"></param>
		public override void StartRaisingShadeGroup(int room, int shadeGroup)
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Starts lowering the shade group.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shadeGroup"></param>
		public override void StartLoweringShadeGroup(int room, int shadeGroup)
		{
			throw new System.NotImplementedException();
		}

		/// <summary>
		/// Stops moving the shade group.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shadeGroup"></param>
		public override void StopMovingShadeGroup(int room, int shadeGroup)
		{
			throw new System.NotImplementedException();
		}
		#endregion
	}
}