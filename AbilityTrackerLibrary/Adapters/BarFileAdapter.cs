namespace AbilityTrackerLibrary;

public class BarFileAdapter : FileAdapter<BarClass>
{
    public BarFileAdapter(string imagesDirPath, string barJsonPath) : base(imagesDirPath, barJsonPath) { }

    public bool TryConvertBar(string name, string barImagePath, out string message, out BarClass bar)
    {
        bar = null;

        if (string.IsNullOrWhiteSpace(name.ToString()))
        {
            message = "Please enter a Bar Name.";
            return false;
        }
        if (TList != null && TList.Any(obj => obj.Name == name))
        {
            message = $"A Bar with the name {name} already exists.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(barImagePath))
        {
            message = "Please select a Bar Icon.";
            return false;
        }

        bar = new(name, barImagePath);
        message = "Validation successfull!";
        return true;
    }
}
