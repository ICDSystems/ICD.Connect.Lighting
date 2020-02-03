using System;
using System.Collections.Generic;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Xml;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Panels.EventArguments;
using ICD.Connect.Protocol.Sigs;

namespace ICD.Connect.Lighting.CrestronPro.EiscLightingAdapter.Components
{
	public sealed class LoadRoomComponent : AbstractEiscLightingComponent
	{
		private const float TOLERANCE = 0.001f;

		private const string LEVEL_ANALOG_JOIN_ELEMENT = "LevelAnalogJoin";
		private const string RAISE_DIGITAL_JOIN_ELEMENT = "RaiseDigitalJoin";
		private const string LOWER_DIGITAL_JOIN_ELEMENT = "LowerDigitalJoin";

		public event EventHandler<FloatEventArgs> OnLevelChanged;

		private float m_Level;

		public uint? LevelAnalogJoin { get; private set; }
		public uint? RaiseDigitalJoin { get; private set; }
		public uint? LowerDigitalJoin { get; private set; }

		public float Level
		{
			get { return m_Level; }
			private set
			{
				if (Math.Abs(m_Level - value) < TOLERANCE)
					return;

				m_Level = value;

                OnLevelChanged.Raise(this, new FloatEventArgs(value));             
			}
		}

		protected override LightingProcessorControl.ePeripheralType ControlType { get { return LightingProcessorControl.ePeripheralType.Load; } }

		private LoadRoomComponent(EiscLightingRoom room, int id, string name) : base(room, id, name)
		{
		}

		public static LoadRoomComponent FromXml(string xml, EiscLightingRoom room)
		{
			int id = GetId(xml);
			string name = GetName(xml);

			uint? levelAnalogJoin = XmlUtils.TryReadChildElementContentAsUInt(xml, LEVEL_ANALOG_JOIN_ELEMENT);
			uint? raiseDigitalJoin = XmlUtils.TryReadChildElementContentAsUInt(xml, RAISE_DIGITAL_JOIN_ELEMENT);
			uint? lowerDigitalJoin = XmlUtils.TryReadChildElementContentAsUInt(xml, LOWER_DIGITAL_JOIN_ELEMENT);

			var load = new LoadRoomComponent(room, id, name)
			{
				LevelAnalogJoin = levelAnalogJoin,
				RaiseDigitalJoin = raiseDigitalJoin,
				LowerDigitalJoin = lowerDigitalJoin
			};

			load.Register();

			return load;
		}

		protected override void Register()
		{

			if (!LevelAnalogJoin.HasValue)
				return;

			SigInfo currentValue = Room.RegisterSigChangeCallback(LevelAnalogJoin.Value, eSigType.Analog, (m,a) => LevelChangeSigCallback(a));

			Level = ConvertToPercentage(currentValue.GetUShortValue());
		}

		protected override void Unregister()
		{
			if (LevelAnalogJoin.HasValue)
				Room.UnregisterSigChangeCallback(LevelAnalogJoin.Value, eSigType.Analog, (m, a) => LevelChangeSigCallback(a));
		}

		private void LevelChangeSigCallback(SigInfoEventArgs args)
		{
			Level = ConvertToPercentage(args.Data.GetUShortValue());
		}

		public void SetLevel(float percentage)
		{
			if (LevelAnalogJoin.HasValue)
				Room.SendAnalogJoin(LevelAnalogJoin.Value, ConvertToUshort(percentage));
		}

		public void StartRaising()
		{
			if (RaiseDigitalJoin.HasValue)
				Room.SendDigitalJoin(RaiseDigitalJoin.Value, true);
		}

		public void StartLowering()
		{
			if (LowerDigitalJoin.HasValue)
				Room.SendDigitalJoin(LowerDigitalJoin.Value, true);
		}

		public void StopRamping()
		{
			if (RaiseDigitalJoin.HasValue)
				Room.SendDigitalJoin(RaiseDigitalJoin.Value, false);
			if (LowerDigitalJoin.HasValue)
				Room.SendDigitalJoin(LowerDigitalJoin.Value, false);
		}

		private static ushort ConvertToUshort(float percentage)
		{
			return (ushort)MathUtils.MapRange(0f, 1f, 0f, 65535f, percentage);
		}

		private static float ConvertToPercentage(ushort value)
		{
			return MathUtils.MapRange(0f, 65535f, 0f, 1f, value);
		}

		public override void Dispose()
		{
			base.Dispose();

			OnLevelChanged = null;
		}

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			addRow("Level", Level);
			addRow("LevelAnalogJoin", LevelAnalogJoin);
			addRow("RaiseDigitalJoin", RaiseDigitalJoin);
			addRow("LowerDigitalJoin", LowerDigitalJoin);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return
				new GenericConsoleCommand<float>("SetLevel", "SetLevel <float> - level between 0 and 1", l => SetLevel(l));
		}

		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		#endregion
	}
}