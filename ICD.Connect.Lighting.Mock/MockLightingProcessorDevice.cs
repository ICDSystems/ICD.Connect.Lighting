using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Lighting.Environment;
using ICD.Connect.Lighting.EventArguments;
using ICD.Connect.Lighting.Mock.Controls;
using ICD.Connect.Lighting.Processors;
using ICD.Connect.Misc.Occupancy;

namespace ICD.Connect.Lighting.Mock
{
	/// <summary>
	/// Configurable ILightingProcessorDevice for testing lighting programs.
	/// </summary>
	public sealed class MockLightingProcessorDevice : AbstractLightingProcessorDevice<MockLightingProcessorDeviceSettings>
	{
		private readonly List<int> m_RoomIdsOrdered;
		private readonly Dictionary<int, MockLightingRoom> m_IdToRooms;
		private readonly SafeCriticalSection m_CacheSection;

		/// <summary>
		/// Constructor.
		/// </summary>
		public MockLightingProcessorDevice()
		{
			m_RoomIdsOrdered = new List<int>();
			m_IdToRooms = new Dictionary<int, MockLightingRoom>();
			m_CacheSection = new SafeCriticalSection();
		}

		#region Methods

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			Clear();
		}

		/// <summary>
		/// Clears the control collections.
		/// </summary>
		[PublicAPI]
		public void Clear()
		{
			m_CacheSection.Enter();

			try
			{
				foreach (MockLightingRoom room in m_IdToRooms.Values)
				{
					Unsubscribe(room);
					room.Dispose();
				}

				m_RoomIdsOrdered.Clear();
				m_IdToRooms.Clear();
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Adds the room to the collection.
		/// </summary>
		/// <param name="id"></param>
		[PublicAPI]
		public void AddRoom(int id)
		{
			m_CacheSection.Enter();

			try
			{
				if (m_IdToRooms.ContainsKey(id))
					return;

				MockLightingRoom room = new MockLightingRoom(id);

				m_RoomIdsOrdered.Add(id);
				m_IdToRooms[id] = room;

				Subscribe(room);
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Adds the load to the collection.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="room"></param>
		/// <param name="name"></param>
		[PublicAPI]
		public void AddLoad(int id, int room, string name)
		{
			MockLightingLoadControl load = new MockLightingLoadControl(id, room, name);
			GetRoom(room).AddLoad(load);
		}

		/// <summary>
		/// Adds the shade to the collection.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="room"></param>
		/// <param name="name"></param>
		[PublicAPI]
		public void AddShade(int id, int room, string name)
		{
			MockLightingShadeControl shade = new MockLightingShadeControl(id, room, name);
			GetRoom(room).AddShade(shade);
		}

		/// <summary>
		/// Adds the shade group to the collection.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="room"></param>
		/// <param name="name"></param>
		[PublicAPI]
		public void AddShadeGroup(int id, int room, string name)
		{
			MockLightingShadeGroupControl shadeGroup = new MockLightingShadeGroupControl(id, room, name);
			GetRoom(room).AddShadeGroup(shadeGroup);
		}

		/// <summary>
		/// Adds the preset to the collection.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="room"></param>
		/// <param name="name"></param>
		[PublicAPI]
		public void AddPreset(int id, int room, string name)
		{
			MockLightingPresetControl preset = new MockLightingPresetControl(id, room, name);
			GetRoom(room).AddPreset(preset);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the room with the given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		private MockLightingRoom GetRoom(int id)
		{
			return m_CacheSection.Execute(() => m_IdToRooms[id]);
		}

		/// <summary>
		/// Gets the load with the given id.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		/// <returns></returns>
		private ILightingLoadEnvironmentPeripheral GetLoad(int room, int load)
		{
			return GetRoom(room).GetLoad(load);
		}

		/// <summary>
		/// Gets the shade with the given id.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shade"></param>
		/// <returns></returns>
		private IShadeEnvironmentPeripheral GetShade(int room, int shade)
		{
			return GetRoom(room).GetShade(shade);
		}

		/// <summary>
		/// Gets the shade group with the given id.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shadeGroup"></param>
		/// <returns></returns>
		private IShadeEnvironmentPeripheral GetShadeGroup(int room, int shadeGroup)
		{
			return GetRoom(room).GetShadeGroup(shadeGroup);
		}

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return true;
		}

		#endregion

		#region Room Callbacks

		/// <summary>
		/// Subscribe to the room events.
		/// </summary>
		/// <param name="room"></param>
		private void Subscribe(MockLightingRoom room)
		{
			if (room == null)
				return;

			room.OnActivePresetChanged += RoomOnActivePresetChanged;
			room.OnLoadLevelChanged += RoomOnLoadLevelChanged;
			room.OnOccupancyChanged += RoomOnOccupancyChanged;
			room.OnControlsChanged += RoomOnControlsChanged;
		}

		/// <summary>
		/// Unsubscribe from the room events.
		/// </summary>
		/// <param name="room"></param>
		private void Unsubscribe(MockLightingRoom room)
		{
			if (room == null)
				return;

			room.OnActivePresetChanged -= RoomOnActivePresetChanged;
			room.OnLoadLevelChanged -= RoomOnLoadLevelChanged;
			room.OnOccupancyChanged -= RoomOnOccupancyChanged;
			room.OnControlsChanged -= RoomOnControlsChanged;
		}

		/// <summary>
		/// Called when the room occupancy changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void RoomOnOccupancyChanged(object sender, RoomOccupancyEventArgs args)
		{
			RaiseOnRoomOccupancyChanged(args.RoomId, args.OccupancyState);
		}

		/// <summary>
		/// Called when the room load level changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void RoomOnLoadLevelChanged(object sender, RoomLoadLevelEventArgs args)
		{
			RaiseOnRoomLoadLevelChanged(args.RoomId, args.LoadId, args.Percentage);
		}

		/// <summary>
		/// Called when the room preset changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void RoomOnActivePresetChanged(object sender, RoomPresetChangeEventArgs args)
		{
			RaiseOnRoomPresetChanged(args.RoomId, args.Preset);
		}

		private void RoomOnControlsChanged(object sender, EventArgs eventArgs)
		{
			MockLightingRoom room = sender as MockLightingRoom;
			if (room == null)
				return;

			RaiseOnRoomControlsChanged(room.Id);
		}

		#endregion

		#region Lighting Processor Methods

		#region Controls

		/// <summary>
		/// Gets the available rooms.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<int> GetRooms()
		{
			return m_CacheSection.Execute(() => m_RoomIdsOrdered.ToArray());
		}

		/// <summary>
		/// Returns true if the given room is available.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public override bool ContainsRoom(int room)
		{
			return m_CacheSection.Execute(() => m_IdToRooms.ContainsKey(room));
		}

		/// <summary>
		/// Gets the available light loads for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public override IEnumerable<LightingProcessorControl> GetLoadsForRoom(int room)
		{
			return GetRoom(room).GetLoads();
		}

		/// <summary>
		/// Gets the available individual shades for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public override IEnumerable<LightingProcessorControl> GetShadesForRoom(int room)
		{
			return GetRoom(room).GetShades();
		}

		/// <summary>
		/// Gets the available shade groups for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public override IEnumerable<LightingProcessorControl> GetShadeGroupsForRoom(int room)
		{
			return GetRoom(room).GetShadeGroups();
		}

		/// <summary>
		/// Gets the available presets for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public override IEnumerable<LightingProcessorControl> GetPresetsForRoom(int room)
		{
			return GetRoom(room).GetPresets();
		}

		#endregion

		#region Rooms

		/// <summary>
		/// Set the current occupancy for the given room.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="occupancy"></param>
		[PublicAPI]
		public void SetOccupancyForRoom(int room, eOccupancyState occupancy)
		{
			GetRoom(room).Occupancy = occupancy;
		}

		/// <summary>
		/// Gets the current occupancy state for the given room.
		/// Defaults to Unknown if not set.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public override eOccupancyState GetOccupancyForRoom(int room)
		{
			return GetRoom(room).Occupancy;
		}

		/// <summary>
		/// Sets the preset for the given room.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="preset"></param>
		public override void SetPresetForRoom(int room, int? preset)
		{
			GetRoom(room).ActivePreset = preset;
		}

		/// <summary>
		/// Gets the current preset for the given room.
		/// Returns zero if not set.
		/// </summary>
		/// <param name="room"></param>
		public override int? GetPresetForRoom(int room)
		{
			return GetRoom(room).ActivePreset;
		}

		#endregion

		#region Loads

		/// <summary>
		/// Sets the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		/// <param name="percentage"></param>
		public override void SetLoadLevel(int room, int load, float percentage)
		{
			GetRoom(room).GetLoad(load).LoadLevel = percentage;
		}

		/// <summary>
		/// Gets the current lighting level for the given load.
		/// Returns 0 if not set.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		public override float GetLoadLevel(int room, int load)
		{
			return GetRoom(room).GetLoad(load).LoadLevel;
		}

		/// <summary>
		/// Starts raising the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		public override void StartRaisingLoadLevel(int room, int load)
		{
			GetLoad(room, load).StartRaising();
		}

		/// <summary>
		/// Starts lowering the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		public override void StartLoweringLoadLevel(int room, int load)
		{
			GetLoad(room, load).StartLowering();
		}

		/// <summary>
		/// Stops raising/lowering the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		public override void StopRampingLoadLevel(int room, int load)
		{
			GetLoad(room, load).StopRamping();
		}

		#endregion

		#region Shades

		/// <summary>
		/// Starts raising the shade.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shade"></param>
		public override void StartRaisingShade(int room, int shade)
		{
			GetShade(room, shade).StartRaising();
		}

		/// <summary>
		/// Starts lowering the shade.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shade"></param>
		public override void StartLoweringShade(int room, int shade)
		{
			GetShade(room, shade).StartLowering();
		}

		/// <summary>
		/// Stops moving the shade.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shade"></param>
		public override void StopMovingShade(int room, int shade)
		{
			GetShade(room, shade).StopMoving();
		}

		#endregion

		#region Shade Groups

		/// <summary>
		/// Starts raising the shade group.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shadeGroup"></param>
		public override void StartRaisingShadeGroup(int room, int shadeGroup)
		{
			GetShadeGroup(room, shadeGroup).StartRaising();
		}

		/// <summary>
		/// Starts lowering the shade group.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shadeGroup"></param>
		public override void StartLoweringShadeGroup(int room, int shadeGroup)
		{
			GetShadeGroup(room, shadeGroup).StartLowering();
		}

		/// <summary>
		/// Stops moving the shade group.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shadeGroup"></param>
		public override void StopMovingShadeGroup(int room, int shadeGroup)
		{
			GetShadeGroup(room, shadeGroup).StopMoving();
		}

		#endregion

		#endregion

		#region Console

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			foreach (IConsoleNodeBase node in GetBaseConsoleNodes())
				yield return node;

			MockLightingRoom[] rooms = m_CacheSection.Execute(() => m_IdToRooms.Values.ToArray());
			yield return ConsoleNodeGroup.KeyNodeMap("Rooms", rooms, room => (uint)room.Id);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new GenericConsoleCommand<int>("AddRoom", "AddRoom <ROOM>", a => AddRoom(a));

			string help = string.Format("SetRoomOccupancy <ROOM> <OCCUPANCY {0}>",
			                            StringUtils.ArrayFormat(EnumUtils.GetValues<eOccupancyState>()));
			yield return
				new GenericConsoleCommand<int, eOccupancyState>("SetRoomOccupancy", help,
				                                                                       (a, b) => SetOccupancyForRoom(a, b));
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleNodeBase> GetBaseConsoleNodes()
		{
			return base.GetConsoleNodes();
		}

		#endregion
	}
}
