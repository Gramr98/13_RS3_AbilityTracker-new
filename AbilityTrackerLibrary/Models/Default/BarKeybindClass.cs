using Newtonsoft.Json;

namespace AbilityTrackerLibrary;

public class BarKeybindClass
{
    public BarKeybindClass() { }

    public BarKeybindClass(string key, string modifier, BarClass bar, BarClass barDependency)
    {
        Modifier = modifier;
        Key = key;
        Bar = bar;
        BarDependency = barDependency;
    }

    public string Modifier { get; set; }
    public string Key { get; set; }
    public BarClass Bar { get; set; }
    public BarClass BarDependency { get; set; }
    [JsonIgnore] public int Dependencies { get; set; } = 0;
    [JsonIgnore] public int Occurances { get; set; } = 0;
    [JsonIgnore] public bool IsActivatedForCurrentProfile { get; set; } = false;
    public override string ToString()
    {
        return $"{Key}+{Modifier}: ({Bar?.Name},{BarDependency?.Name})";
    }

    public void InvertActivation() => IsActivatedForCurrentProfile = !IsActivatedForCurrentProfile;
}
