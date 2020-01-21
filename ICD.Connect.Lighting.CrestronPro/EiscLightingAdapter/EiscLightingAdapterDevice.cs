using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.IO;
using ICD.Common.Utils.Xml;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Lighting.EventArguments;
using ICD.Connect.Misc.Occupancy;
using ICD.Connect.Panels;
using ICD.Connect.Panels.EventArguments;
using ICD.Connect.Protocol.Sigs;
using ICD.Connect.Settings.Core;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.EthernetCommunication;
using ICD.Connect.Misc.CrestronPro;
using ICD.Connect.Misc.CrestronPro.Extensions;
using ICD.Connect.Protocol.Sigs;
#endif

namespace ICD.Connect.Lighting.CrestronPro.EiscLightingAdapter
{
	public sealed partial class EiscLightingAdapterDevice : AbstractDevice<EiscLightingAdapterDeviceSettings>
	{
		private const string ROOM_ELEMNET = "Room";

		#region Events

		public event EventHandler<RoomOccupancyEventArgs> OnRoomOccupancyChanged;

		public event EventHandler<RoomPresetChangeEventArgs> OnRoomPresetChanged;

		public event EventHandler<RoomLoadLevelEventArgs> OnRoomLoadLevelChanged;

		public event EventHandler<IntEventArgs> OnRoomControlsChanged;

		#endregion

		#region Fields

#if SIMPLSHARP
		private EthernetIntersystemCommunications m_Eisc;
#endif
		private byte? m_Ipid;
		private string m_Address;

		private string m_Config;


		private readonly SigCallbackManager m_SigCallbacks;

		private readonly SafeCriticalSection m_RoomsSection;
		private readonly Dictionary<int, EiscLightingRoom> m_Rooms;

		#endregion

		public EiscLightingAdapterDevice()
		{
			m_SigCallbacks = new SigCallbackManager();
			m_RoomsSection = new SafeCriticalSection();
			m_Rooms = new Dictionary<int, EiscLightingRoom>();
		}

		#region Methods

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
#if SIMPLSHARP
			return m_Eisc != null && m_Eisc.IsOnline;
#else
			return false;
#endif
		}

		/// <summary>
		/// Parses the XML file at the given path for configuration data.
		/// </summary>
		/// <param name="path"></param>
		[PublicAPI]
		public void LoadIntegrationConfig(string path)
		{
			m_Config = path;

			string fullPath = PathUtils.GetDefaultConfigPath(EiscLightingAdapterDeviceSettings.CONFIG_PATH, path);
			string xml;

			try
			{
				xml = IcdFile.ReadToEnd(fullPath, new UTF8Encoding(false));
				xml = EncodingUtils.StripUtf8Bom(xml);
			}
			catch (Exception e)
			{
				IcdErrorLog.Error("Failed to load integration config {0} - {1}", fullPath, e.Message);
				return;
			}

			ParseXml(xml);
		}

		private void ParseXml(string xml)
		{
			IEnumerable<EiscLightingRoom> rooms =
				XmlUtils.GetChildElementsAsString(xml, ROOM_ELEMNET).Select(x => EiscLightingRoom.FromXml(x, this));

			m_RoomsSection.Enter();
			try
			{
				ClearRooms();

				m_Rooms.AddRange(rooms, r => r.Id);

				foreach (EiscLightingRoom room in m_Rooms.Values)
				{
					Subscribe(room);
					OnRoomControlsChanged.Raise(this, new IntEventArgs(room.Id));
				}
			}
			finally
			{
				m_RoomsSection.Leave();
			}
		}

		private void ClearRooms()
		{
			m_RoomsSection.Enter();
			try
			{
				foreach (var kvp in m_Rooms)
				{
					Unsubscribe(kvp.Value);
					kvp.Value.Dispose();
				}

				m_Rooms.Clear();
			}
			finally
			{
				m_RoomsSection.Leave();
			}
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);
			if (disposing)
			{
				ClearRooms();
				OnRoomOccupancyChanged = null;
				OnRoomPresetChanged = null;
				OnRoomLoadLevelChanged = null;
				OnRoomControlsChanged = null;
			}
		}

		#endregion

		#region Settings

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			m_Config = null;

			ClearRooms();

#if SIMPLSHARP
			SetEisc(null);
#endif
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(EiscLightingAdapterDeviceSettings settings)
		{
			base.CopySettingsFinal(settings);

			if (m_Ipid.HasValue)
				settings.Ipid = m_Ipid;

			settings.Address = m_Address;

			settings.Config = m_Config;

		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(EiscLightingAdapterDeviceSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			if (!settings.Ipid.HasValue)
				throw new InvalidOperationException(String.Format("Must have IPID defined for {0}", this));
			if (String.IsNullOrEmpty(settings.Address))
				throw new InvalidOperationException(String.Format("Must have address defined for {0}", this));

			m_Address = settings.Address;
			m_Ipid = settings.Ipid;

#if SIMPLSHARP
			EthernetIntersystemCommunications eisc = new EthernetIntersystemCommunications(m_Ipid.Value, m_Address,
			                                                                               ProgramInfo.ControlSystem)
			{
				Description = Name
			};

			SetEisc(eisc);
#endif

			LoadIntegrationConfig(settings.Config);

		}

		#endregion

		#region EISC Callbacks

#if SIMPLSHARP
		private void SetEisc(EthernetIntersystemCommunications eisc)
		{
			if (m_Eisc == eisc)
				return;

			Unsubscribe(m_Eisc);

			if (m_Eisc != null)
				m_Eisc.UnRegister();

			m_Eisc = eisc;

			if (m_Eisc != null)
				m_Eisc.Description = Name;

			Subscribe(m_Eisc);

			if (m_Eisc != null)
				m_Eisc.Register();
		}

		private void Subscribe(EthernetIntersystemCommunications eisc)
		{
			if (eisc == null)
				return;

			eisc.SigChange += EiscOnSigChange;
			eisc.OnlineStatusChange += EiscOnLineStatusChange;
		}

		private void Unsubscribe(EthernetIntersystemCommunications eisc)
		{
			if (eisc == null)
				return;

			eisc.SigChange -= EiscOnSigChange;
			eisc.OnlineStatusChange -= EiscOnLineStatusChange;
		}

		private void EiscOnSigChange(BasicTriList currentDevice, SigEventArgs args)
		{
			SigInfo sigInfo = args.Sig.ToSigInfo();
			m_SigCallbacks.RaiseSigChangeCallback(sigInfo);
		}

		private void EiscOnLineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
		{
			UpdateCachedOnlineStatus();
		}
#endif

		internal void SendDigitalJoin(uint join, bool value)
		{
#if SIMPLSHARP
			if (m_Eisc != null)
				m_Eisc.BooleanInput[join].BoolValue = value;
#endif
		}

		internal void SendAnalogJoin(uint join, ushort value)
		{
#if SIMPLSHARP
			if (m_Eisc != null)
				m_Eisc.UShortInput[join].UShortValue = value;
#endif
		}

		internal void SendStringJoin(uint join, string value)
		{
#if SIMPLSHARP
			if (m_Eisc != null)
				m_Eisc.StringInput[join].StringValue = value;
#endif
		}



		#endregion

		#region Component Callbacks

		internal SigInfo RegisterSigChangeCallback(uint number, Protocol.Sigs.eSigType type,
		                                        Action<SigCallbackManager, SigInfoEventArgs> callback)
		{
			m_SigCallbacks.RegisterSigChangeCallback(number, type, callback);
#if SIMPLSHARP
			if (m_Eisc != null)
			{
				switch (type)
				{
					case (Protocol.Sigs.eSigType.Digital):
						return new SigInfo(number, 0, m_Eisc.BooleanOutput[number].BoolValue);
					case (Protocol.Sigs.eSigType.Analog):
						return new SigInfo(number, 0, m_Eisc.UShortOutput[number].UShortValue);
					case (Protocol.Sigs.eSigType.Serial):
						return new SigInfo(number, 0, m_Eisc.StringOutput[number].StringValue);
				}
			}

#endif
			return new SigInfo();
		}

		internal void UnregisterSigChangeCallback(uint number, Protocol.Sigs.eSigType type,
		                                          Action<SigCallbackManager, SigInfoEventArgs> callback)
		{
			m_SigCallbacks.UnregisterSigChangeCallback(number, type, callback);
		}

#endregion

#region Room Callbacks

		private void Subscribe(EiscLightingRoom room)
		{
			if (room == null)
				return;

			room.OnOccupancyStateChanged += RoomOnOccupancyStateChanged;
			room.OnPresetChanged += RoomOnPresetChanged;
			room.OnLoadLevelChanged += RoomOnLoadLevelChanged;
			room.OnControlsChanged += RoomOnControlsChanged;
		}

		private void Unsubscribe(EiscLightingRoom room)
		{
			if (room == null)
				return;

			room.OnOccupancyStateChanged -= RoomOnOccupancyStateChanged;
			room.OnPresetChanged -= RoomOnPresetChanged;
			room.OnLoadLevelChanged -= RoomOnLoadLevelChanged;
			room.OnControlsChanged -= RoomOnControlsChanged;
		}

		private void RoomOnOccupancyStateChanged(object sender, GenericEventArgs<eOccupancyState> args)
		{
			EiscLightingRoom room = sender as EiscLightingRoom;
			if (room != null)
				OnRoomOccupancyChanged.Raise(this, new RoomOccupancyEventArgs(room.Id, args.Data));
		}

		private void RoomOnPresetChanged(object sender, GenericEventArgs<int?> args)
		{
			EiscLightingRoom room = sender as EiscLightingRoom;
			if (room != null)
				OnRoomPresetChanged.Raise(this, new RoomPresetChangeEventArgs(room.Id, args.Data));
		}

		private void RoomOnLoadLevelChanged(object sender, LoadLevelEventArgs args)
		{
			EiscLightingRoom room = sender as EiscLightingRoom;
			if (room != null)
				OnRoomLoadLevelChanged.Raise(this, new RoomLoadLevelEventArgs(room.Id, args.LoadId, args.Percentage));
		}

		private void RoomOnControlsChanged(object sender, EventArgs args)
		{
			EiscLightingRoom room = sender as EiscLightingRoom;
			if (room != null)
				OnRoomControlsChanged.Raise(this, new IntEventArgs(room.Id));
		}

#endregion

#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			if (m_Ipid.HasValue)
				addRow("IPID", StringUtils.ToIpIdString(m_Ipid.Value));
			addRow("Address", m_Address);
			addRow("Config File", m_Config);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new ConsoleCommand("ReloadConfig", "Reloads the current configuration file", () => LoadIntegrationConfig(m_Config));
			yield return
				new GenericConsoleCommand<string>("LoadConfig", "LoadConfig <file> - loads the specified configuration file",
				                                  s => LoadIntegrationConfig(s));
		}

		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			foreach (IConsoleNodeBase node in GetBaseConsoleNodes())
				yield return node;

			IEnumerable<EiscLightingRoom> rooms = m_RoomsSection.Execute(() => m_Rooms.Values.ToList(m_Rooms.Count));

			yield return ConsoleNodeGroup.KeyNodeMap("Rooms", rooms, r => (uint)r.Id);
		}

		private IEnumerable<IConsoleNodeBase> GetBaseConsoleNodes()
		{
			return base.GetConsoleNodes();
		}

#endregion
	}
}