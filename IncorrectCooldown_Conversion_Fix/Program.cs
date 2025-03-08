/* When executing this console project, this will refresh the incorrect cooldowns in the json-Datafiles. 
 * For example: 54.0 seconds for "Greater Concentrated Blast" will get fixed into 5.4 seconds.
 * This will occure for the fules Profiles.json, Abilitybindings.json and Abilities.json" */

using AbilityTrackerLibrary;
using System.Collections.Generic;

string sourceFileWithCorrectCooldowns = @"D:\private_CSharpProjekte\13_RS3_AbilityTracker\WPFUI\bin\Debug\net6.0-windows\Data_CorrectCooldowns\Abilities.json";
string destinationFiles_ProfilesJson_Cooldowns = @"D:\private_CSharpProjekte\13_RS3_AbilityTracker\WPFUI\bin\Debug\net6.0-windows\Data\Profiles.json";
string destinationFiles_AbilityBindingsJson_Cooldowns = @"D:\private_CSharpProjekte\13_RS3_AbilityTracker\WPFUI\bin\Debug\net6.0-windows\Data\Abilitybindings.json";
string destinationFiles_AbilitiesJson_Cooldowns = @"D:\private_CSharpProjekte\13_RS3_AbilityTracker\WPFUI\bin\Debug\net6.0-windows\Data\Abilities.json";

List<Profile> incorrectProfiles = LoadIncorrectProfileData();
Console.WriteLine($"incorrectProfiles: {incorrectProfiles?.Count}\n");

List<Ability> incorrectAbilities = GetIncorrectAbilities();
Console.WriteLine($"incorrectAbilities: {incorrectAbilities?.Count}\n");

List<KeybindClass> incorrectKeybindings = GetIncorrectAbilityKeybindings();
Console.WriteLine($"incorrectKeybindings: {incorrectKeybindings?.Count} \n");

List<Ability> correctAbilities = GetCorrectAbilities();
Console.WriteLine($"correctAbilities: {correctAbilities?.Count}\n\n\n");

int relevantIncorrectAbilitiesTotalCounter = 0;
int relevantIncorrectAbilityBindingsTotalCounter = 0;
int relevantIncorrectAbilityBindingsInsideIncorrectProfileTotalCounter = 0;

foreach (Ability correctAbility in correctAbilities)
{
    // change the cooldown for Abilities themselves
    IEnumerable<Ability> relevantIncorrectAbilities = incorrectAbilities.Where(obj => obj.FriendlyName == correctAbility.FriendlyName && obj.Name == correctAbility.Name && obj.CooldownInSec != correctAbility.CooldownInSec );
    foreach (Ability incorrectAbility in relevantIncorrectAbilities)
    {
        incorrectAbility.CooldownInSec = correctAbility.CooldownInSec;
        relevantIncorrectAbilitiesTotalCounter++;
    }

    // change the cooldown for Abilities inside AbilityBindings
    IEnumerable<KeybindClass> relevantIncorrectAbilityBindings = incorrectKeybindings.Where(obj => obj.Ability.FriendlyName == correctAbility.FriendlyName && obj.Ability.Name == correctAbility.Name && obj.Ability.CooldownInSec != correctAbility.CooldownInSec);
    foreach (KeybindClass incorrectAbilityBinding in relevantIncorrectAbilityBindings)
    {
        incorrectAbilityBinding.Ability.CooldownInSec = correctAbility.CooldownInSec;
        relevantIncorrectAbilityBindingsTotalCounter++;
    }

    // change the cooldown for Abilities inside Profiles
    foreach (Profile incorrectProfile in incorrectProfiles)
    {
        List<KeybindClass> incorrectAbilitiesInsideProfile = incorrectProfile.ActiveAbilityKeybindObjects;

        IEnumerable<KeybindClass> relevantIncorrectAbilityBindingsInsideIncorrectProfile = incorrectAbilitiesInsideProfile.Where(obj => obj.Ability.FriendlyName == correctAbility.FriendlyName && obj.Ability.Name == correctAbility.Name && obj.Ability.CooldownInSec != correctAbility.CooldownInSec);
        foreach (KeybindClass incorrectAbilityBindingInsideIncorrectProfile in relevantIncorrectAbilityBindingsInsideIncorrectProfile)
        {
            incorrectAbilityBindingInsideIncorrectProfile.Ability.CooldownInSec = correctAbility.CooldownInSec;
            relevantIncorrectAbilityBindingsInsideIncorrectProfileTotalCounter++;
        }
    }
    
}

Console.WriteLine("========================");
IEnumerable<Ability> notUpdatedAbilities = incorrectAbilities.Where(inc => !correctAbilities.Any(cor => inc.FriendlyName == cor.FriendlyName && inc.Name == cor.Name && inc.CooldownInSec != cor.CooldownInSec));
Console.WriteLine($"notUpdatedAbilities: {(notUpdatedAbilities == null ? "null" : notUpdatedAbilities.Count())}");
Console.WriteLine($"relevantIncorrectAbilitiesTotalCounter: {relevantIncorrectAbilitiesTotalCounter}\n");

IEnumerable<KeybindClass> notUpdatedKeybindings = incorrectKeybindings.Where(inc => !correctAbilities.Any(cor => inc.Ability.FriendlyName == cor.FriendlyName && inc.Ability.Name == cor.Name && inc.Ability.CooldownInSec != cor.CooldownInSec));
Console.WriteLine($"notUpdatedKeybindings: {(notUpdatedKeybindings == null ? "null" : notUpdatedKeybindings.Count())}");
Console.WriteLine($"relevantIncorrectAbilityBindingsTotalCounter: {relevantIncorrectAbilityBindingsTotalCounter}\n");

foreach (Profile incorrectProfile in incorrectProfiles)
{
    Console.WriteLine($"Profile: {incorrectProfile}");

    IEnumerable<KeybindClass> notUpdatedKeybindingsInsideProfile = incorrectProfile.ActiveAbilityKeybindObjects.Where(inc => !correctAbilities.Any(cor => inc.Ability.FriendlyName == cor.FriendlyName && inc.Ability.Name == cor.Name && inc.Ability.CooldownInSec != cor.CooldownInSec ));
    Console.WriteLine($"notUpdatedKeybindingsInsideProfile: {(notUpdatedKeybindingsInsideProfile == null ? "null" : notUpdatedKeybindingsInsideProfile.Count())}");
    Console.WriteLine($"relevantIncorrectAbilityBindingsInsideIncorrectProfileTotalCounter: {relevantIncorrectAbilityBindingsInsideIncorrectProfileTotalCounter}\n");

    Console.WriteLine();
}

try
{
    Console.WriteLine($"Save the result above? (y/n)");
    if (Console.ReadLine() == "y")
    {
        Console.WriteLine("Starting saving Abilities...");
        AbilityFileAdapter abilityAdapter = new AbilityFileAdapter(null, destinationFiles_AbilitiesJson_Cooldowns);
        abilityAdapter.ReplaceTList(incorrectAbilities);
        abilityAdapter.SaveData(destinationFiles_AbilitiesJson_Cooldowns);

        Console.WriteLine("Starting saving AbilityKeybindings...");
        AbilityKeybindAdapter abilityKeybindAdapter = new AbilityKeybindAdapter(destinationFiles_AbilityBindingsJson_Cooldowns);
        abilityKeybindAdapter.ReplaceTList(incorrectKeybindings);
        abilityKeybindAdapter.SaveData(destinationFiles_AbilityBindingsJson_Cooldowns);

        Console.WriteLine("Starting saving Abilities inside Profiles...");
        FileAdapter<Profile> profileAdapter = new(destinationFiles_ProfilesJson_Cooldowns);
        profileAdapter.ReplaceTList(incorrectProfiles);
        profileAdapter.SaveData(destinationFiles_ProfilesJson_Cooldowns);
    }
    else
    {
        Console.WriteLine("No saving occured.");
    }

}
catch (Exception ex)
{
    Console.WriteLine($"\n\nEXCEPTION\n: " + ex);
}

Console.WriteLine("\n\n=== done ===");



List<Profile> LoadIncorrectProfileData()
{
    //FileAdapter<Profile> profileAdapter = new(GlobalApplicationSettings.FilePathSettings.ProfileJsonFile);
    FileAdapter<Profile> profileAdapter = new(destinationFiles_ProfilesJson_Cooldowns);
    List<Profile> result = profileAdapter.TList.OrderBy(i => i.ProfileName)?.ToList();
    return result;
}

List<Ability> GetIncorrectAbilities()
{
    //AbilityFileAdapter adapter = new AbilityFileAdapter(GlobalApplicationSettings.FilePathSettings.AbilityImages, GlobalApplicationSettings.FilePathSettings.AbilityJsonFile);
    AbilityFileAdapter adapter = new AbilityFileAdapter(null, destinationFiles_AbilitiesJson_Cooldowns);
    List<Ability> result = adapter.TList;
    return result;
}

List<KeybindClass> GetIncorrectAbilityKeybindings()
{
    //AbilityKeybindAdapter abilityKeybindAdapter = new AbilityKeybindAdapter(GlobalApplicationSettings.FilePathSettings.AbilityKeybindJsonFile);
    AbilityKeybindAdapter abilityKeybindAdapter = new AbilityKeybindAdapter(destinationFiles_AbilityBindingsJson_Cooldowns);
    List<KeybindClass> result = abilityKeybindAdapter.TList.OrderBy(i => i.Ability.Name)?.ToList();
    return result;
}

List<Ability> GetCorrectAbilities()
{
    //AbilityFileAdapter adapter = new AbilityFileAdapter(GlobalApplicationSettings.FilePathSettings.AbilityImages, GlobalApplicationSettings.FilePathSettings.AbilityJsonFile);
    AbilityFileAdapter adapter = new AbilityFileAdapter(null, sourceFileWithCorrectCooldowns);
    List<Ability> result = adapter.TList;
    return result;
}