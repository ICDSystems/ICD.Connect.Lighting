using ICD.Connect.Lighting.Shades;

namespace ICD.Connect.Lighting.Environment
{
	public interface IShadeEnvironmentPeripheral : IEnvironmentPeripheral
	{
		eShadeDirection LastDirection { get; }

		/// <summary>
		/// Starts raising the shade.
		/// </summary>
		void StartRaising();

		/// <summary>
		/// Starts lowering the shade.
		/// </summary>
		void StartLowering();

		/// <summary>
		/// Stops moving the shade.
		/// </summary>
		void StopMoving();
	}
}