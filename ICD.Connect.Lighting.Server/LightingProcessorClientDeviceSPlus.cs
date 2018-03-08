using ICD.Common.Properties;
#if SIMPLSHARP
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Lighting.EventArguments;
using ICD.Connect.Protocol.Network.Tcp;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;

namespace ICD.Connect.Lighting.Server
{
	[PublicAPI("S+")]
	public sealed class LightingProcessorClientDeviceSPlus
	{
		#region Fields

		private LightingProcessorClientDevice m_LightingProcessorClient;

		private AsyncTcpClient m_TcpClient;

		private readonly SafeCriticalSection m_CollectionsCriticalSection = new SafeCriticalSection();

		// Presets
		//todo: make this suck less
		private List<LightingProcessorControl> m_Presets; // This is in-order list
		private Dictionary<int, LightingProcessorControl> m_PresetsId; // Key is preset id, value is preset
		private Dictionary<int, int> m_PresetsOrderMap; // Key is Preset Id, value is position in list

		private List<LightingProcessorControl> m_Loads; // This is in-order list
		private Dictionary<int, LightingProcessorControl> m_LoadsId; //  Key is preset id, value is preset
		private Dictionary<int, int> m_LoadsOrderMap; //Key is preset Id, value is position in list 

		private List<LightingProcessorControl> m_Shades; // This is in-order list
		private Dictionary<int, LightingProcessorControl> m_ShadesId; //  Key is preset id, value is preset
		private Dictionary<int, int> m_ShadesOrderMap; //Key is preset Id, value is position in list 

		#endregion

		#region SPlus Methods / Delegates

		#region SPlus Setup Methods

		public void SPlusSetAddress(SimplSharpString address, ushort port, ushort roomId)
		{
			if (m_LightingProcessorClient != null)
			{
				m_LightingProcessorClient.Disconnect();
				m_LightingProcessorClient.Dispose();
			}
			if (m_TcpClient != null)
			{
				m_TcpClient.Disconnect();
				m_TcpClient.Dispose();
			}


			m_TcpClient = new AsyncTcpClient {Address = address.ToString(), Port = port};
			m_LightingProcessorClient = new LightingProcessorClientDevice();
			m_LightingProcessorClient.SetRoomId(roomId);

			SubscribeToLightingDevice();

			m_LightingProcessorClient.SetPort(m_TcpClient);
			m_LightingProcessorClient.Connect();
		}

		public void SPlusConnect()
		{
			m_LightingProcessorClient.Connect();
		}

		public void SPlusDisconnect()
		{
			m_LightingProcessorClient.Disconnect();
		}

		public void SPlusDispose()
		{
			if (m_LightingProcessorClient != null)
			{
				m_LightingProcessorClient.Disconnect();
				m_LightingProcessorClient.Dispose();
			}
			if (m_TcpClient != null)
			{
				m_TcpClient.Disconnect();
				m_TcpClient.Dispose();
			}
		}

		#endregion

		#region SPlus Callbacks


		public delegate void DelNumberUpdate(ushort number);

		public delegate void DelNameUpdate(ushort index, SimplSharpString name);

		public delegate void DelValueUpdate(ushort index, ushort value);


		public DelNumberUpdate SPlusPresetCountUpdate { get; set; }

		public DelNameUpdate SPlusPresetNameUpdate { get; set; }

		public DelNumberUpdate SPlusPresetActiveUpdate { get; set; }

		public DelNumberUpdate SPlusLoadCountUpdate { get; set; }

		public DelNameUpdate SPlusLoadNameUpdate { get; set; }

		public DelValueUpdate SPlusLoadValueUpdate { get; set; }

		public DelNumberUpdate SPlusShadeCountUpdate { get; set; }

		public DelNameUpdate SPlusShadeNameUpdate { get; set; }

		/// <summary>
		/// Occupancy Changes
		/// 0 = Unknown/Inactive
		/// 1 = Unoccupied
		/// 2 = Occupied
		/// </summary>
		public DelNumberUpdate SPlusOccupancyUpdate { get; set; }

		public DelNumberUpdate SPlusIsOnlineUpdate { get; set; }

		public DelNumberUpdate SPlusIsConnectedUpdate { get; set; }

		#endregion

		#region SPlus Controls

		/// <summary>
		/// Recall a preset
		/// </summary>
		/// <param name="presetIndex">Index of preset to recall</param>
		public void SPlusRecallPreset(ushort presetIndex)
		{
			int? presetId = null;
			m_CollectionsCriticalSection.Enter();
			try
			{
				if (presetIndex < m_Presets.Count)
					presetId = m_Presets[presetIndex].Id;
			}
			finally
			{
				m_CollectionsCriticalSection.Leave();
			}
			if (presetId != null)
				m_LightingProcessorClient.SetPreset(presetId);
		}

		/// <summary>
		/// Control ramping of loads
		/// </summary>
		/// <param name="loadIndex">Index of load</param>
		/// <param name="action">Ramp Action - 0 = Stop, 1 = Lower, 2 = Raise</param>
		public void SPlusLoadControl(ushort loadIndex, ushort action)
		{
			int loadId;

			m_CollectionsCriticalSection.Enter();
			try
			{
				if (loadIndex < m_Loads.Count)
					loadId = m_Loads[loadIndex].Id;
				else
					return;
			}
			finally
			{
				m_CollectionsCriticalSection.Leave();
			}

			switch (action)
			{
				case 0:
					m_LightingProcessorClient.StopRampingLoadLevel(loadId);
					break;
				case 1:
					m_LightingProcessorClient.StartLoweringLoadLevel(loadId);
					break;
				case 2:
					m_LightingProcessorClient.StartRaisingLoadLevel(loadId);
					break;
			}
		}

		/// <summary>
		/// Control direction of shades
		/// </summary>
		/// <param name="shadeIndex">Index of Shade (or shade group)</param>
		/// <param name="action">Ramp Action - 0 = Stop, 1 = Lower, 2 = Raise</param>
		public void SPlusShadeControl(ushort shadeIndex, ushort action)
		{
			int shadeId;
			LightingProcessorControl shadeDevice;


			m_CollectionsCriticalSection.Enter();
			try
			{
				if (shadeIndex < m_Shades.Count)
				{
					shadeId = m_Shades[shadeIndex].Id;
					shadeDevice = m_Shades[shadeIndex];
				}
				else
					return;
			}
			finally
			{
				m_CollectionsCriticalSection.Leave();
			}


			switch (shadeDevice.PeripheralType)
			{
				case LightingProcessorControl.ePeripheralType.Shade:
					switch (action)
					{
						case 0:
							m_LightingProcessorClient.StopMovingShade(shadeId);
							break;
						case 1:
							m_LightingProcessorClient.StartLoweringShade(shadeId);
							break;
						case 2:
							m_LightingProcessorClient.StartLoweringShade(shadeId);
							break;
					}
					break;
				case LightingProcessorControl.ePeripheralType.ShadeGroup:
					switch (action)
					{
						case 0:
							m_LightingProcessorClient.StopMovingShadeGroup(shadeId);
							break;
						case 1:
							m_LightingProcessorClient.StartLoweringShadeGroup(shadeId);
							break;
						case 2:
							m_LightingProcessorClient.StartLoweringShadeGroup(shadeId);
							break;
					}
					break;
			}
		}

		#endregion

		#endregion

		#region Lighting Client Callbacks

		private void LightingProcessorClientOnRoomControlsChanged(object sender, IntEventArgs intEventArgs)
		{
			UpdateControls();
		}

		private void LightingProcessorClientOnRoomPresetChanged(object sender, RoomPresetChangeEventArgs roomPresetChangeEventArgs)
		{
			var valueUpdate = SPlusPresetActiveUpdate;
			if (valueUpdate == null)
				return;

			if (roomPresetChangeEventArgs.Preset == null)
			{
				valueUpdate(65535);
				return;
			}

			int preset = (int)roomPresetChangeEventArgs.Preset;
			int index;

			if (m_PresetsOrderMap.TryGetValue(preset, out index))
				valueUpdate((ushort)index);
			else
				valueUpdate(65535);
		}

		private void LightingProcessorClientOnRoomOccupancyChanged(object sender, RoomOccupancyEventArgs roomOccupancyEventArgs)
		{
			var valueUpdate = SPlusOccupancyUpdate;
			if (valueUpdate == null)
				return;

			switch (roomOccupancyEventArgs.OccupancyState)
			{
				case RoomOccupancyEventArgs.eOccupancyState.Inactive:
					valueUpdate(0);
					break;
				case RoomOccupancyEventArgs.eOccupancyState.Unknown:
					valueUpdate(0);
					break;
				case RoomOccupancyEventArgs.eOccupancyState.Unoccupied:
					valueUpdate(1);
					break;
				case RoomOccupancyEventArgs.eOccupancyState.Occupied:
					valueUpdate(2);
					break;
			}
		}

		private void LightingProcessorClientOnRoomLoadLevelChanged(object sender, RoomLoadLevelEventArgs roomLoadLevelEventArgs)
		{
			var valueUpdate = SPlusLoadValueUpdate;
			if (valueUpdate == null)
				return;

			int loadIndex;
			bool loadFound;

			m_CollectionsCriticalSection.Enter();
			try
			{
				loadFound = m_LoadsOrderMap.TryGetValue(roomLoadLevelEventArgs.LoadId, out loadIndex);
			}
			finally
			{
				m_CollectionsCriticalSection.Leave();
			}

			if (!loadFound)
				return;

			var valueUshort = (ushort)(roomLoadLevelEventArgs.Percentage * 65535);
			valueUpdate((ushort)loadIndex, valueUshort);
		}

		private void LightingProcessorClientOnIsOnlineStateChanged(object sender, BoolEventArgs boolEventArgs)
		{
			var valueUpdate = SPlusIsOnlineUpdate;
			if (valueUpdate == null)
				return;

			valueUpdate((ushort)(boolEventArgs.Data ? 1 : 0));
		}

		private void LightingProcessorClientOnConnectedStateChanged(object sender, BoolEventArgs boolEventArgs)
		{
			var valueUpdate = SPlusIsConnectedUpdate;
			if (valueUpdate == null)
				return;

			valueUpdate((ushort)(boolEventArgs.Data ? 1 : 0));
		}

		#endregion

		#region Methods

		public void UpdateControls()
		{
			if (m_LightingProcessorClient == null)
				return;

			// Get new valuse from the lighting processor client


			var presets = m_LightingProcessorClient.GetPresets().ToList();
			var presetsId = new Dictionary<int, LightingProcessorControl>();
			var presetsOrderMap = new Dictionary<int, int>();

			for (int i = 0; i < presets.Count; i++)
			{
				var p = presets[i];
				presetsId[p.Id] = p;
				presetsOrderMap[p.Id] = i;
			}


			var loads = m_LightingProcessorClient.GetLoads().ToList();
			var loadsId = new Dictionary<int, LightingProcessorControl>();
			var loadsOrderMap = new Dictionary<int, int>();

			for (int i = 0; i < loads.Count; i++)
			{
				var l = loads[i];
				loadsId[l.Id] = l;
				loadsOrderMap[l.Id] = i;
			}


			var shades = m_LightingProcessorClient.GetShadeGroups().ToList();
			// Combine shade groups and individual shades, so both will work.
			shades.AddRange(m_LightingProcessorClient.GetShades());
			var shadesId = new Dictionary<int, LightingProcessorControl>();
			var shadesOrderMap = new Dictionary<int, int>();

			for (int i = 0; i < shades.Count; i++)
			{
				var l = shades[i];
				shadesId[l.Id] = l;
				shadesOrderMap[l.Id] = i;
			}

			m_CollectionsCriticalSection.Enter();
			try
			{
				m_PresetsId = presetsId;
				m_Presets = presets;
				m_PresetsOrderMap = presetsOrderMap;
				m_LoadsId = loadsId;
				m_Loads = loads;
				m_LoadsOrderMap = loadsOrderMap;
				m_ShadesId = shadesId;
				m_Shades = shades;
				m_ShadesOrderMap = shadesOrderMap;

				UpdateSPlusPresets();
				UpdateSPlusLoads();
				UpdateSPlusShades();
			}
			finally
			{
				m_CollectionsCriticalSection.Leave();
			}
		}

		#endregion

		#region Private Methods

		private void UpdateSPlusPresets()
		{
			// Preset updates to S+
			var countUpdate = SPlusPresetCountUpdate;
			if (countUpdate != null)
				countUpdate((ushort)m_PresetsId.Count);

			var nameUpdate = SPlusPresetNameUpdate;
			if (nameUpdate == null)
				return;
			for (int i = 0; i < m_Presets.Count; i++)
			{
				var p = m_Presets[i];
				nameUpdate((ushort)i, new SimplSharpString(p.Name));
			}

			// This method converts the active preset id to index
			LightingProcessorClientOnRoomPresetChanged(this, new RoomPresetChangeEventArgs(m_LightingProcessorClient.Id, m_LightingProcessorClient.GetActivePreset()));
		}

		private void UpdateSPlusLoads()
		{
			var countUpdate = SPlusLoadCountUpdate;
			if (countUpdate != null)
				countUpdate((ushort)m_Loads.Count);

			var nameUpdate = SPlusLoadNameUpdate;
			var valueUpdate = SPlusLoadValueUpdate;
			if (nameUpdate == null && valueUpdate == null)
				return;

			for (ushort i = 0; i < m_Loads.Count ; i++)
			{
				if (nameUpdate != null)
					nameUpdate(i, new SimplSharpString(m_Loads[i].Name));
				if (valueUpdate != null)
					valueUpdate(i, (ushort)m_LightingProcessorClient.GetLoadLevel(m_Loads[i].Id));
			}
		}

		private void UpdateSPlusShades()
		{
			// Shade updates to S+
			var countUpdate = SPlusShadeCountUpdate;
			if (countUpdate != null)
				countUpdate((ushort)m_Shades.Count);

			var nameUpdate = SPlusShadeNameUpdate;
			if (nameUpdate == null)
				return;
			for (int i = 0; i < m_Shades.Count; i++)
			{
				var s = m_Shades[i];
				nameUpdate((ushort)i, new SimplSharpString(s.Name));
			}
		}

		private void SubscribeToLightingDevice()
		{
			if (m_LightingProcessorClient == null)
				return;

			m_LightingProcessorClient.OnConnectedStateChanged += LightingProcessorClientOnConnectedStateChanged;
			m_LightingProcessorClient.OnIsOnlineStateChanged += LightingProcessorClientOnIsOnlineStateChanged;
			m_LightingProcessorClient.OnRoomControlsChanged += LightingProcessorClientOnRoomControlsChanged;
			m_LightingProcessorClient.OnRoomLoadLevelChanged += LightingProcessorClientOnRoomLoadLevelChanged;
			m_LightingProcessorClient.OnRoomOccupancyChanged += LightingProcessorClientOnRoomOccupancyChanged;
			m_LightingProcessorClient.OnRoomPresetChanged += LightingProcessorClientOnRoomPresetChanged;
		}

		private void UnsubscribeToLightingDevice()
		{
			if (m_LightingProcessorClient == null)
				return;

			m_LightingProcessorClient.OnConnectedStateChanged -= LightingProcessorClientOnConnectedStateChanged;
			m_LightingProcessorClient.OnIsOnlineStateChanged -= LightingProcessorClientOnIsOnlineStateChanged;
			m_LightingProcessorClient.OnRoomControlsChanged -= LightingProcessorClientOnRoomControlsChanged;
			m_LightingProcessorClient.OnRoomLoadLevelChanged -= LightingProcessorClientOnRoomLoadLevelChanged;
			m_LightingProcessorClient.OnRoomOccupancyChanged -= LightingProcessorClientOnRoomOccupancyChanged;
			m_LightingProcessorClient.OnRoomPresetChanged -= LightingProcessorClientOnRoomPresetChanged;

			//Send some updates to S+, just in case
			LightingProcessorClientOnConnectedStateChanged(this, new BoolEventArgs(false));
			LightingProcessorClientOnIsOnlineStateChanged(this, new BoolEventArgs(false));
		}

		#endregion
	}
}
#endif