namespace ICD.Connect.Lighting.CrestronPro.Shades
{
	public abstract class AbstractShadeWithBasicSettingsAdapter<TShade, TSettings>
		: AbstractShadeBaseAdapter<TShade, TSettings>, IShadeWithBasicSettingsAdapter
		where TShade : Crestron.SimplSharpPro.Shades.ShadeWithBasicSettings
		where TSettings : IShadeWithBasicSettingsAdapterSettings, new()
	{
		
	}
}
