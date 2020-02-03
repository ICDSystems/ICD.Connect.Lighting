using System;
using System.Collections.Generic;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Xml;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Panels.EventArguments;
using ICD.Connect.Protocol.Sigs;

namespace ICD.Connect.Lighting.CrestronPro.EiscLightingAdapter.Components
{
	public sealed class PresetRoomComponent : AbstractEiscLightingComponent
	{
		public event EventHandler<BoolEventArgs> OnPresetStateChanged;

		private const string DIGITAL_JOIN_ELEMENT = "DigitalJoin";

		private bool m_Active;

		protected override LightingProcessorControl.ePeripheralType ControlType { get { return LightingProcessorControl.ePeripheralType.Preset; } }

		public uint DigitalJoinNumber { get; private set; }

		public bool Active
		{
			get { return m_Active; }
			private set
			{
				if (m_Active == value)
					return;

				m_Active = value;

				OnPresetStateChanged.Raise(this, new BoolEventArgs(value));
			}
		}

		private PresetRoomComponent(EiscLightingRoom room, int id, string name) : base(room, id, name)
		{
		}

		public static PresetRoomComponent FromXml(string xml, EiscLightingRoom room)
		{
			int id = GetId(xml);
			string name = GetName(xml);

			uint digitalJoin = XmlUtils.ReadChildElementContentAsUint(xml, DIGITAL_JOIN_ELEMENT);

			var preset = new PresetRoomComponent(room, id, name)
			{
				DigitalJoinNumber = digitalJoin
			};

			preset.Register();

			return preset;
		}

		protected override void Register()
		{
			if (DigitalJoinNumber == 0)
				throw new NotSupportedException("Preset Must Have Join Number");

			SigInfo currentValue = Room.RegisterSigChangeCallback(DigitalJoinNumber, eSigType.Digital, (m,a) => DigitalJoinChangedCallback(a));

			Active = currentValue.GetBoolValue();
		}

		protected override void Unregister()
		{
			if (DigitalJoinNumber == 0)
				return;

			Room.UnregisterSigChangeCallback(DigitalJoinNumber, eSigType.Digital, (m,a) => DigitalJoinChangedCallback(a));
		}

		private void DigitalJoinChangedCallback(SigInfoEventArgs args)
		{
			Active = args.Data.GetBoolValue();
		}

		public void Activate()
		{
			Room.SendDigitalJoin(DigitalJoinNumber, true);
			Room.SendDigitalJoin(DigitalJoinNumber, false);
		}

		public override void Dispose()
		{
			base.Dispose();

			OnPresetStateChanged = null;
		}

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			addRow("Active", Active);
			addRow("DigitalJoin", DigitalJoinNumber);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new ConsoleCommand("Activate", "Activates Preset", () => Activate());
		}

		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		#endregion
	}
}