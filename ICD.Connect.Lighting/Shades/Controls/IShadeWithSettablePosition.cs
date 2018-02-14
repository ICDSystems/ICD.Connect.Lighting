namespace ICD.Connect.Lighting.Shades.Controls
{
	public interface IShadeWithSettablePosition : IShadeDevice
	{
		void SetPosition(float position);
	}
}