using ICD.Connect.Lighting.Shades;

namespace ICD.Connect.Lighting.Lutron.Nwk.Integrations.Interfaces
{
	public interface IShadeIntegration : IIntegration<int>
	{
		/// <summary>
		/// Specifies the type of shade this is
		/// </summary>
		eShadeType ShadeType { get; }

		/// <summary>
		/// Starts raising the shade/s.
		/// </summary>
		void StartRaising();

		/// <summary>
		/// Starts lowering the shade/s.
		/// </summary>
		void StartLowering();

		/// <summary>
		/// Stops raising or lowering the shade/s.
		/// </summary>
		void StopMoving();
	}
}