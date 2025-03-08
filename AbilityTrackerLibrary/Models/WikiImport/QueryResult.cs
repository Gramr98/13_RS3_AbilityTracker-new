namespace AbilityTrackerLibrary;

public class QueryResult
{
    public QueryResult(List<Ability> abilities, List<Tuple<string, string>> failedAbilities, string friendlyName, string wikiEndpoint, string namePrefix)
    {
        Abilities = abilities;
        ReportEntry = new WikiImportReportEntry(friendlyName, wikiEndpoint, namePrefix, Abilities.Count);
        FailedAbilities = failedAbilities;
    }

    public List<Ability> Abilities { get; set; }
    public WikiImportReportEntry ReportEntry { get; set; } = new WikiImportReportEntry();
    public List<Tuple<string, string>> FailedAbilities { get; private set; } = new List<Tuple<string, string>>();
}