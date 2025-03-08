using Newtonsoft.Json;

namespace AbilityTrackerLibrary;

public class Profile
{
    public string ProfileName { get; set; }

    public Profile() { }

    public Profile(string profileName)
    {
        ProfileName = profileName;
    }

    public List<BarKeybindClass> ActiveBarKeybindObjects { get; set; } = new List<BarKeybindClass>();
    public List<KeybindClass> ActiveAbilityKeybindObjects { get; set; } = new List<KeybindClass>();

    [JsonIgnore] public int BarCount { get { return ActiveBarKeybindObjects.Count; } }
    [JsonIgnore] public int AbilityCount { get { return ActiveAbilityKeybindObjects.Count; } }
    [JsonIgnore] public string InfoText { get { return $"{ProfileName} ({BarCount} Bars, {AbilityCount} Keys)"; } }

    public override string ToString()
    {
        return $"{ProfileName} ({ActiveBarKeybindObjects.Count},{ActiveAbilityKeybindObjects.Count})";
    }
}
