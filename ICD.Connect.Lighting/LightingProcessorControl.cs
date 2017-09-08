using ICD.Common.Properties;
using ICD.Common.Utils;
using Newtonsoft.Json;

namespace ICD.Connect.Lighting
{
	public struct LightingProcessorControl
	{
		public enum eControlType
		{
			[PublicAPI] Load,
			[PublicAPI] Shade,
			[PublicAPI] ShadeGroup,
			[PublicAPI] Preset
		}

		private readonly eControlType m_ControlType;
		private readonly int m_Id;
		private readonly int m_Room;
		private readonly string m_Name;

		#region Properties

		/// <summary>
		/// Gets the type of lighting control.
		/// </summary>
		[PublicAPI]
		public eControlType ControlType { get { return m_ControlType; } }

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
		/// <param name="controlType"></param>
		/// <param name="id"></param>
		/// <param name="room"></param>
		/// <param name="name"></param>
		[JsonConstructor]
		public LightingProcessorControl(eControlType controlType, int id, int room, string name)
		{
			m_ControlType = controlType;
			m_Id = id;
			m_Room = room;
			m_Name = name;
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
			return new LightingProcessorControl(eControlType.Load, id, room, name);
		}

		/// <summary>
		/// Creates a shade control.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="room"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		[PublicAPI]
		public static LightingProcessorControl Shade(int id, int room, string name)
		{
			return new LightingProcessorControl(eControlType.Shade, id, room, name);
		}

		/// <summary>
		/// Creates a shade group control.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="room"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		[PublicAPI]
		public static LightingProcessorControl ShadeGroup(int id, int room, string name)
		{
			return new LightingProcessorControl(eControlType.ShadeGroup, id, room, name);
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
			return new LightingProcessorControl(eControlType.Preset, id, room, name);
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
			builder.AppendProperty("Type", m_ControlType);
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
				hash = hash * 23 + (int)m_ControlType;
				hash = hash * 23 + m_Id;
				hash = hash * 23 + (m_Name == null ? 0 : m_Name.GetHashCode());
				return hash;
			}
		}

		#endregion
	}
}
