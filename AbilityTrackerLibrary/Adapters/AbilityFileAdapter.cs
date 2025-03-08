namespace AbilityTrackerLibrary;

public class AbilityFileAdapter : FileAdapter<Ability>
{
    public AbilityFileAdapter(string abilityJsonPath, bool savePreviousItemsData = true) : base(abilityJsonPath, savePreviousItemsData) { }
    public AbilityFileAdapter(string imagesDirPath, string abilityJsonPath) : base(imagesDirPath, abilityJsonPath) { }

    public bool TryConvertAbility(string name, string cooldown, string abilityImagePath, bool? activateInsideGCD, bool? isBleedAbility, out string message, out Ability ability)
    {
        ability = null;

        if (string.IsNullOrWhiteSpace(name.ToString()))
        {
            message = "Please enter a Ability Name.";
            return false;
        }
        if (TList != null && TList.Any(obj => obj.Name == name.ToString()))
        {
            message = $"A Ability with the name {name} already exists.";
            return false;
        }
        if (double.TryParse(cooldown, out double abilityCooldown) == false)
        {
            message = "Please enter a valid Ability Cooldown.";
            return false;
        }
        if (abilityCooldown < 0)
        {
            message = "Please enter a Ability Cooldown that is equals to or greater than 0.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(abilityImagePath))
        {
            message = "Please select a Ability Icon.";
            return false;
        }

        ability = new(name, abilityCooldown, abilityImagePath, activateInsideGCD == true, isBleedAbility == true);
        ability.FriendlyName = name;
        message = "Validation successfull!";
        return true;
    }
}