using System;
using ICD.Common.Properties;
using ICD.Common.Utils.Xml;

namespace ICD.Connect.Lighting.SerialLightingRoomInterfaceDevice
{
	/// <summary>
	/// Represents a preset for a SerialLightingRoomInterfaceDevice
	/// </summary>
	/// <remarks>
	/// Not a Krang Control
	/// </remarks>
	public sealed class PresetSerialLightingControl : AbstractSerialLightingControl
	{
		private const string NAME_ELEMENT = "Name";
		private const string SERIAL_COMMAND_ELEMENT = "SerialCommand";

		/// <summary>
		/// Name of the preset
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Serial command to send when recalling the preset
		/// </summary>
		public string SerialCommand { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="index"></param>
		public PresetSerialLightingControl(int index) : base(index)
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="index"></param>
		/// <param name="name"></param>
		/// <param name="serialCommand"></param>
		public PresetSerialLightingControl(int index, string name, string serialCommand) : this(index)
		{
			Name = name;
			SerialCommand = serialCommand;
		}

		/// <summary>
		/// Returns a new control from XML
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		public static PresetSerialLightingControl FromXml(string xml)
		{
			int index = GetIndexFromXml(xml);
			string name = XmlUtils.TryReadChildElementContentAsString(xml, NAME_ELEMENT) ?? string.Empty;
			string serialCommand = XmlUtils.TryReadChildElementContentAsString(xml, SERIAL_COMMAND_ELEMENT) ?? string.Empty;

			return new PresetSerialLightingControl(index, name, serialCommand);
		}

		/// <summary>
		/// Writes the inner XML for the element
		/// Override to add additional XML elements
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteInnerElements(IcdXmlTextWriter writer)
		{
			base.WriteInnerElements(writer);

			writer.WriteElementString(NAME_ELEMENT, IcdXmlConvert.ToString(Name));
			writer.WriteElementString(SERIAL_COMMAND_ELEMENT, IcdXmlConvert.ToString(SerialCommand));
		}
	}

	public static class SerialLightingPresetExtensions
	{
		/// <summary>
		/// Returns a LightingProcessorControl for this preset
		/// </summary>
		/// <param name="extends"></param>
		/// <returns></returns>
		public static LightingProcessorControl ToLightingProcessorControl([NotNull] this PresetSerialLightingControl extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return LightingProcessorControl.Preset(extends.Index, 0, extends.Name);
		}
	}
}