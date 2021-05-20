using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Lighting.EventArguments;
using ICD.Connect.Lighting.RoomInterface;
using ICD.Connect.Partitioning.Commercial.Controls.Occupancy;
using ICD.Connect.Settings.CrestronSPlus.SPlusShims;
#if SIMPLSHARP
using ICDPlatformString = Crestron.SimplSharp.SimplSharpString;
#else
using ICDPlatformString = System.String;
#endif

namespace ICD.Connect.Lighting.CrestronSPlus.Shim
{
	public delegate void SPlusUShortDelegate(ushort data);

	public delegate void SPlusLightingControlInfoDelegate(ushort index, ICDPlatformString name, ushort data);

	[PublicAPI("S+")]
	public sealed class LightingRoomInterfaceShim : AbstractSPlusOriginatorShim<ILightingRoomInterfaceDevice>
	{

		private const int SPLUS_INDEX_START = 1;
		private const int PRESET_NULL = 0;

		private readonly BiDictionary<ushort, LightingProcessorControl> m_PresetControlsByIndex;
		private readonly BiDictionary<int, LightingProcessorControl>  m_PresetControlsById; 
		private readonly SafeCriticalSection m_PresetControlsSection;

		private readonly BiDictionary<ushort, LightingProcessorControl> m_LoadControlsByIndex;
		private readonly BiDictionary<int, LightingProcessorControl> m_LoadControlsById;
		private readonly SafeCriticalSection m_LoadControlsSection;

		private eOccupancyState m_OccupancyState;

		#region SPlus Delegates

		[PublicAPI("S+")]
		public SPlusUShortDelegate SPlusLightingPresetCount { get; set; }

		[PublicAPI("S+")]
		public SPlusLightingControlInfoDelegate SPlusLightingPresetControl { get; set; }

		[PublicAPI("S+")]
		public SPlusUShortDelegate SPlusLightingPresetActive { get; set; }

		[PublicAPI("S+")]
		public SPlusUShortDelegate SPlusLightingLoadCount { get; set; }

		[PublicAPI("S+")]
		public SPlusLightingControlInfoDelegate SPlusLightingLoadControl { get; set; }

		[PublicAPI("S+")]
		public SPlusUShortDelegate SPlusOccupancyState { get; set; }

		#endregion

		#region Properties

		[PublicAPI]
		public eOccupancyState OccupancyState
		{
			get { return m_OccupancyState; }
			private set
			{
				if (m_OccupancyState == value)
					return;

				m_OccupancyState = value;

				var callback = SPlusOccupancyState;
				if (callback != null)
					callback((ushort)value);
			}
		}

		#endregion


		public LightingRoomInterfaceShim()
		{
			m_PresetControlsByIndex = new BiDictionary<ushort, LightingProcessorControl>();
			m_PresetControlsById = new BiDictionary<int, LightingProcessorControl>();
			m_PresetControlsSection = new SafeCriticalSection();
			m_LoadControlsByIndex = new BiDictionary<ushort, LightingProcessorControl>();
			m_LoadControlsById = new BiDictionary<int, LightingProcessorControl>();
			m_LoadControlsSection = new SafeCriticalSection();
			m_OccupancyState = eOccupancyState.Unknown;
		}

		#region SPlus Methods

		/// <summary>
		/// Sets the preset to the specified index
		/// If presetIndex is less than ushort.MinValue or
		/// greater than ushort.MaxValue, the preset is set to null
		/// </summary>
		/// <param name="presetIndex"></param>
		[PublicAPI("S+")]
		public void SetPreset(int presetIndex)
		{
			if (Originator == null)
				return;

			if (presetIndex < ushort.MinValue || presetIndex > ushort.MaxValue)
			{
				Originator.SetPreset(null);
				return;
			}

			LightingProcessorControl preset;
			if (!TryGetPresetFromIndex((ushort)presetIndex, out preset))
				return;

			Originator.SetPreset(preset.Id);
		}

		/// <summary>
		/// Starts raising the load level for the given load index
		/// </summary>
		/// <param name="loadIndex"></param>
		[PublicAPI("S+")]
		public void StartRaisingLoadLevel(ushort loadIndex)
		{
			if (Originator == null)
				return;

			LightingProcessorControl load;
			if (!TryGetLoadFromIndex(loadIndex, out load))
				return;

			Originator.StartRaisingLoadLevel(load.Id);
		}

		/// <summary>
		/// Starts Lowering the load level for the given load index
		/// </summary>
		/// <param name="loadIndex"></param>
		[PublicAPI("S+")]
		public void StartLoweringLoadLevel(ushort loadIndex)
		{
			if (Originator == null)
				return;

			LightingProcessorControl load;
			if (!TryGetLoadFromIndex(loadIndex, out load))
				return;

			Originator.StartLoweringLoadLevel(load.Id);
		}

		/// <summary>
		/// Stops ramping the load level for the given load index
		/// </summary>
		/// <param name="loadIndex"></param>
		[PublicAPI("S+")]
		public void StopRampingLoadLevel(ushort loadIndex)
		{
			if (Originator == null)
				return;

			LightingProcessorControl load;
			if (!TryGetLoadFromIndex(loadIndex, out load))
				return;

			Originator.StopRampingLoadLevel(load.Id);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Called when the originator is attached.
		/// Do any actions needed to syncronize
		/// </summary>
		protected override void InitializeOriginator()
		{
			base.InitializeOriginator();

			if (Originator == null)
				return;

			RefreshPresets();
			RefreshLoads();
			SetPresetActive(Originator.GetPreset());
		}

		/// <summary>
		/// Called when the originator is detached
		/// Do any actions needed to desyncronize
		/// </summary>
		protected override void DeinitializeOriginator()
		{
			base.DeinitializeOriginator();

			ClearLoads();
			ClearPresets();

			var presetCount = SPlusLightingPresetCount;
			if (presetCount != null)
				presetCount(0);

			var lightingCount = SPlusLightingLoadCount;
			if (lightingCount != null)
				lightingCount(0);

			var presetActive = SPlusLightingPresetActive;
			if (presetActive != null)
				presetActive(PRESET_NULL);
		}

		/// <summary>
		/// Tries to get the load at the specified index
		/// </summary>
		/// <param name="loadIndex"></param>
		/// <param name="load"></param>
		/// <returns>True if load was found, false if not</returns>
		private bool TryGetLoadFromIndex(ushort loadIndex, out LightingProcessorControl load)
		{
			m_LoadControlsSection.Enter();
			try
			{
				return m_LoadControlsByIndex.TryGetValue(loadIndex, out load);
			}
			finally
			{
				m_LoadControlsSection.Leave();
			}
		}

		/// <summary>
		/// Tries to get the preset at the specified index
		/// </summary>
		/// <param name="presetIndex"></param>
		/// <param name="preset"></param>
		/// <returns>True if preset was found, false if not</returns>
		private bool TryGetPresetFromIndex(ushort presetIndex, out LightingProcessorControl preset)
		{
			m_PresetControlsSection.Enter();
			try
			{
				return m_PresetControlsByIndex.TryGetValue(presetIndex, out preset);
			}
			finally
			{
				m_PresetControlsSection.Leave();
			}
		}

		/// <summary>
		/// Pulls the current presets from the originator, and if the collection has changed,
		/// refreshes the data to S+
		/// </summary>
		private void RefreshPresets()
		{
			if (Originator == null)
				return;

			IEnumerable<LightingProcessorControl> presets = Originator.GetPresets();

			var dict = new BiDictionary<ushort, LightingProcessorControl>();

			ushort i = SPLUS_INDEX_START;
			foreach (var preset in presets)
			{
				dict.Add(i, preset);
				i++;
			}

			m_PresetControlsSection.Enter();

			try
			{
				if (m_PresetControlsByIndex.DictionaryEqual(dict))
					return;

				m_PresetControlsByIndex.Clear();
				m_PresetControlsById.Clear();
				m_PresetControlsByIndex.AddRange(dict);
				m_PresetControlsById.AddRange(dict.Values, v => v.Id);
			}
			finally
			{
				m_PresetControlsSection.Leave();
			}

			SPlusUShortDelegate countCallback = SPlusLightingPresetCount;
			if (countCallback != null)
				countCallback((ushort)dict.Count);

			
			SPlusLightingControlInfoDelegate itemCallback = SPlusLightingPresetControl;
			if (itemCallback == null)
				return;

			int? activePreset = Originator.GetPreset();
			foreach (KeyValuePair<ushort, LightingProcessorControl> kvp in dict)
			{
				bool active = activePreset.HasValue && activePreset.Value == kvp.Value.Id;
				itemCallback(kvp.Key, SPlusSafeString(kvp.Value.Name), active.ToUShort());
			}

		}


		/// <summary>
		/// Pulls the current Loads from the originator, and if the collection has changed,
		/// refreshes the data to S+
		/// </summary>
		private void RefreshLoads()
		{
			if (Originator == null)
				return;

			IEnumerable<LightingProcessorControl> loads = Originator.GetLoads();

			var dict = new BiDictionary<ushort, LightingProcessorControl>();

			ushort i = SPLUS_INDEX_START;
			foreach (var load in loads)
			{
				dict.Add(i, load);
				i++;
			}

			m_LoadControlsSection.Enter();

			try
			{
				if (m_LoadControlsByIndex.DictionaryEqual(dict))
					return;

				m_LoadControlsByIndex.Clear();
				m_LoadControlsById.Clear();
				m_LoadControlsByIndex.AddRange(dict);
				m_LoadControlsById.AddRange(dict.Values, v => v.Id);
			}
			finally
			{
				m_LoadControlsSection.Leave();
			}

			SPlusUShortDelegate countCallback = SPlusLightingLoadCount;
			if (countCallback != null)
				countCallback((ushort)dict.Count);


			SPlusLightingControlInfoDelegate itemCallback = SPlusLightingLoadControl;
			if (itemCallback == null)
				return;

			foreach (KeyValuePair<ushort, LightingProcessorControl> kvp in dict)
				itemCallback(kvp.Key, SPlusSafeString(kvp.Value.Name), FromFloatLevel(Originator.GetLoadLevel(kvp.Value.Id)));
		}

		/// <summary>
		/// Clears the preset collections
		/// </summary>
		private void ClearPresets()
		{
			m_PresetControlsSection.Enter();
			try
			{
				m_PresetControlsById.Clear();
				m_PresetControlsByIndex.Clear();
			}
			finally
			{
				m_PresetControlsSection.Leave();
			}
		}

		/// <summary>
		/// Clears the load collections
		/// </summary>
		private void ClearLoads()
		{
			m_LoadControlsSection.Enter();
			try
			{
				m_LoadControlsById.Clear();
				m_LoadControlsByIndex.Clear();
			}
			finally
			{
				m_LoadControlsSection.Leave();
			}

		}

		/// <summary>
		/// Updates S+ with the current load level, if it exists in the dictionary
		/// </summary>
		/// <param name="loadId"></param>
		/// <param name="level"></param>
		private void SetLoadLevel(int loadId, float level)
		{
			SPlusLightingControlInfoDelegate callback = SPlusLightingLoadControl;
			if (callback == null)
				return;

			LightingProcessorControl load;
			ushort loadIndex;

			m_LoadControlsSection.Enter();
			try
			{
				
				if (!m_LoadControlsById.TryGetValue(loadId, out load))
					return;
				if (!m_LoadControlsByIndex.TryGetKey(load, out loadIndex))
					return;
			}
			finally
			{
				m_LoadControlsSection.Leave();
			}

			callback(loadIndex, SPlusSafeString(load.Name), FromFloatLevel(level));
		}

		private void SetPresetActive(int? presetId)
		{
			SPlusUShortDelegate callback = SPlusLightingPresetActive;

			if (callback == null)
				return;

			// If no preset is selected, send the preset null value to S+
			if (!presetId.HasValue)
			{
				callback(PRESET_NULL);
				return;
			}

			ushort presetIndex;

			m_PresetControlsSection.Enter();
			try
			{
				LightingProcessorControl preset;
				if (!m_PresetControlsById.TryGetValue(presetId.Value, out preset))
					return;
				if (!m_PresetControlsByIndex.TryGetKey(preset, out presetIndex))
					return;
			}
			finally
			{
				m_PresetControlsSection.Leave();
			}

			callback(presetIndex);
		}

		#endregion

		#region Originator Callbacks

		/// <summary>
		/// Subscribes to the originator events.
		/// </summary>
		/// <param name="originator"></param>
		protected override void Subscribe(ILightingRoomInterfaceDevice originator)
		{
			base.Subscribe(originator);

			if (originator == null)
				return;

			originator.OnControlsChanged += OriginatorOnControlsChanged;
			originator.OnPresetChanged += OriginatorOnPresetChanged;
			originator.OnLoadLevelChanged += OriginatorOnLoadLevelChanged;
			originator.OnOccupancyChanged += OriginatorOnOccupancyChanged;
		}

		/// <summary>
		/// Unsubscribes from the originator events.
		/// </summary>
		/// <param name="originator"></param>
		protected override void Unsubscribe(ILightingRoomInterfaceDevice originator)
		{
			base.Unsubscribe(originator);

			if (originator == null)
				return;

			originator.OnControlsChanged -= OriginatorOnControlsChanged;
			originator.OnPresetChanged -= OriginatorOnPresetChanged;
			originator.OnLoadLevelChanged -= OriginatorOnLoadLevelChanged;
			originator.OnOccupancyChanged -= OriginatorOnOccupancyChanged;
		}

		private void OriginatorOnControlsChanged(object sender, EventArgs args)
		{
			RefreshPresets();
			RefreshLoads();
			OccupancyState = Originator.GetOccupancy();
		}

		private void OriginatorOnPresetChanged(object sender, GenericEventArgs<int?> args)
		{
			SetPresetActive(args.Data);
		}

		private void OriginatorOnLoadLevelChanged(object sender, LoadLevelEventArgs args)
		{
			SetLoadLevel(args.LoadId, args.Percentage);
		}

		private void OriginatorOnOccupancyChanged(object sender, GenericEventArgs<eOccupancyState> args)
		{
			OccupancyState = args.Data;
		}

		#endregion

		#region Static Methods

		private static ushort FromFloatLevel(float level)
		{
			return (ushort)(level * ushort.MaxValue);
		}

		#endregion
	}
}