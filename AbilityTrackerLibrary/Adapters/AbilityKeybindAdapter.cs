namespace AbilityTrackerLibrary;

public class AbilityKeybindAdapter : FileAdapter<KeybindClass>
{
    public AbilityKeybindAdapter(string abilityKeybindJsonPath) : base(abilityKeybindJsonPath) { }

    public bool TryConvertKeybindAbility(string keyInput, Ability abilityObject, BarClass barObject, List<BarKeybindClass> barsToCompare, out string message, out KeybindClass abilityKeybinding)
    {
        abilityKeybinding = null;

        if (string.IsNullOrWhiteSpace(keyInput))
        {
            message = "Please enter a Keybinding.";
            return false;
        }

        if (keyInput.Contains("+"))
        {
            string[] inputParts = keyInput.Split('+');
            string modifier = inputParts[0];
            string key = inputParts[1];
            // Edit 2022-12-13: Removed this check to allow the same keybinding on the same bar for multiple profiles
            //if (TList != null && TList.Any(obj => obj.Key == key && obj.Modifier == modifier && obj.Bar.Name == barObject.Name))
            //{
            //    message = $"Key \'{keyInput}\' is already binded to a ability for the Bar \'{barObject.Name}\'.";
            //    return false;
            //}
            // Edit 2022-11-29: Removed this check to allow the same keybinding for both abilities and Bar swaps (for example from FSOA to BOLG)
            //if (barsToCompare != null && barsToCompare.Any(obj => obj.Key == key && obj.Modifier == modifier))
            //{
            //    message = $"The key \'{keyInput}\' is already keybinded to another bar.";
            //    return false;
            //}
            abilityKeybinding = new(key, modifier, barObject, abilityObject);
        }
        else
        {
            // Edit 2022-12-13: Removed this check to allow the same keybinding on the same bar for multiple profiles
            //if (TList != null && TList.Any(obj => obj.Key == keyInput && string.IsNullOrWhiteSpace(obj.Modifier) && obj.Bar.Name == barObject.Name))
            //{
            //    message = $"Key \'{keyInput}\' is already binded to a ability for the Bar \'{barObject.Name}\'.";
            //    return false;
            //}
            // Edit 2022-11-29: Removed this check to allow the same keybinding for both abilities and Bar swaps (for example from FSOA to BOLG)
            //if (barsToCompare != null && barsToCompare.Any(obj => obj.Key == keyInput && string.IsNullOrWhiteSpace(obj.Modifier)))
            //{
            //    message = $"The key \'{keyInput}\' is already keybinded to another bar.";
            //    return false;
            //}
            abilityKeybinding = new(keyInput, string.Empty, barObject, abilityObject);
        }

        //ToDo: Validation für abilityObject & barObject benötigt oder nicht?

        message = "Validation successfull!";
        return true;
    }
}
