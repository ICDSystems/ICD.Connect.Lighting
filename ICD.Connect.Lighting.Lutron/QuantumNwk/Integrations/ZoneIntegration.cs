using System;
using ICD.Common.EventArguments;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;

namespace ICD.Connect.Lighting.Lutron.QuantumNwk.Integrations
{
	public sealed class ZoneIntegration : AbstractIntegration
	{
		private const int ACTION_OUTPUT_LEVEL = 1;
		private const int ACTION_START_RAISING = 2;
		private const int ACTION_START_LOWERING = 3;
		private const int ACTION_STOP_RAISING_LOWERING = 4;

		/// <summary>
		/// Raised when the light output level changes.
		/// </summary>
		[PublicAPI]
		public event EventHandler<FloatEventArgs> OnOutputLevelChanged;

		private float m_OutputLevel;

		#region Properties

		/// <summary>
		/// Gets the light output level as a percentage 0.0f to 1.0f.
		/// </summary>
		[PublicAPI]
		public float OutputLevel
		{
			get { return m_OutputLevel; }
			private set
			{
				if (Math.Abs(value - m_OutputLevel) < LutronUtils.FLOAT_TOLERANCE)
					return;

				m_OutputLevel = value;

				OnOutputLevelChanged.Raise(this, new FloatEventArgs(m_OutputLevel));
			}
		}

		/// <summary>
		/// The string prefix for communication with the lighting processor, e.g. SHADES.
		/// </summary>
		protected override string Command { get { return LutronUtils.COMMAND_OUTPUT; } }

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="integrationId"></param>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		private ZoneIntegration(int integrationId, string name, LutronQuantumNwkDevice parent)
			: base(integrationId, name, parent)
		{
		}

		/// <summary>
		/// Instantiates a ZoneIntegration from xml.
		/// </summary>
		/// <param name="xml"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static ZoneIntegration FromXml(string xml, LutronQuantumNwkDevice parent)
		{
			int integrationId = GetIntegrationIdFromXml(xml);
			string name = GetNameFromXml(xml);

			return new ZoneIntegration(integrationId, name, parent);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Sets the light level for the zone.
		/// </summary>
		/// <param name="percentage">Percentage between 0.0f and 1.0f</param>
		[PublicAPI]
		public void SetOutputLevel(float percentage)
		{
			SetOutputLevel(percentage, new TimeSpan(), new TimeSpan());
		}

		/// <summary>
		/// Sets the light level for the zone.
		/// </summary>
		/// <param name="percentage">Percentage between 0.0f and 1.0f</param>
		/// <param name="fade">Hours, minutes and seconds for the duration of the ramp</param>
		/// <param name="delay">Hours, minutes and seconds before starting to ramp</param>
		[PublicAPI]
		public void SetOutputLevel(float percentage, TimeSpan fade, TimeSpan delay)
		{
			string percentageParam = LutronUtils.FloatToPercentageParameter(percentage);
			string fadeParam = LutronUtils.TimeSpanToParameter(fade);
			string delayParam = LutronUtils.TimeSpanToParameter(delay);

			Execute(ACTION_OUTPUT_LEVEL, percentageParam, fadeParam, delayParam);
		}

		/// <summary>
		/// Starts raising the zone output level.
		/// </summary>
		[PublicAPI]
		public void StartRaising()
		{
			Execute(ACTION_START_RAISING);
		}

		/// <summary>
		/// Starts lowering the zone output level.
		/// </summary>
		[PublicAPI]
		public void StartLowering()
		{
			Execute(ACTION_START_LOWERING);
		}

		/// <summary>
		/// Stops raising/lowering the zone output level.
		/// </summary>
		[PublicAPI]
		public void StopRaisingLowering()
		{
			Execute(ACTION_STOP_RAISING_LOWERING);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Override to query the device when it goes online.
		/// </summary>
		protected override void Initialize()
		{
			base.Initialize();

			Query(ACTION_OUTPUT_LEVEL);
		}

		/// <summary>
		/// Called when we receive a response from the lighting processor for this integration.
		/// </summary>
		/// <param name="action"></param>
		/// <param name="parameters"></param>
		protected override void ParentOnResponse(int action, string[] parameters)
		{
			base.ParentOnResponse(action, parameters);

			switch (action)
			{
				case ACTION_OUTPUT_LEVEL:
					OutputLevel = LutronUtils.PercentageParameterToFloat(parameters[0]);
					break;
			}
		}

		#endregion
	}
}
