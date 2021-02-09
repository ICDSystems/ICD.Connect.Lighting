using System;
using ICD.Connect.Lighting.Lutron.Nwk.Devices.AbstractLutronNwkDevice;
using ICD.Connect.Lighting.Lutron.Nwk.Devices.GrafikEyeNwk;
using ICD.Connect.Lighting.Lutron.Nwk.Integrations.Abstracts;
using ICD.Connect.Lighting.Lutron.Nwk.Integrations.Interfaces;
using ICD.Connect.Lighting.Shades;

namespace ICD.Connect.Lighting.Lutron.Nwk.Integrations.GrafikEyeIntegrations
{
	public sealed class GrafikEyeShadeIntegration : AbstractGrafikEyeIntegration, IShadeIntegration
	{
		/// <summary>
		/// Specifies the type of shade this is
		/// </summary>
		public eShadeType ShadeType { get; private set; }

		private GrafikEyeShadeIntegration(int integrationId, string name, ILutronNwkDevice parent, GrafikEyeRoom room, eShadeType shadeType) : base(integrationId, name, parent, room)
		{
			ShadeType = shadeType;
		}

		public static GrafikEyeShadeIntegration FromXml(string xml, GrafikEyeRoom room, ILutronNwkDevice parent)
		{
			int integrationId = GetIntegrationIdIntFromXml(xml);
			string name = GetNameFromXml(xml);

			eShadeType shadeType = AbstractShadeIntegration.GetShadeTypeFromXml(xml);

			return new GrafikEyeShadeIntegration(integrationId, name, parent, room, shadeType);
		}

		/// <summary>
		/// Starts raising the shade/s.
		/// </summary>
		public void StartRaising()
		{
			Room.GrafikEye.ShadeStartRaising(IntegrationId);
		}

		/// <summary>
		/// Starts lowering the shade/s.
		/// </summary>
		public void StartLowering()
		{
			Room.GrafikEye.ShadeStartLowering(IntegrationId);
		}

		/// <summary>
		/// Stops raising or lowering the shade/s.
		/// </summary>
		public void StopMoving()
		{
			//Not support in GrafikEye?
		}
	}
}
