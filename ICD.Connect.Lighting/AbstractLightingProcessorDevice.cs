using System;
using System.Collections.Generic;
using ICD.Common.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Commands;
using ICD.Connect.Devices;
using ICD.Connect.Lighting.EventArguments;

namespace ICD.Connect.Lighting
{
	/// <summary>
	/// Base class for lighting processor devices.
	/// </summary>
	/// <typeparam name="TSettings"></typeparam>
	public abstract class AbstractLightingProcessorDevice<TSettings> : AbstractDevice<TSettings>, ILightingProcessorDevice
		where TSettings : AbstractLightingProcessorDeviceSettings, new()
	{
		/// <summary>
		/// Raised when a room occupancy state changes.
		/// </summary>
		public event EventHandler<RoomOccupancyEventArgs> OnRoomOccupancyChanged;

		/// <summary>
		/// Raised when a room preset changes.
		/// </summary>
		public event EventHandler<RoomPresetChangeEventArgs> OnRoomPresetChanged;

		/// <summary>
		/// Raised when a lighting load level changes.
		/// </summary>
		public event EventHandler<RoomLoadLevelEventArgs> OnRoomLoadLevelChanged;

		/// <summary>
		/// Raised when a room controls change.
		/// </summary>
		public event EventHandler<IntEventArgs> OnRoomControlsChanged;

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			OnRoomOccupancyChanged = null;
			OnRoomPresetChanged = null;
			OnRoomLoadLevelChanged = null;

			base.DisposeFinal(disposing);
		}

		#region LightingProcessorDevice Methods

		/// <summary>
		/// Gets the available rooms.
		/// </summary>
		/// <returns></returns>
		public abstract IEnumerable<int> GetRooms();

		/// <summary>
		/// Returns true if the given room is available.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public abstract bool ContainsRoom(int room);

		/// <summary>
		/// Gets the available light loads for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public abstract IEnumerable<LightingProcessorControl> GetLoadsForRoom(int room);

		/// <summary>
		/// Gets the available individual shades for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public abstract IEnumerable<LightingProcessorControl> GetShadesForRoom(int room);

		/// <summary>
		/// Gets the available shade groups for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public abstract IEnumerable<LightingProcessorControl> GetShadeGroupsForRoom(int room);

		/// <summary>
		/// Gets the available presets for the room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public abstract IEnumerable<LightingProcessorControl> GetPresetsForRoom(int room);

		/// <summary>
		/// Gets the current occupancy state for the given room.
		/// </summary>
		/// <param name="room"></param>
		/// <returns></returns>
		public abstract RoomOccupancyEventArgs.eOccupancyState GetOccupancyForRoom(int room);

		/// <summary>
		/// Sets the preset for the given room.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="preset"></param>
		public abstract void SetPresetForRoom(int room, int? preset);

		/// <summary>
		/// Gets the current preset for the given room.
		/// </summary>
		/// <param name="room"></param>
		public abstract int? GetPresetForRoom(int room);

		/// <summary>
		/// Sets the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		/// <param name="percentage"></param>
		public abstract void SetLoadLevel(int room, int load, float percentage);

		/// <summary>
		/// Gets the current lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		public abstract float GetLoadLevel(int room, int load);

		/// <summary>
		/// Starts raising the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		public abstract void StartRaisingLoadLevel(int room, int load);

		/// <summary>
		/// Starts lowering the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		public abstract void StartLoweringLoadLevel(int room, int load);

		/// <summary>
		/// Stops raising/lowering the lighting level for the given load.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		public abstract void StopRampingLoadLevel(int room, int load);

		/// <summary>
		/// Starts raising the shade.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shade"></param>
		public abstract void StartRaisingShade(int room, int shade);

		/// <summary>
		/// Starts lowering the shade.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shade"></param>
		public abstract void StartLoweringShade(int room, int shade);

		/// <summary>
		/// Stops moving the shade.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shade"></param>
		public abstract void StopMovingShade(int room, int shade);

		/// <summary>
		/// Starts raising the shade group.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shadeGroup"></param>
		public abstract void StartRaisingShadeGroup(int room, int shadeGroup);

		/// <summary>
		/// Starts lowering the shade group.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shadeGroup"></param>
		public abstract void StartLoweringShadeGroup(int room, int shadeGroup);

		/// <summary>
		/// Stops moving the shade group.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="shadeGroup"></param>
		public abstract void StopMovingShadeGroup(int room, int shadeGroup);

		#endregion

		#region Protected Methods

		/// <summary>
		/// Raises the OnRoomOccupancyChanged event.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="occupancyState"></param>
		protected void RaiseOnRoomOccupancyChanged(int room, RoomOccupancyEventArgs.eOccupancyState occupancyState)
		{
			OnRoomOccupancyChanged.Raise(this, new RoomOccupancyEventArgs(room, occupancyState));
		}

		/// <summary>
		/// Raises the OnRoomPresetChanged event.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="preset"></param>
		protected void RaiseOnRoomPresetChanged(int room, int? preset)
		{
			OnRoomPresetChanged.Raise(this, new RoomPresetChangeEventArgs(room, preset));
		}

		/// <summary>
		/// Raises the OnRoomLoadLevelChanged event.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="load"></param>
		/// <param name="percentage"></param>
		protected void RaiseOnRoomLoadLevelChanged(int room, int load, float percentage)
		{
			OnRoomLoadLevelChanged.Raise(this, new RoomLoadLevelEventArgs(room, load, percentage));
		}

		/// <summary>
		/// Raises the OnRoomControlsChanged event.
		/// </summary>
		/// <param name="room"></param>
		protected void RaiseOnRoomControlsChanged(int room)
		{
			OnRoomControlsChanged.Raise(this, new IntEventArgs(room));
		}

		#endregion

		#region Console

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return
				new GenericConsoleCommand<int, int>("SetRoomPreset", "SetRoomPreset <ROOM> <PRESET>",
				                                    (a, b) => ConsoleSetPresetForRoom(a, b));
			yield return
				new GenericConsoleCommand<int>("ClearRoomPreset", "SetRoomPreset <ROOM>", a => ConsoleClearPresetForRoom(a));

			yield return new GenericConsoleCommand<int, int, float>("SetLoadLevel", "SetLoadLevel <ROOM> <LOAD> <PERCENTAGE>",
			                                                        (a, b, c) => SetLoadLevel(a, b, c));

			yield return new GenericConsoleCommand<int, int>("StartRaisingShade", "StartRaisingShade <ROOM> <SHADE>",
			                                                 (a, b) => StartRaisingShade(a, b));
			yield return new GenericConsoleCommand<int, int>("StartLoweringShade", "StartLoweringShade <ROOM> <SHADE>",
			                                                 (a, b) => StartLoweringShade(a, b));
			yield return new GenericConsoleCommand<int, int>("StopMovingShade", "StopMovingShade <ROOM> <SHADE>",
			                                                 (a, b) => StopMovingShade(a, b));

			yield return new GenericConsoleCommand<int, int>("StartRaisingShadeGroup", "StartRaisingShadeGroup <ROOM> <SHADE>",
			                                                 (a, b) => StartRaisingShadeGroup(a, b));
			yield return new GenericConsoleCommand<int, int>("StartLoweringShadeGroup", "StartLoweringShadeGroup <ROOM> <SHADE>",
			                                                 (a, b) => StartLoweringShadeGroup(a, b));
			yield return new GenericConsoleCommand<int, int>("StopMovingShadeGroup", "StopMovingShadeGroup <ROOM> <SHADE>",
			                                                 (a, b) => StopMovingShadeGroup(a, b));
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
		/// Sets the room preset, called via console command.
		/// </summary>
		/// <param name="room"></param>
		/// <param name="preset"></param>
		private void ConsoleSetPresetForRoom(int room, int preset)
		{
			SetPresetForRoom(room, preset);
		}

		/// <summary>
		/// Clears the room preset, called via console command.
		/// </summary>
		/// <param name="room"></param>
		private void ConsoleClearPresetForRoom(int room)
		{
			SetPresetForRoom(room, null);
		}

		#endregion
	}
}
