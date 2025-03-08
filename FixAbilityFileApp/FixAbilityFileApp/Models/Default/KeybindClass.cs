using Newtonsoft.Json;

namespace AbilityTrackerLibrary;

public class KeybindClass
{
    public KeybindClass() { }

    public KeybindClass(string key, string modifier, BarClass bar, Ability ability)
    {
        Modifier = modifier;
        Key = key;
        Bar = bar;
        Ability = ability;
    }

    public string Modifier { get; set; }
    public string Key { get; set; }
    public BarClass Bar { get; set; }
    public Ability Ability { get; set; }
    [JsonIgnore] public int Dependencies { get; set; } = 0;
    [JsonIgnore] public int Occurances { get; set; } = 0;
    [JsonIgnore] public bool IsActivatedForCurrentProfile { get; set; } = false;
}
