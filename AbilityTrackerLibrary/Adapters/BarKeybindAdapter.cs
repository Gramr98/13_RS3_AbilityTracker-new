namespace AbilityTrackerLibrary;

public class BarKeybindAdapter : FileAdapter<BarKeybindClass>
{
    public BarKeybindAdapter(string barKeybindJsonPath) : base(barKeybindJsonPath) { }
    public BarKeybindAdapter(string imagesDirPath, string barKeybindJsonPath) : base(imagesDirPath, barKeybindJsonPath) { }

    public bool TryConvertKeybindBar(string keyInput, BarClass barObject, BarClass barDependency, List<KeybindClass> abilitiesToCompare, out string message, out BarKeybindClass barKeybinding)
    {
        barKeybinding = null;

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
            // Edit 2022-11-24: Removed this check to allow multiple bars to be binded to a key, so you can (for example) switch with F6 from Mage to 
            // Melee and the other way around.
            //if (TList != null && TList.Any(obj => obj.Key == key && obj.Modifier == modifier))
            //{
            //    message = $"The key \'{keyInput}\' is already keybinded to a bar.";
            //    return false;
            //}
            // Edit 2022-11-29: Removed this check to allow the same keybinding for both abilities and Bar swaps (for example from FSOA to BOLG)
            //if (abilitiesToCompare != null && abilitiesToCompare.Any(obj => obj.Key == key && obj.Modifier == modifier))
            //{
            //    message = $"The key \'{keyInput}\' is already keybinded to another ability.";
            //    return false;
            //}
            //Edit 2022-11-30: Dependant Bar isn't allowed to be the same as the selected Bar - removed this section later on again
            //if (barObject.Name == barDependency.Name)
            //{
            //    message = $"The selected bar must not be the same as the dependant bar.";
            //    return false;
            //}
            barKeybinding = new(key, modifier, barObject, barDependency);
        }
        else
        {
            // Edit 2022-11-24: Removed this check to allow multiple bars to be binded to a key, so you can (for example) switch with F6 from Mage to 
            // Melee and the other way around.
            //if (TList != null && TList.Any(obj => obj.Key == keyInput && string.IsNullOrWhiteSpace(obj.Modifier)))
            //{
            //    message = $"The key \'{keyInput}\' is already keybinded to a bar.";
            //    return false;
            //}
            // Edit 2022-11-29: Removed this check to allow the same keybinding for both abilities and Bar swaps (for example from FSOA to BOLG)
            //if (abilitiesToCompare != null && abilitiesToCompare.Any(obj => obj.Key == keyInput && string.IsNullOrWhiteSpace(obj.Modifier)))
            //{
            //    message = $"The key \'{keyInput}\' is already keybinded to another ability.";
            //    return false;
            //}
            //Edit 2022-11-30: Dependant Bar isn't allowed to be the same as the selected Bar - removed this section later on again
            //if (barObject.Name == barDependency.Name)
            //{
            //    message = $"The selected bar must not be the same as the dependant bar.";
            //    return false;
            //}
            barKeybinding = new(keyInput, string.Empty, barObject, barDependency);
        }

        //ToDo: Validation für barObject benötigt oder nicht?

        message = "Validation successfull!";
        return true;
    }
}
