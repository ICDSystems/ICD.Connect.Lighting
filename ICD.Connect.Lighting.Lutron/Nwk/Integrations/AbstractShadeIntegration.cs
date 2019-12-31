﻿using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Xml;
using ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice;
using ICD.Connect.Lighting.Shades;

namespace ICD.Connect.Lighting.Lutron.Nwk.Integrations
{
	/// <summary>
	/// Base class for shade integrations.
	/// </summary>
	public abstract class AbstractShadeIntegration : AbstractIntegrationWithoutComponent
	{
		private const int ACTION_RAISE = 2;
		private const int ACTION_LOWER = 3;
		private const int ACTION_STOP_RAISING_LOWERING = 4;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="integrationId"></param>
		/// <param name="name"></param>
		/// <param name="parent"></param>
		protected AbstractShadeIntegration(int integrationId, string name, ILutronNwkDevice parent)
			: base(integrationId, name, parent)
		{
		}

		#region Methods

		/// <summary>
		/// Starts raising the shade/s.
		/// </summary>
		[PublicAPI]
		public void StartRaising()
		{
			Execute(ACTION_RAISE);
		}

		/// <summary>
		/// Starts lowering the shade/s.
		/// </summary>
		[PublicAPI]
		public void StartLowering()
		{
			Execute(ACTION_LOWER);
		}

		/// <summary>
		/// Stops raising or lowering the shade/s.
		/// </summary>
		[PublicAPI]
		public void StopMoving()
		{
			Execute(ACTION_STOP_RAISING_LOWERING);
		}

		#endregion

		protected static eShadeType GetShadeTypeFromXml(string xml)
		{
			eShadeType shadeType;
			bool parsed = EnumUtils.TryParseStrict(XmlUtils.GetAttributeAsString(xml, "shadeType"), true, out shadeType);
			return parsed ? shadeType : eShadeType.None;
		}
	}
}
