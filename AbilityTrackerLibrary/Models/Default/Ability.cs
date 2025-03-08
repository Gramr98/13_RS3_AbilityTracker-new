using Newtonsoft.Json;
using System.Diagnostics;

namespace AbilityTrackerLibrary;

public class Ability
{
    private string img;

    public Ability() { }

    public Ability(string name, double cooldown, string img, bool activateInsideGCD, bool isBleedAbility)
    {
        Name = name;
        CooldownInSec = cooldown;
        Img = img;
        CanActivateDuringGCD = activateInsideGCD;
        IsBleedAbility = isBleedAbility;
    }

    public string FriendlyName { get; set; }
    public string Name { get; set; }
    public string Img 
    {
        get
        {
            //return img;
            return Path.Combine(Directory.GetCurrentDirectory(), img);
        }
        set => img = value; 
    }
    public double CooldownInSec { get; set; }
    public bool CanActivateDuringGCD { get; set; }
    public bool IsBleedAbility { get; set; }

    [JsonIgnore]
    public Stopwatch AbilityStopwatch { get; } = new Stopwatch();
    [JsonIgnore]
    public double CooldownInMs { get { return CooldownInSec * 1000; } }
    [JsonIgnore]
    public double CurrentCooldownInMs { get { return CooldownInMs - AbilityStopwatch.ElapsedMilliseconds; } }
    [JsonIgnore]
    public bool IsOnCooldown { get { return AbilityStopwatch.ElapsedMilliseconds < CooldownInMs; } } // If the elapsed time since the ability has been activated is less than the ability cooldown itself, the ability is not ready yet (true => on cooldown)
    [JsonIgnore]
    public bool DoubleAbilitySecondActivation { get; set; } = false;
    [JsonIgnore]
    public bool IsKeybinded { get { return Dependencies != 0; } }
    [JsonIgnore]
    public int Dependencies { get; set; } = 0;
}
