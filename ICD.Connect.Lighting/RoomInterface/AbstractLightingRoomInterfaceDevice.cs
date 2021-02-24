using System;
using System.Collections.Generic;
using System.Text;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Lighting.EventArguments;
using ICD.Connect.Partitioning.Commercial.Controls.Occupancy;
using ICD.Connect.Settings;

namespace ICD.Connect.Lighting.RoomInterface
{
	public abstract class AbstractLightingRoomInterfaceDevice<T> : AbstractDevice<T>, ILightingRoomInterfaceDevice
		where T : ILightingRoomInterfaceDeviceSettings, new()
	{
		private const int OCCUPANCY_CONTROL_ID = 0;

		#region Settings

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(T settings)
		{
			base.CopySettingsFinal(settings);

			settings.EnableOccupancyControl = Controls.Contains(OCCUPANCY_CONTROL_ID);
		}

		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(T settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			if (settings.EnableOccupancyControl)
				addControl(new LightingRoomInterfaceOccupancyControl(this, OCCUPANCY_CONTROL_ID));
		}

		#endregion

		#region ILightingRoomInterface

		public abstract event EventHandler<GenericEventArgs<eOccupancyState>> OnOccupancyChanged;
		public abstract event EventHandler<GenericEventArgs<int?>> OnPresetChanged;
		public abstract event EventHandler<LoadLevelEventArgs> OnLoadLevelChanged;
		public abstract event EventHandler OnControlsChanged;

		/// <summary>
		/// Gets the available light loads for the room.
		/// </summary>
		/// <returns></returns>
		public abstract IEnumerable<LightingProcessorControl> GetLoads();

		/// <summary>
		/// Gets the available individual shades for the room.
		/// </summary>
		/// <returns></returns>
		public abstract IEnumerable<LightingProcessorControl> GetShades();

		/// <summary>
		/// Gets the available shade groups for the room.
		/// </summary>
		/// <returns></returns>
		public abstract IEnumerable<LightingProcessorControl> GetShadeGroups();

		/// <summary>
		/// Gets the available presets for the room.
		/// </summary>
		/// <returns></returns>
		public abstract IEnumerable<LightingProcessorControl> GetPresets();

		/// <summary>
		/// Gets the current occupancy state for the given room.
		/// </summary>
		/// <returns></returns>
		public abstract eOccupancyState GetOccupancy();

		/// <summary>
		/// Sets the preset for the given room.
		/// </summary>
		/// <param name="preset"></param>
		public abstract void SetPreset(int? preset);

		/// <summary>
		/// Gets the current preset for the given room.
		/// </summary>
		public abstract int? GetPreset();

		/// <summary>
		/// Sets the lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		/// <param name="percentage"></param>
		public abstract void SetLoadLevel(int load, float percentage);

		/// <summary>
		/// Gets the current lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		public abstract float GetLoadLevel(int load);

		/// <summary>
		/// Starts raising the lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		public abstract void StartRaisingLoadLevel(int load);

		/// <summary>
		/// Starts lowering the lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		public abstract void StartLoweringLoadLevel(int load);

		/// <summary>
		/// Stops raising/lowering the lighting level for the given load.
		/// </summary>
		/// <param name="load"></param>
		public abstract void StopRampingLoadLevel(int load);

		/// <summary>
		/// Starts raising the shade.
		/// </summary>
		/// <param name="shade"></param>
		public abstract void StartRaisingShade(int shade);

		/// <summary>
		/// Starts lowering the shade.
		/// </summary>
		/// <param name="shade"></param>
		public abstract void StartLoweringShade(int shade);

		/// <summary>
		/// Stops moving the shade.
		/// </summary>
		/// <param name="shade"></param>
		public abstract void StopMovingShade(int shade);

		/// <summary>
		/// Starts raising the shade group.
		/// </summary>
		/// <param name="shadeGroup"></param>
		public abstract void StartRaisingShadeGroup(int shadeGroup);

		/// <summary>
		/// Starts lowering the shade group.
		/// </summary>
		/// <param name="shadeGroup"></param>
		public abstract void StartLoweringShadeGroup(int shadeGroup);

		/// <summary>
		/// Stops moving the shade group.
		/// </summary>
		/// <param name="shadeGroup"></param>
		public abstract void StopMovingShadeGroup(int shadeGroup);

		#endregion

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			addRow("Preset", GetPreset());
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new ConsoleCommand("PrintPresets", "Prints all presets for the room", () => PrintPresets());
			yield return new GenericConsoleCommand<int>("RecallPreset", "RecallPreset <int>", preset => SetPreset(preset));
		}

		private string PrintPresets()
		{
			StringBuilder response = new StringBuilder();
			response.AppendLine("Presets");
			foreach (var preset in GetPresets())
			{
				response.AppendFormat("{0} - {1}", preset.Id, preset.Name);
				response.AppendLine();
			}
			
			return response.ToString();
		}

		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		#endregion
	}
}