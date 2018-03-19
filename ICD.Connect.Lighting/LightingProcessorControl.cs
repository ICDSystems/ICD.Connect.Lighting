using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Connect.Lighting.Shades;
using Newtonsoft.Json;

namespace ICD.Connect.Lighting
{
	public struct LightingProcessorControl
	{
		public enum ePeripheralType
		{
			[PublicAPI] Load,
			[PublicAPI] Shade,
			[PublicAPI] ShadeGroup,
			[PublicAPI] Preset
		}

		private readonly ePeripheralType m_PeripheralType;
		private readonly eShadeType m_ShadeType;
		private readonly int m_Id;
		private readonly int m_Room;
		private readonly string m_Name;

		#region Properties

		/// <summary>
		/// Gets the type of lighting control.
		/// </summary>
		[PublicAPI]
		public ePeripheralType PeripheralType { get { return m_PeripheralType; } }

		/// <summary>
		/// Gets the id of the control.
		/// </summary>
		public int Id { get { return m_Id; } }

		/// <summary>
		/// Gets the id of the parent room.
		/// </summary>
		public int Room { get { return m_Room; } }

		/// <summary>
		/// Gets the name of the control.
		/// </summary>
		[PublicAPI]
		public string Name { get { return m_Name; } }

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="peripheralType"></param>
		/// <param name="id"></param>
		/// <param name="room"></param>
		/// <param name="name"></param>
		[JsonConstructor]
		public LightingProcessorControl(ePeripheralType peripheralType, int id, int room, string name)
		{
			m_PeripheralType = peripheralType;
			m_Id = id;
			m_Room = room;
			m_Name = name;
			m_ShadeType = eShadeType.None;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="peripheralType"></param>
		/// <param name="id"></param>
		/// <param name="room"></param>
		/// <param name="name"></param>
		/// <param name="shadeType"></param>
		[JsonConstructor]
		public LightingProcessorControl(ePeripheralType peripheralType, int id, int room, string name, eShadeType shadeType)
		{
			m_PeripheralType = peripheralType;
			m_Id = id;
			m_Room = room;
			m_Name = name;

			if (peripheralType != ePeripheralType.Shade && peripheralType != ePeripheralType.ShadeGroup)
				m_ShadeType = eShadeType.None;
			else
				m_ShadeType = shadeType;
		}

		/// <summary>
		/// Creates a load control.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="room"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		[PublicAPI]
		public static LightingProcessorControl Load(int id, int room, string name)
		{
			return new LightingProcessorControl(ePeripheralType.Load, id, room, name);
		}

		/// <summary>
		/// Creates a shade control.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="room"></param>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		[PublicAPI]
		public static LightingProcessorControl Shade(int id, int room, string name, eShadeType type)
		{
			return new LightingProcessorControl(ePeripheralType.Shade, id, room, name, type);
		}

		/// <summary>
		/// Creates a shade group control.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="room"></param>
		/// <param name="name"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		[PublicAPI]
		public static LightingProcessorControl ShadeGroup(int id, int room, string name, eShadeType type)
		{
			return new LightingProcessorControl(ePeripheralType.ShadeGroup, id, room, name, type);
		}

		/// <summary>
		/// Creates a preset control.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="room"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		[PublicAPI]
		public static LightingProcessorControl Preset(int id, int room, string name)
		{
			return new LightingProcessorControl(ePeripheralType.Preset, id, room, name);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Gets the string representation for this instance.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			ReprBuilder builder = new ReprBuilder(this);
			
			builder.AppendProperty("Id", m_Id);
			builder.AppendProperty("Type", m_PeripheralType);
			builder.AppendProperty("Room", m_Room);
			builder.AppendProperty("Name", m_Name);

			return builder.ToString();
		}

		/// <summary>
		/// Implementing default equality.
		/// </summary>
		/// <param name="s1"></param>
		/// <param name="s2"></param>
		/// <returns></returns>
		public static bool operator ==(LightingProcessorControl s1, LightingProcessorControl s2)
		{
			return s1.Equals(s2);
		}

		/// <summary>
		/// Implementing default inequality.
		/// </summary>
		/// <param name="s1"></param>
		/// <param name="s2"></param>
		/// <returns></returns>
		public static bool operator !=(LightingProcessorControl s1, LightingProcessorControl s2)
		{
			return !(s1 == s2);
		}

		/// <summary>
		/// Returns true if this instance is equal to the given object.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public override bool Equals(object other)
		{
			if (other == null || GetType() != other.GetType())
				return false;

			return GetHashCode() == ((LightingProcessorControl)other).GetHashCode();
		}

		/// <summary>
		/// Gets the hashcode for this instance.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + (int)m_PeripheralType;
				hash = hash * 23 + m_Id;
				hash = hash * 23 + (m_Name == null ? 0 : m_Name.GetHashCode());
				return hash;
			}
		}

		#endregion
	}
}
