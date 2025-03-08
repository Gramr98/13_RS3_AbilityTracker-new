namespace AbilityTrackerLibrary;

public class AbilityDisplayerSettings
{
    public AbilityDisplayerSettings(bool trackAbilityCooldown, bool trackGlobalCooldown, bool pinWindowOnTop, 
        int defaultGCD, int configuredGCD, bool hasDblSurgeAndEscape, bool hasMobilePerk, bool displayBleedTimerForMagicT95DW, int bleedStartForMagicDWT95TimerInS)
    {
        TrackAbilityCooldown = trackAbilityCooldown;
        TrackGlobalCooldown = trackGlobalCooldown;
        PinWindowOnTop = pinWindowOnTop;
        DefaultGCD = defaultGCD;
        ConfiguredGCD = configuredGCD;
        HasDblSurgeAndEscape = hasDblSurgeAndEscape;
        HasMobilePerk = hasMobilePerk;
        DisplayBleedTimerForMagicT95DW = displayBleedTimerForMagicT95DW;
        BleedStartForMagicDWT95TimerInS = bleedStartForMagicDWT95TimerInS;
    }

    public bool TrackAbilityCooldown { get; set; }
    public bool TrackGlobalCooldown { get; set; }
    public bool PinWindowOnTop { get; set; }
    public int DefaultGCD { get; set; }
    public int ConfiguredGCD { get; set; }
    public bool HasDblSurgeAndEscape { get; set; }
    public bool HasMobilePerk { get; set; }
    public bool DisplayBleedTimerForMagicT95DW { get; set; }
    public int BleedStartForMagicDWT95TimerInS { get; set; }

    //... bool allowAbilityQueueing ...
}
