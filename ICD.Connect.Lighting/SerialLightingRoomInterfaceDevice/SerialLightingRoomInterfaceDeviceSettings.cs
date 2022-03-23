using System;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Xml;
using ICD.Connect.Lighting.RoomInterface;
using ICD.Connect.Protocol.Network.Settings;
using ICD.Connect.Protocol.Settings;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Lighting.SerialLightingRoomInterfaceDevice
{
	[KrangSettings("SerialLightingRoomInterface", typeof(SerialLightingRoomInterfaceDevice))]
	public sealed class SerialLightingRoomInterfaceDeviceSettings : AbstractLightingRoomInterfaceDeviceSettings
	{
		private const string PORT_ELEMENT = "Port";
		private const string PRESETS_ELEMENT = "Presets";
		private const string PRESET_ELEMENT = "Preset";

		private readonly SecureNetworkProperties m_NetworkProperties;       
		private readonly ComSpecProperties m_ComSpecProperties;
		private readonly Dictionary<int, PresetSerialLightingControl> m_Presets;

		/// <summary>
		/// Port to use for communications
		/// </summary>
		public int? Port { get; set; }

		/// <summary>
		/// Properties to use for Network ports
		/// </summary>
		public SecureNetworkProperties NetworkProperties { get { return m_NetworkProperties; } }

		/// <summary>
		/// Properties to use for Com ports
		/// </summary>
		public ComSpecProperties ComSpecProperties { get { return m_ComSpecProperties; } }

		/// <summary>
		/// Lighting Presets
		/// </summary>
		public IEnumerable<PresetSerialLightingControl> Presets
		{
			get { return m_Presets.Values.ToArray(m_Presets.Count); }
		}

		#region DAV Compatible Properties

		/// <summary>
		/// Sets the name for preset 0
		/// </summary>
		[PublicAPI]
		public string Preset00Name { get { return TryGetPresetName(0); } set { SetPresetName(0, value); } }
		
		/// <summary>
		/// Sets the name for preset 1
		/// </summary>
		[PublicAPI]
		public string Preset01Name { get { return TryGetPresetName(1); } set { SetPresetName(1, value); } }
		
		/// <summary>
		/// Sets the name for preset 2
		/// </summary>
		[PublicAPI]
		public string Preset02Name { get { return TryGetPresetName(2); } set { SetPresetName(2, value); } }
		
		/// <summary>
		/// Sets the name for preset 3
		/// </summary>
		[PublicAPI]
		public string Preset03Name { get { return TryGetPresetName(3); } set { SetPresetName(3, value); } }
		
		/// <summary>
		/// Sets the name for preset 4
		/// </summary>
		[PublicAPI]
		public string Preset04Name { get { return TryGetPresetName(4); } set { SetPresetName(4, value); } }
		
		/// <summary>
		/// Sets the name for preset 5
		/// </summary>
		[PublicAPI]
		public string Preset05Name { get { return TryGetPresetName(5); } set { SetPresetName(5, value); } }
		
		/// <summary>
		/// Sets the name for preset 6
		/// </summary>
		[PublicAPI]
		public string Preset06Name { get { return TryGetPresetName(6); } set { SetPresetName(6, value); } }
		
		/// <summary>
		/// Sets the name for preset 7
		/// </summary>
		[PublicAPI]
		public string Preset07Name { get { return TryGetPresetName(7); } set { SetPresetName(7, value); } }
		
		/// <summary>
		/// Sets the name for preset 8
		/// </summary>
		[PublicAPI]
		public string Preset08Name { get { return TryGetPresetName(8); } set { SetPresetName(8, value); } }
		
		/// <summary>
		/// Sets the name for preset 9
		/// </summary>
		[PublicAPI]
		public string Preset09Name { get { return TryGetPresetName(9); } set { SetPresetName(9, value); } }


		/// <summary>
		/// Sets the serial command for preset 0
		/// </summary>
		[PublicAPI]
		public string Preset00SerialCommand { get { return TryGetPresetSerialCommand(0); } set { SetPresetSerialCommand(0, value); } }

		/// <summary>
		/// Sets the serial command for preset 1
		/// </summary>
		[PublicAPI]
		public string Preset01SerialCommand { get { return TryGetPresetSerialCommand(1); } set { SetPresetSerialCommand(1, value); } }

		/// <summary>
		/// Sets the serial command for preset 2
		/// </summary>
		[PublicAPI]
		public string Preset02SerialCommand { get { return TryGetPresetSerialCommand(2); } set { SetPresetSerialCommand(2, value); } }

		/// <summary>
		/// Sets the serial command for preset 3
		/// </summary>
		[PublicAPI]
		public string Preset03SerialCommand { get { return TryGetPresetSerialCommand(3); } set { SetPresetSerialCommand(3, value); } }

		/// <summary>
		/// Sets the serial command for preset 4
		/// </summary>
		[PublicAPI]
		public string Preset04SerialCommand { get { return TryGetPresetSerialCommand(4); } set { SetPresetSerialCommand(4, value); } }

		/// <summary>
		/// Sets the serial command for preset 5
		/// </summary>
		[PublicAPI]
		public string Preset05SerialCommand { get { return TryGetPresetSerialCommand(5); } set { SetPresetSerialCommand(5, value); } }

		/// <summary>
		/// Sets the serial command for preset 6
		/// </summary>
		[PublicAPI]
		public string Preset06SerialCommand { get { return TryGetPresetSerialCommand(6); } set { SetPresetSerialCommand(6, value); } }

		/// <summary>
		/// Sets the serial command for preset 7
		/// </summary>
		[PublicAPI]
		public string Preset07SerialCommand { get { return TryGetPresetSerialCommand(7); } set { SetPresetSerialCommand(7, value); } }

		/// <summary>
		/// Sets the serial command for preset 8
		/// </summary>
		[PublicAPI]
		public string Preset08SerialCommand { get { return TryGetPresetSerialCommand(8); } set { SetPresetSerialCommand(8, value); } }

		/// <summary>
		/// Sets the serial command for preset 9
		/// </summary>
		[PublicAPI]
		public string Preset09SerialCommand { get { return TryGetPresetSerialCommand(9); } set { SetPresetSerialCommand(9, value); } }

		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		public SerialLightingRoomInterfaceDeviceSettings()
		{
			m_NetworkProperties = new SecureNetworkProperties();
			m_ComSpecProperties = new ComSpecProperties();
			m_Presets = new Dictionary<int, PresetSerialLightingControl>();
		}

		#region Methods

		/// <summary>
		/// Gets the name of the given preset
		/// If preset is not in the collection, return null
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		[CanBeNull]
		private string TryGetPresetName(int index)
		{
			PresetSerialLightingControl preset;
			if (m_Presets.TryGetValue(index, out preset))
				return preset.Name;
			return null;
		}

		/// <summary>
		/// Sets the preset name
		/// If the name is null or empty, removes the preset from the collection
		/// </summary>
		/// <param name="index"></param>
		/// <param name="name"></param>
		private void SetPresetName(int index, [CanBeNull] string name)
		{
			PresetSerialLightingControl preset;
			
			// Try to get existing preset
			if (!m_Presets.TryGetValue(index, out preset))
			{
				// If the name is empty, don't bother creating a new preset for it
				if (String.IsNullOrEmpty(name))
					return;
				// Create new preset
				preset = new PresetSerialLightingControl(index);
				m_Presets.Add(index,preset);
			}
			// If name is null or empty, remove the item from the collection
			else if (string.IsNullOrEmpty(name))
			{
				m_Presets.Remove(index);
				return;
			}
			// Update the name
			preset.Name = name;
		}

		/// <summary>
		/// Gets the serial command for the given preset
		/// If preset is not in the collection, returns null
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		private string TryGetPresetSerialCommand(int index)
		{
			PresetSerialLightingControl preset;
			if (m_Presets.TryGetValue(index, out preset))
				return preset.SerialCommand;
			return null;
		}

		/// <summary>
		/// Sets the preset serial command
		/// If the command is null or empty, removes the preset from the collection
		/// </summary>
		/// <param name="index"></param>
		/// <param name="serialCommand"></param>
		private void SetPresetSerialCommand(int index, [CanBeNull] string serialCommand)
		{
			PresetSerialLightingControl preset;

			// Try to get existing preset
			if (!m_Presets.TryGetValue(index, out preset))
			{
				// If the name is empty, don't bother creating a new preset for it
				if (String.IsNullOrEmpty(serialCommand))
					return;
				// Create new preset
				preset = new PresetSerialLightingControl(index);
				m_Presets.Add(index, preset);
			}
			// If name is null or empty, remove the item from the collection
			else if (string.IsNullOrEmpty(serialCommand))
			{
				m_Presets.Remove(index);
				return;
			}
			// Update the name
			preset.SerialCommand = serialCommand;
		}

		/// <summary>
		/// Replaces the preset collection with the given presets
		/// </summary>
		/// <param name="presets"></param>
		public void SetPresets(IEnumerable<PresetSerialLightingControl> presets)
		{
			m_Presets.Clear();
			m_Presets.AddRange(presets, p => p.Index);
		}

		#endregion

		#region XML

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(PORT_ELEMENT, IcdXmlConvert.ToString(Port));

			m_NetworkProperties.WriteElements(writer);
			m_ComSpecProperties.WriteElements(writer);

			XmlUtils.WriteListToXml(writer, Presets, PRESETS_ELEMENT, (w, p) => p.WriteXml(w, PRESET_ELEMENT));
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			Port = XmlUtils.TryReadChildElementContentAsInt(xml, PORT_ELEMENT);

			m_NetworkProperties.ParseXml(xml);
			m_ComSpecProperties.ParseXml(xml);

			IEnumerable<PresetSerialLightingControl> presets = XmlUtils.ReadListFromXml(xml, PRESETS_ELEMENT, PRESET_ELEMENT,
			                                                                            s => PresetSerialLightingControl.FromXml(s));

			SetPresets(presets);
		}

		#endregion
	}
}