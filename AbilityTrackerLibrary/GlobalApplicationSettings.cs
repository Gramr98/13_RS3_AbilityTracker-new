namespace AbilityTrackerLibrary;

public static class GlobalApplicationSettings
{
    public static class FilePathSettings
    {
        public static readonly string StartJsonFile = @".\Data\StartSettings.json";
        public static readonly string AbilityImages = @".\Images\Abilities";
        public static readonly string AbilityJsonFile = @".\Data\Abilities.json";
        public static readonly string BarImages = @".\Images\Bars";
        public static readonly string BarJsonFile = @".\Data\Bars.json";
        public static readonly string BarKeybindJsonFile = @".\Data\BarKeybindings.json";
        public static readonly string AbilityKeybindJsonFile = @".\Data\Abilitybindings.json";
        public static readonly string KeybindForAllBarsImage = @".\Images\BarIcon_AllStyles.png";
        public static readonly string ProfileJsonFile = @".\Data\Profiles.json";
        //public static readonly string AbilityProfileJsonFile = @".\Data\AbilitiesProfiles.json";
    }
    public static class OtherSettings
    {
        public static readonly string AllBarsName = "All Styles";
        public static readonly string UserAgent_WikiAPICalls = "PostmanRuntime/7.26.1"; //public static readonly string UserAgent_WikiAPICalls = "Custom Ability Tracker C# .NET 6.0";

        public static readonly string StartLoggingPath = @".\StartLogger.txt";
    }
}
