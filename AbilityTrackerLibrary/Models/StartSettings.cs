namespace AbilityTrackerLibrary;

public class StartSettings
{
    public bool TrackAbilityCD { get; set; } = true;
    public bool TrackGCD { get; set; } = true;
    public bool DisplayOnTop { get; set; } = true;
    public bool CanResize { get; set; } = false;
    public int SelectedProfileIndex { get; set; } = 0;
    public int SelectedBarIndex { get; set; } = 0;
    public bool HasDoubleSurgeOrEscape { get; set; } = true;
    public string GCDReductionText { get; set; } = String.Empty;
    public bool HasMobilePerkOrRelic { get; set; } = true;
    public bool DisplayBleedTimerForMagicT95DW { get; set; } = true;
    public string BleedTimerForMagicT95DW { get; set; } = String.Empty;
}
