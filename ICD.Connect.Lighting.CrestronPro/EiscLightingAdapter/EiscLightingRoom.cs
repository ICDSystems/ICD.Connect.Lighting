using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Xml;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Lighting.CrestronPro.EiscLightingAdapter.Components;
using ICD.Connect.Misc.Occupancy;
using ICD.Connect.Panels;
using ICD.Connect.Panels.EventArguments;
using ICD.Connect.Protocol.Sigs;

namespace ICD.Connect.Lighting.CrestronPro.EiscLightingAdapter
{
	public sealed class EiscLightingRoom : IDisposable, IConsoleNode
	{
		private const string ID_ATTRIBUTE = "id";
		private const string NAME_ELEMENT = "Name";
		private const string PRESETS_ELEMENT = "Presets";
		private const string LOADS_ELEMENT = "Loads";
		private const string SHADES_ELEMENT = "Shades";
		
		#region events

		public event EventHandler<GenericEventArgs<eOccupancyState>> OnOccupancyStateChanged;

		public event EventHandler<GenericEventArgs<int?>> OnPresetChanged;

		public event EventHandler<LoadLevelEventArgs> OnLoadLevelChanged;

		public event EventHandler OnControlsChanged;

		#endregion

		#region Fields
		private readonly EiscLightingAdapterDevice m_Parent;

		private int? m_ActivePreset;
		private eOccupancyState m_Occupancy;

		private readonly Dictionary<int, PresetRoomComponent> m_Presets;
		private readonly SafeCriticalSection m_PresetsSection;

		private readonly Dictionary<int, LoadRoomComponent> m_Loads;
		private readonly SafeCriticalSection m_LoadsSection;

		private readonly Dictionary<int, ShadeRoomComponent> m_Shades;
		private readonly SafeCriticalSection m_ShadesSection;

		#endregion

		#region Properties
		public int Id { get; private set; }
		public string Name { get; private set; }

		public int? ActivePreset
		{
			get { return m_ActivePreset; }
			private set
			{
				if (m_ActivePreset == value)
					return;

				m_ActivePreset = value;

				OnPresetChanged.Raise(this, new GenericEventArgs<int?>(value));
			}
		}

		public eOccupancyState Occupancy
		{
			get { return m_Occupancy; }
			private set
			{
				if (m_Occupancy == value)
					return;

				m_Occupancy = value;

				OnOccupancyStateChanged.Raise(this, new GenericEventArgs<eOccupancyState>(value));
			}
		}
		#endregion

		private EiscLightingRoom(EiscLightingAdapterDevice parent)
		{
			m_Parent = parent;

			m_Presets = new Dictionary<int, PresetRoomComponent>();
			m_PresetsSection = new SafeCriticalSection();

			m_Loads = new Dictionary<int, LoadRoomComponent>();
			m_LoadsSection = new SafeCriticalSection();

			m_Shades = new Dictionary<int, ShadeRoomComponent>();
			m_ShadesSection = new SafeCriticalSection();
		}

		public static EiscLightingRoom FromXml(string xml, EiscLightingAdapterDevice parent)
		{
			int id = XmlUtils.GetAttributeAsInt(xml, ID_ATTRIBUTE);
			string name = XmlUtils.ReadChildElementContentAsString(xml, NAME_ELEMENT);

			string presetsXml;
			string loadsXml;
			string shadesXml;

			XmlUtils.TryGetChildElementAsString(xml, PRESETS_ELEMENT, out presetsXml);
			XmlUtils.TryGetChildElementAsString(xml, LOADS_ELEMENT, out loadsXml);
			XmlUtils.TryGetChildElementAsString(xml, SHADES_ELEMENT, out shadesXml);

			EiscLightingRoom room = new EiscLightingRoom(parent)
			{
				Id = id,
				Name = name
			};

			if (!String.IsNullOrEmpty(presetsXml))
				room.ParsePresets(presetsXml);
			if (!String.IsNullOrEmpty(loadsXml))
				room.ParseLoads(loadsXml);
			if (!String.IsNullOrEmpty(shadesXml))
				room.ParseShades(shadesXml);

			return room;
		}

		public void Dispose()
		{
			ClearPresets();
			ClearLoads();

			OnOccupancyStateChanged = null;
			OnPresetChanged = null;
			OnLoadLevelChanged = null;
			OnControlsChanged = null;
		}

		#region EISC Methods

		internal SigInfo RegisterSigChangeCallback(uint number, eSigType type,
									  Action<SigCallbackManager, SigInfoEventArgs> callback)
		{
			return m_Parent.RegisterSigChangeCallback(number, type, callback);
		}

		internal void UnregisterSigChangeCallback(uint number, eSigType type,
												   Action<SigCallbackManager, SigInfoEventArgs> callback)
		{
			m_Parent.UnregisterSigChangeCallback(number, type, callback);
		}

		internal void SendDigitalJoin(uint join, bool value)
		{
			m_Parent.SendDigitalJoin(join, value);
		}

		internal void SendAnalogJoin(uint join, ushort value)
		{
			m_Parent.SendAnalogJoin(join, value);
		}

		internal void SendStringJoin(uint join, string value)
		{
			m_Parent.SendStringJoin(join, value);
		}

		#endregion


		#region Presets

		private void ParsePresets(string xml)
		{
			IEnumerable<PresetRoomComponent> items = XmlUtils.GetChildElementsAsString(xml)
			                                                 .Select(x => PresetRoomComponent.FromXml(x, this));
			m_PresetsSection.Enter();
			try
			{
				ClearPresets();

				m_Presets.AddRange(items, i => i.Id);

				m_Presets.Values.ForEach(Subscribe);
			}
			finally
			{
				m_PresetsSection.Leave();
			}

			OnControlsChanged.Raise(this);
		}

		private void ClearPresets()
		{
			m_PresetsSection.Enter();

			try
			{
				foreach (var preset in m_Presets.Values)
				{
					Unsubscribe(preset);
					preset.Dispose();
				}
				m_Loads.Clear();
			}
			finally
			{
				m_PresetsSection.Leave();
			}
		}

		public IEnumerable<LightingProcessorControl> GetPresets()
		{
			return m_PresetsSection.Execute(() => m_Presets.Values.Select(p => p.ToLightingProcessorControl()).ToArray(m_Presets.Count));
		}

		public void SetPreset(int? preset)
		{
			if (!preset.HasValue)
				return;

			PresetRoomComponent presetComponent;

			m_PresetsSection.Enter();
			try
			{
				if (!m_Presets.TryGetValue(preset.Value, out presetComponent))
					throw new ArgumentOutOfRangeException(String.Format("No preset {0} for room {1}", preset, Id));
			}
			finally
			{
				m_PresetsSection.Leave();
			}

			presetComponent.Activate();
		}

		#region Preset Callbacks

		private void Subscribe(PresetRoomComponent preset)
		{
			if (preset == null)
				return;

			preset.OnPresetStateChanged += PresetOnPresetStateChanged;
		}

		private void Unsubscribe(PresetRoomComponent preset)
		{
			if (preset == null)
				return;

			preset.OnPresetStateChanged -= PresetOnPresetStateChanged;
		}

		private void PresetOnPresetStateChanged(object sender, BoolEventArgs args)
		{
			PresetRoomComponent preset = sender as PresetRoomComponent;
			if (preset == null)
				return;

			// If this preset goes high, set it as the preset for the room
			if (args.Data)
				ActivePreset = preset.Id;
			// If this preset goes low, and it's not the active preset, do nothing
			else if (ActivePreset != preset.Id)
				return;

			//Try to find an active preset
			PresetRoomComponent activePreset;

			m_PresetsSection.Enter();

			try
			{
				activePreset = m_Presets.Values.FirstOrDefault(p => p.Active);
			}
			finally
			{
				m_PresetsSection.Leave();
			}

			ActivePreset = activePreset == null ? null : (int?)activePreset.Id;
		}

		#endregion
		#endregion

		#region Loads

		private void ParseLoads(string xml)
		{
			IEnumerable<LoadRoomComponent> items = XmlUtils.GetChildElementsAsString(xml)
			                                               .Select(x => LoadRoomComponent.FromXml(x,this));
			m_LoadsSection.Enter();
			try
			{
				ClearLoads();

				m_Loads.AddRange(items, i => i.Id);
				
				m_Loads.Values.ForEach(Subscribe);
			}
			finally
			{
				m_LoadsSection.Leave();
			}

			OnControlsChanged.Raise(this);
		}

		private void ClearLoads()
		{
			m_LoadsSection.Enter();

			try
			{
				foreach (var load in m_Loads.Values)
				{
					Unsubscribe(load);
					load.Dispose();
				}
				m_Loads.Clear();
			}
			finally
			{
				m_LoadsSection.Leave();
			}
		}

		public IEnumerable<LightingProcessorControl> GetLoads()
		{
			return m_LoadsSection.Execute(() => m_Loads.Values.Select(p => p.ToLightingProcessorControl()).ToArray(m_Loads.Count));
		}

		public void SetLoadLevel(int loadId, float percentage)
		{
			LoadRoomComponent load = null;

			if (!m_LoadsSection.Execute(() => m_Loads.TryGetValue(loadId, out load)))
				throw new ArgumentOutOfRangeException(String.Format("No load {0} for room {1}", loadId, Id));
			
			load.SetLevel(percentage);
		}

		public float GetLoadLevel(int loadId)
		{
			LoadRoomComponent load = null;

			if (!m_LoadsSection.Execute(() => m_Loads.TryGetValue(loadId, out load)))
				throw new ArgumentOutOfRangeException(String.Format("No load {0} for room {1}", loadId, Id));

			return load.Level;
		}

		public void StartRaisingLoadLevel(int loadId)
		{
			LoadRoomComponent load = null;

			if (!m_LoadsSection.Execute(() => m_Loads.TryGetValue(loadId, out load)))
				throw new ArgumentOutOfRangeException(String.Format("No load {0} for room {1}", loadId, Id));

			load.StartRaising();
		}

		public void StartLoweringLoadLevel(int loadId)
		{
			LoadRoomComponent load = null;

			if (!m_LoadsSection.Execute(() => m_Loads.TryGetValue(loadId, out load)))
				throw new ArgumentOutOfRangeException(String.Format("No load {0} for room {1}", loadId, Id));

			load.StartLowering();
		}

		public void StopRampingLoadLevel(int loadId)
		{
			LoadRoomComponent load = null;

			if (!m_LoadsSection.Execute(() => m_Loads.TryGetValue(loadId, out load)))
				throw new ArgumentOutOfRangeException(String.Format("No load {0} for room {1}", loadId, Id));

			load.StopRamping();
		}

		#region Load Callbacks

		private void Subscribe(LoadRoomComponent load)
		{
			if (load == null)
				return;

			load.OnLevelChanged += LoadOnLevelChanged;
		}

		private void Unsubscribe(LoadRoomComponent load)
		{
			if (load == null)
				return;

			load.OnLevelChanged -= LoadOnLevelChanged;
		}

		private void LoadOnLevelChanged(object sender, FloatEventArgs args)
		{
			LoadRoomComponent load = sender as LoadRoomComponent;
			if (load == null)
				return;
			
			OnLoadLevelChanged.Raise(this, new LoadLevelEventArgs(load.Id, args.Data));
		}

		#endregion
		#endregion

		#region Shades

		private void ParseShades(string xml)
		{
			IEnumerable<ShadeRoomComponent> items = XmlUtils.GetChildElementsAsString(xml)
															 .Select(x => ShadeRoomComponent.FromXml(x, this));
			m_ShadesSection.Enter();
			try
			{
				ClearShades();

				m_Shades.AddRange(items, i => i.Id);

			}
			finally
			{
				m_ShadesSection.Leave();
			}
		}

		private void ClearShades()
		{
			m_ShadesSection.Enter();

			try
			{
				foreach (var shade in m_Shades.Values)
				{
					shade.Dispose();
				}
				m_Shades.Clear();
			}
			finally
			{
				m_ShadesSection.Leave();
			}
		}

		public IEnumerable<LightingProcessorControl> GetShades()
		{
			return m_ShadesSection.Execute(() => m_Shades.Values.Select(s => s.ToLightingProcessorControl()).ToArray(m_Shades.Count));
		}

		public void OpenShade(int shadeId)
		{
			ShadeRoomComponent shade = null;

			if (!m_ShadesSection.Execute(() => m_Shades.TryGetValue(shadeId, out shade)))
				throw new ArgumentOutOfRangeException(String.Format("No shade {0} for room {1}", shadeId, Id));

			shade.OpenShade();
		}

		public void CloseShade(int shadeId)
		{
			ShadeRoomComponent shade = null;

			if (!m_ShadesSection.Execute(() => m_Shades.TryGetValue(shadeId, out shade)))
				throw new ArgumentOutOfRangeException(String.Format("No shade {0} for room {1}", shadeId, Id));

			shade.CloseShade();
		}

		public void StopShade(int shadeId)
		{
			ShadeRoomComponent shade = null;

			if (!m_ShadesSection.Execute(() => m_Shades.TryGetValue(shadeId, out shade)))
				throw new ArgumentOutOfRangeException(String.Format("No shade {0} for room {1}", shadeId, Id));

			shade.StopShade();
		}
		

		#endregion

		#region Console
		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		public string ConsoleName { get { return Name; } }

		/// <summary>
		/// Gets the help information for the node.
		/// </summary>
		public string ConsoleHelp { get { return "Represents a room for EISC Lighting Integration"; } }

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			IEnumerable<PresetRoomComponent> presets = m_PresetsSection.Execute(() => m_Presets.Values.ToList(m_Presets.Count));
			IEnumerable<LoadRoomComponent> loads = m_LoadsSection.Execute(() => m_Loads.Values.ToList(m_Loads.Count));

			yield return ConsoleNodeGroup.KeyNodeMap("Presets", presets, p => (uint)p.Id);
			yield return ConsoleNodeGroup.KeyNodeMap("Loads", loads, l => (uint)l.Id);
		}

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			addRow("Id", Id);
			addRow("Name", Name);
			addRow("ActivePreset", ActivePreset);
			addRow("Occupancy", Occupancy);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			yield return
				new GenericConsoleCommand<int>("ActivePreset", "ActivatePreset<presetId> - Activate preset with given id",
				                               p => SetPreset(p));
		}

		#endregion
	}
}