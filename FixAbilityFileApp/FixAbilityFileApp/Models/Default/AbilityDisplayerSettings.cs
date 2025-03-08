namespace AbilityTrackerLibrary;

public class AbilityDisplayerSettings
{
    public AbilityDisplayerSettings(bool trackAbilityCooldown, bool trackGlobalCooldown, bool pinWindowOnTop, 
        int defaultGCD, int configuredGCD, bool hasDblSurgeAndEscape, bool hasMobilePerk)
    {
        TrackAbilityCooldown = trackAbilityCooldown;
        TrackGlobalCooldown = trackGlobalCooldown;
        PinWindowOnTop = pinWindowOnTop;
        DefaultGCD = defaultGCD;
        ConfiguredGCD = configuredGCD;
        HasDblSurgeAndEscape = hasDblSurgeAndEscape;
        HasMobilePerk = hasMobilePerk;
    }

    public bool TrackAbilityCooldown { get; set; }
    public bool TrackGlobalCooldown { get; set; }
    public bool PinWindowOnTop { get; set; }
    public int DefaultGCD { get; set; }
    public int ConfiguredGCD { get; set; }
    public bool HasDblSurgeAndEscape { get; set; }
    public bool HasMobilePerk { get; set; }

    //... bool allowAbilityQueueing ...
}
