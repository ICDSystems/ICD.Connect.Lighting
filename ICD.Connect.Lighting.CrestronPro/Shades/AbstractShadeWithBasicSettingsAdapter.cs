namespace ICD.Connect.Lighting.CrestronPro.Shades
{
#if SIMPLSHARP
	public abstract class AbstractShadeWithBasicSettingsAdapter<TShade, TSettings> : AbstractShadeBaseAdapter<TShade, TSettings>, IShadeWithBasicSettingsAdapter
		where TShade : Crestron.SimplSharpPro.Shades.ShadeWithBasicSettings
#else
	public abstract class AbstractShadeWithBasicSettingsAdapter<TSettings> : AbstractShadeBaseAdapter<TSettings>, IShadeWithBasicSettingsAdapter
#endif
		where TSettings : IShadeWithBasicSettingsAdapterSettings, new()
	{
		
	}
}
