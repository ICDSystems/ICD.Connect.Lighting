using ICD.Connect.Lighting.Shades.Controls;

namespace ICD.Connect.Lighting.CrestronPro.Shades
{
	public interface IShadeBaseAdapter 
		: IShadeWithStop,
		  IShadeWithLastDirectionFeedback,
		  IShadeWithInMotionFeedback,
		  IShadeWithPositionFeedback,
		  IShadeWithSettablePosition
	{
		 
	}
}