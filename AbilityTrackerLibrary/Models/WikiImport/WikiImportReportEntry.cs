namespace AbilityTrackerLibrary;

public class WikiImportReportEntry
{
    public WikiImportReportEntry() { }

    public WikiImportReportEntry(string friendlyName, string wikiEndpoint, string namePrefix, int count)
    {
        FriendlyName = friendlyName;
        WikiEndpoint = wikiEndpoint;
        NamePrefix = namePrefix;
        Count = count;
    }

    public string FriendlyName { get; set; }
    public string WikiEndpoint { get; set; }
    public string NamePrefix { get; set; }
    public int Count { get; set; } = 0;
}
