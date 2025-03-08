using AbilityTrackerLibrary;

string fileToFix = @"C:\Users\stefa\OneDrive\Desktop\RS3_AbilityTracker\WPFUI\bin\Release\net6.0-windows\publish\win-x86\Data\Abilities.json";
string templateFile = @"C:\Users\stefa\OneDrive\Desktop\RS3_AbilityTracker\WPFUI\bin\Release\net6.0-windows\publish\win-x86\Data\Abilitybindings.json";
string fileToFixSave = @"C:\Users\stefa\OneDrive\Desktop\RS3_AbilityTracker\WPFUI\bin\Release\net6.0-windows\publish\win-x86\Data\Abilities2.json";

FileAdapter<Ability> abilityAdapter = new();
abilityAdapter.LoadJsonData(fileToFix);
List<Ability> allAbilities = abilityAdapter.TList;

FileAdapter <KeybindClass> keybindAdapter = new();
keybindAdapter.LoadJsonData(templateFile);
List<KeybindClass> allKeybinds = keybindAdapter.TList;

foreach (Ability abil in allAbilities)
{
    foreach (KeybindClass keybind in allKeybinds)
    {
        if(keybind.Ability.Name == abil.Name && 
           keybind.Ability.FriendlyName != abil.FriendlyName)
        {
            Console.WriteLine($"RENAMED: abil.FriendlyName: {abil.FriendlyName} - keybind.Ability.FriendlyName: {keybind.Ability.FriendlyName}");
            abil.FriendlyName = keybind.Ability.FriendlyName;
        }
        //else
        //{
        //    Console.WriteLine($"NOT CHANGED: abil.FriendlyName: {abil.FriendlyName} - keybind.Ability.FriendlyName: {keybind.Ability.FriendlyName}");
        //}
    }
}

abilityAdapter.TList = allAbilities;
abilityAdapter.SaveData(fileToFixSave);

Console.ReadLine();