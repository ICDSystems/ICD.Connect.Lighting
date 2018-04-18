using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Lighting.Environment;
using ICD.Connect.Lighting.EventArguments;
using ICD.Connect.Misc.Occupancy;

namespace ICD.Connect.Lighting.Mock.Controls
{
	public sealed class MockLightingRoom : IConsoleNode, IDisposable, IEnvironmentRoom
	{
		public event EventHandler<RoomPresetChangeEventArgs> OnActivePresetChanged;
		public event EventHandler<RoomOccupancyEventArgs> OnOccupancyChanged;
		public event EventHandler<RoomLoadLevelEventArgs> OnLoadLevelChanged;
		public event EventHandler OnControlsChanged;

		private readonly List<int> m_Loads;
		private readonly List<int> m_Presets;
		private readonly List<int> m_Shades;
		private readonly List<int> m_ShadeGroups;

		private readonly Dictionary<int, ILightingLoadEnvironmentPeripheral> m_IdToLoad;
		private readonly Dictionary<int, IPresetEnvironmentPeripheral> m_IdToPreset;
		private readonly Dictionary<int, IShadeEnvironmentPeripheral> m_IdToShade;
		private readonly Dictionary<int, IShadeEnvironmentPeripheral> m_IdToShadeGroup;

		private readonly SafeCriticalSection m_CacheSection;

		private readonly int m_Id;
		private int? m_ActivePreset;
		private eOccupancyState m_Occupancy;

		#region Properties

		/// <summary>
		/// Gets the room id.
		/// </summary>
		public int Id { get { return m_Id; } }

		/// <summary>
		/// Gets/sets the active preset.
		/// </summary>
		public int? ActivePreset
		{
			get { return m_ActivePreset; }
			set
			{
				if (value == m_ActivePreset)
					return;

				m_ActivePreset = value;

				OnActivePresetChanged.Raise(this, new RoomPresetChangeEventArgs(Id, value));
			}
		}

		/// <summary>
		/// Gets/sets the room occupancy state.
		/// </summary>
		public eOccupancyState Occupancy
		{
			get { return m_Occupancy; }
			set
			{
				if (value == m_Occupancy)
					return;

				m_Occupancy = value;

				OnOccupancyChanged.Raise(this, new RoomOccupancyEventArgs(Id, value));
			}
		}

		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		public string ConsoleName { get { return GetType().Name; } }

		/// <summary>
		/// Gets the help information for the node.
		/// </summary>
		public string ConsoleHelp { get { return string.Empty; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="id"></param>
		public MockLightingRoom(int id)
		{
			m_Loads = new List<int>();
			m_Presets = new List<int>();
			m_Shades = new List<int>();
			m_ShadeGroups = new List<int>();

			m_IdToLoad = new Dictionary<int, ILightingLoadEnvironmentPeripheral>();
			m_IdToPreset = new Dictionary<int, IPresetEnvironmentPeripheral>();
			m_IdToShade = new Dictionary<int, IShadeEnvironmentPeripheral>();
			m_IdToShadeGroup = new Dictionary<int, IShadeEnvironmentPeripheral>();
			m_CacheSection = new SafeCriticalSection();

			m_Id = id;
		}

		#region Methods

		public void Dispose()
		{
			foreach (ILightingLoadEnvironmentPeripheral load in m_IdToLoad.Values)
			{
				Unsubscribe(load);
				load.Dispose();
			}

			m_Loads.Clear();
			m_Presets.Clear();
			m_Shades.Clear();
			m_ShadeGroups.Clear();

			m_IdToLoad.Clear();
			m_IdToPreset.Clear();
			m_IdToShade.Clear();
			m_IdToShadeGroup.Clear();
		}

		public void AddLoad(ILightingLoadEnvironmentPeripheral load)
		{
			m_CacheSection.Enter();

			try
			{
				// Remove existing
				m_Loads.Remove(load.Id);
				ILightingLoadEnvironmentPeripheral existing = m_IdToLoad.GetDefault(load.Id, null);
				Unsubscribe(existing);
				if (existing != null)
					existing.Dispose();

				// Add new
				m_Loads.Add(load.Id);
				m_IdToLoad[load.Id] = load;
				Subscribe(load);
			}
			finally
			{
				m_CacheSection.Leave();
			}

			OnControlsChanged.Raise(this);
		}

		public void AddShade(IShadeEnvironmentPeripheral shade)
		{
			m_CacheSection.Enter();

			try
			{
				m_Shades.Remove(shade.Id);
				m_Shades.Add(shade.Id);
				m_IdToShade[shade.Id] = shade;
			}
			finally
			{
				m_CacheSection.Leave();
			}

			OnControlsChanged.Raise(this);
		}

		public void AddShadeGroup(IShadeEnvironmentPeripheral shadeGroup)
		{
			m_CacheSection.Enter();

			try
			{
				m_ShadeGroups.Remove(shadeGroup.Id);
				m_ShadeGroups.Add(shadeGroup.Id);
				m_IdToShadeGroup[shadeGroup.Id] = shadeGroup;
			}
			finally
			{
				m_CacheSection.Leave();
			}

			OnControlsChanged.Raise(this);
		}

		public void AddPreset(IPresetEnvironmentPeripheral preset)
		{
			m_CacheSection.Enter();

			try
			{
				m_Presets.Remove(preset.Id);
				m_Presets.Add(preset.Id);
				m_IdToPreset[preset.Id] = preset;
			}
			finally
			{
				m_CacheSection.Leave();
			}

			OnControlsChanged.Raise(this);
		}

		/// <summary>
		/// Convenience method for adding a LightingProcessorControl to the room.
		/// </summary>
		/// <param name="control"></param>
		public void AddControl(LightingProcessorControl control)
		{
			if (control.Room != m_Id)
				throw new InvalidOperationException();

			int id = control.Id;
			int room = control.Room;
			string name = control.Name;

			switch (control.PeripheralType)
			{
				case LightingProcessorControl.ePeripheralType.Load:
					AddLoad(new MockLightingLoadControl(id, room, name));
					break;
				case LightingProcessorControl.ePeripheralType.Shade:
					AddShade(new MockLightingShadeControl(id, room, name, control.ShadeType));
					break;
				case LightingProcessorControl.ePeripheralType.ShadeGroup:
					AddShadeGroup(new MockLightingShadeGroupControl(id, room, name, control.ShadeType));
					break;
				case LightingProcessorControl.ePeripheralType.Preset:
					AddPreset(new MockLightingPresetControl(id, room, name));
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public ILightingLoadEnvironmentPeripheral GetLoad(int load)
		{
			return m_CacheSection.Execute(() => m_IdToLoad[load]);
		}

		public IShadeEnvironmentPeripheral GetShade(int shade)
		{
			return m_CacheSection.Execute(() => m_IdToShade[shade]);
		}

		public IShadeEnvironmentPeripheral GetShadeGroup(int shadeGroup)
		{
			return m_CacheSection.Execute(() => m_IdToShadeGroup[shadeGroup]);
		}

		public IPresetEnvironmentPeripheral GetPreset(int preset)
		{
			return m_CacheSection.Execute(() => m_IdToPreset[preset]);
		}

		public IEnumerable<LightingProcessorControl> GetShades()
		{
			return m_CacheSection.Execute(() => m_Shades.Select(i => m_IdToShade[i])
			                                            .Select(s => s.ToLightingProcessorControl())
			                                            .ToArray());
		}

		public IEnumerable<LightingProcessorControl> GetLoads()
		{
			return m_CacheSection.Execute(() => m_Loads.Select(i => m_IdToLoad[i])
			                                           .Select(l => l.ToLightingProcessorControl())
			                                           .ToArray());
		}

		public IEnumerable<LightingProcessorControl> GetShadeGroups()
		{
			return m_CacheSection.Execute(() => m_ShadeGroups.Select(i => m_IdToShadeGroup[i])
			                                                 .Select(s => s.ToLightingProcessorControl())
			                                                 .ToArray());
		}

		public IEnumerable<LightingProcessorControl> GetPresets()
		{
			return m_CacheSection.Execute(() => m_Presets.Select(i => m_IdToPreset[i])
			                                             .Select(p => p.ToLightingProcessorControl())
			                                             .ToArray());
		}

		#endregion

		#region Load Callbacks

		/// <summary>
		/// Subscribe to the load events.
		/// </summary>
		/// <param name="load"></param>
		private void Subscribe(ILightingLoadEnvironmentPeripheral load)
		{
			if (load == null)
				return;

			load.OnLoadLevelChanged += LoadOnLoadLevelChanged;
		}

		/// <summary>
		/// Unsubscribe from the load events.
		/// </summary>
		/// <param name="load"></param>
		private void Unsubscribe(ILightingLoadEnvironmentPeripheral load)
		{
			if (load == null)
				return;

			load.OnLoadLevelChanged -= LoadOnLoadLevelChanged;
		}

		/// <summary>
		/// Called when a load level changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="data"></param>
		private void LoadOnLoadLevelChanged(object sender, FloatEventArgs data)
		{
			MockLightingLoadControl load = sender as MockLightingLoadControl;
			if (load == null)
				return;

			OnLoadLevelChanged.Raise(this, new RoomLoadLevelEventArgs(Id, load.Id, data.Data));
		}

		#endregion

		#region Console

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			ILightingLoadEnvironmentPeripheral[] loads = m_CacheSection.Execute(() => m_IdToLoad.Values.ToArray());
			yield return ConsoleNodeGroup.KeyNodeMap("Loads", loads, control => (uint)control.Id);

			IShadeEnvironmentPeripheral[] shades = m_CacheSection.Execute(() => m_IdToShade.Values.ToArray());
			yield return ConsoleNodeGroup.KeyNodeMap("Shades", shades, control => (uint)control.Id);

			IShadeEnvironmentPeripheral[] shadeGroups = m_CacheSection.Execute(() => m_IdToShadeGroup.Values.ToArray());
			yield return ConsoleNodeGroup.KeyNodeMap("ShadeGroups", shadeGroups, control => (uint)control.Id);
		}

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			addRow("Id", Id);
			addRow("Active Preset", ActivePreset);
			addRow("Occupancy", Occupancy);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			string help = string.Format("SetOccupancy <STATE {0}>",
			                            StringUtils.ArrayFormat(EnumUtils.GetValues<eOccupancyState>()));
			yield return
				new GenericConsoleCommand<eOccupancyState>("SetOccupancy", help, state => Occupancy = state);

			yield return
				new GenericConsoleCommand<int>("SetActivePreset", "SetActivePreset <PRESET>", preset => ActivePreset = preset);
			yield return
				new ConsoleCommand("ClearActivePreset", "Sets the active room preset to null", () => ActivePreset = null);

			yield return
				new GenericConsoleCommand<int, string>("AddPreset", "AddPreset <ID> <NAME>",
				                                       (i, s) => AddPreset(new MockLightingPresetControl(i, Id, s)));
			yield return
				new GenericConsoleCommand<int, string>("AddLoad", "AddLoad <ID> <NAME>",
				                                       (i, s) => AddLoad(new MockLightingLoadControl(i, Id, s)));
			yield return
				new GenericConsoleCommand<int, string>("AddShade", "AddShade <ID> <NAME>",
				                                       (i, s) => AddShade(new MockLightingShadeControl(i, Id, s)));
			yield return
				new GenericConsoleCommand<int, string>("AddShadeGroup", "AddShadeGroup <ID> <NAME>",
				                                       (i, s) => AddShadeGroup(new MockLightingShadeGroupControl(i, Id, s)));
		}

		#endregion
	}
}
