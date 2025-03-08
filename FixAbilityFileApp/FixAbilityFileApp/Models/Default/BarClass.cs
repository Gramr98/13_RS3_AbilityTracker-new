using Newtonsoft.Json;

namespace AbilityTrackerLibrary;

public class BarClass
{
    public BarClass() { }

    public BarClass(string name, string img)
    {
        Name = name;
        Img = img;
    }

    public string Name { get; set; }
    public string Img { get; set; }
    [JsonIgnore]
    public bool IsKeybinded { get { return Dependencies != 0; } }
    [JsonIgnore]
    public int Dependencies { get; set; } = 0;
}
