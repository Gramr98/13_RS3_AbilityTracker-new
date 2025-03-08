namespace AbilityTrackerLibrary;

public class Icon
{
    private string relativeIconPath;

    public Icon(string iconPath)
    {
        IconPath = iconPath;
    }

    public string IconFileName { get => System.IO.Path.GetFileNameWithoutExtension(relativeIconPath); }
    public string IconPath 
    { 
        get
        {
            //return relativeIconPath;
            return Path.Combine(Directory.GetCurrentDirectory(), relativeIconPath);
        }
        set => relativeIconPath = value; 
    }
}
