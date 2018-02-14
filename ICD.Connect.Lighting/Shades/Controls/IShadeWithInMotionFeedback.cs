namespace ICD.Connect.Lighting.Shades.Controls
{
	public interface IShadeWithInMotionFeedback : IShadeDevice
	{
		bool GetIsInMotion();
	}
}