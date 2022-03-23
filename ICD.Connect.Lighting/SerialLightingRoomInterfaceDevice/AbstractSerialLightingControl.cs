using ICD.Common.Utils.Xml;

namespace ICD.Connect.Lighting.SerialLightingRoomInterfaceDevice
{
	/// <summary>
	/// Abstract control for a serial lighting room interface device
	/// </summary>
	/// <remarks>
	/// Not a Krang Control
	/// </remarks>
	public abstract class AbstractSerialLightingControl
	{
		private const string INDEX_ELEMENT = "Index";

		/// <summary>
		/// Index of the control
		/// </summary>
		public int Index { get; private set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="index"></param>
		protected AbstractSerialLightingControl(int index)
		{
			Index = index;
		}

		/// <summary>
		/// Gets the index from the XML element
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		protected static int GetIndexFromXml(string xml)
		{
			return XmlUtils.TryReadChildElementContentAsInt(xml, INDEX_ELEMENT) ?? 0;
		}

		/// <summary>
		/// Writes XML for the control, using the given element name
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="element"></param>
		public void WriteXml(IcdXmlTextWriter writer, string element)
		{
			writer.WriteStartElement(element);
			{
				WriteInnerElements(writer);
			}
			writer.WriteEndElement();
		}

		/// <summary>
		/// Writes the inner XML for the element
		/// Override to add additional XML elements
		/// </summary>
		/// <param name="writer"></param>
		protected virtual void WriteInnerElements(IcdXmlTextWriter writer)
		{
			writer.WriteElementString(INDEX_ELEMENT, IcdXmlConvert.ToString(Index));
		}
	}
}