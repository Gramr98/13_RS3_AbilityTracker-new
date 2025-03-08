using System.Text;

namespace AbilityTrackerLibrary;

public class WikiImportReport
{
    public List<WikiImportReportEntry> ReportList { get; private set; } = new List<WikiImportReportEntry>();
    public List<Tuple<string, string>> FailedAbilities { get; private set; } = new List<Tuple<string, string>>();
    public int TotalCount { get { return ReportList.Sum(obj => obj.Count); } }
    public TimeSpan DurationInMs { get; set; }

    public void AddElement(WikiImportReportEntry entry) => ReportList.Add(entry);

    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendLine($"Total imported abilities: {TotalCount}{Environment.NewLine}" +
                                 $"Total required time: {DurationInMs.ToString()}");

        if (ReportList.Count > 0)
        {
            stringBuilder.AppendLine($"{Environment.NewLine}{new string('=', 20)}{Environment.NewLine}{Environment.NewLine}Information about specific groups:{Environment.NewLine}");
            foreach (WikiImportReportEntry item in ReportList)
            {
                stringBuilder.AppendLine($" - {item.Count} {item.FriendlyName} have been imported from the Wiki-Endpoint \'{item.WikiEndpoint}\'.");
                if (string.IsNullOrWhiteSpace(item.NamePrefix) == false)
                    stringBuilder.AppendLine($"     > Prefix for the saved files: \'{item.NamePrefix}\'.");
            }
        }

        if (FailedAbilities.Count > 0)
        {
            stringBuilder.AppendLine($"{Environment.NewLine}{new string('=', 20)}{Environment.NewLine}{Environment.NewLine}The following abilities could not be imported:{Environment.NewLine}");
            foreach (Tuple<string, string> item in FailedAbilities)
                stringBuilder.AppendLine($"{item.Item1} -> Reason: {item.Item2}");
        }

        return stringBuilder.ToString();
    }
}
