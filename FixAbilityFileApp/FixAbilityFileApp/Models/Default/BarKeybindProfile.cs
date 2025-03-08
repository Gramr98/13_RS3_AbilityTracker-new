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

    [JsonIgnore] public int ObjectCount { get { return ActiveBarKeybindObjects.Count; } }
}
