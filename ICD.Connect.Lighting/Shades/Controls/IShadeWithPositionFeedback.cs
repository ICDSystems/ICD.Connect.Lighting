namespace ICD.Connect.Lighting.Shades.Controls
{
	public interface IShadeWithPositionFeedback : IShadeDevice
	{
		float GetPosition();
	}
}