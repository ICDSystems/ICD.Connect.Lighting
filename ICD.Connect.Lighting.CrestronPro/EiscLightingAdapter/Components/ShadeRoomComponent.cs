using System;
using ICD.Common.Utils.Timers;
using ICD.Common.Utils.Xml;
using ICD.Connect.Lighting.Shades;

namespace ICD.Connect.Lighting.CrestronPro.EiscLightingAdapter.Components
{
	public sealed class ShadeRoomComponent : AbstractEiscLightingComponent
	{
		private const int DEFAULT_PULSE_TIME = 250;

		private const string OPEN_DIGITAL_JOIN_ELEMENT = "OpenDigitalJoin";
		private const string CLOSE_DIGITAL_JOIN_ELEMENT = "CloseDigitalJoin";
		private const string STOP_DIGITAL_JOIN_ELEMENT = "StopDitialJoin";
		private const string SHADE_TYPE_ELEMENT = "ShadeType";
		private const string PULSE_TIME_ELEMENT = "PulseTime";

		private readonly SafeTimer m_OpenPulseTimer;
		private readonly SafeTimer m_ClosePulseTimer;
		private readonly SafeTimer m_StopPulseTimer;

		protected override LightingProcessorControl.ePeripheralType ControlType { get { return LightingProcessorControl.ePeripheralType.Shade; } }

		public uint OpenDigitalJoin { get; private set; }
		public uint CloseDigitalJoin { get; private set; }
		public uint? StopDigitalJoin { get; private set; }
		public eShadeType ShadeType { get; private set; }
		public int PulseTime { get; private set; }

		public ShadeRoomComponent(EiscLightingRoom room, int id, string name) : base(room, id, name)
		{
			m_OpenPulseTimer = SafeTimer.Stopped(OpenSendFalse);
			m_ClosePulseTimer = SafeTimer.Stopped(CloseSendFalse);
			m_StopPulseTimer = SafeTimer.Stopped(StopSendFalse);
		}

		public static ShadeRoomComponent FromXml(string xml, EiscLightingRoom room)
		{
			int id = GetId(xml);
			string name = GetName(xml);

			uint? openDigitalJoin = XmlUtils.TryReadChildElementContentAsUInt(xml, OPEN_DIGITAL_JOIN_ELEMENT);
			uint? closeDigitalJoin = XmlUtils.TryReadChildElementContentAsUInt(xml, CLOSE_DIGITAL_JOIN_ELEMENT);
			uint? stopDigitalJoin = XmlUtils.TryReadChildElementContentAsUInt(xml, STOP_DIGITAL_JOIN_ELEMENT);
			int? pulseTime = XmlUtils.TryReadChildElementContentAsInt(xml, PULSE_TIME_ELEMENT);

			eShadeType shadeType;

			if (!XmlUtils.TryReadChildElementContentAsEnum(xml, SHADE_TYPE_ELEMENT, true, out shadeType))
				shadeType = eShadeType.None;

			if (!openDigitalJoin.HasValue)
				throw new InvalidOperationException(String.Format("Shade {0}-{1} must have open join defined", id, name));

			if (!closeDigitalJoin.HasValue)
				throw new InvalidOperationException(String.Format("Shade {0}-{1} must have close join defined", id, name));

			var load = new ShadeRoomComponent(room, id, name)
			{
				OpenDigitalJoin = openDigitalJoin.Value,
				CloseDigitalJoin = closeDigitalJoin.Value,
				StopDigitalJoin = stopDigitalJoin,
				PulseTime = pulseTime ?? DEFAULT_PULSE_TIME,
				ShadeType = shadeType
			};

			load.Register();

			return load;
		}

		public override LightingProcessorControl ToLightingProcessorControl()
		{
			return new LightingProcessorControl(ControlType, Id, Room.Id, Name, ShadeType);
		}

		public void OpenShade()
		{
			Room.SendDigitalJoin(OpenDigitalJoin, true);
			m_OpenPulseTimer.Reset(PulseTime);
		}

		private void OpenSendFalse()
		{
			Room.SendDigitalJoin(OpenDigitalJoin, false);
		}

		public void CloseShade()
		{
			Room.SendDigitalJoin(CloseDigitalJoin, true);
			m_ClosePulseTimer.Reset(PulseTime);
		}

		private void CloseSendFalse()
		{
			Room.SendDigitalJoin(CloseDigitalJoin, false);
		}

		public void StopShade()
		{
			if (!StopDigitalJoin.HasValue)
				return;

			Room.SendDigitalJoin(StopDigitalJoin.Value, true);
			m_StopPulseTimer.Reset(PulseTime);
		}

		private void StopSendFalse()
		{
			if (StopDigitalJoin.HasValue)
				Room.SendDigitalJoin(StopDigitalJoin.Value, false);
		}
	}
}