using HtmlAgilityPack;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AbilityTrackerLibrary;

public class WikiImporter
{
    public async Task<WikiImportReport> ReadAndSaveAbilities(string imagesDirPath, string abilitiesFilePath, string abilityKeybindingsFilePath)
    {
        Stopwatch watch = Stopwatch.StartNew();

        if (Directory.Exists(imagesDirPath))
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(imagesDirPath);
                foreach (FileInfo info in dir.EnumerateFiles())
                    info.Delete();
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedAccessException($"Not allowed to delete the files in the directory \'{imagesDirPath}\'. " +
                    "A reason for that could be that the folder is protected. " +
                    "Please try to right click the Folder, go to Properties and remove the write protection. If that doesn't work, delete the files manually.", ex);
            }
        }
        else
            Directory.CreateDirectory(imagesDirPath);

        //List<Task<QueryResult>> queryResults = new();

        //queryResults.Add(GetDefaultAbilities("Abilities", imagesDirPath, "", "Abilities"));
        List<Task<Ability>> tasks_Abilities = await GetDefaultAbilities("Abilities", imagesDirPath, "", "Abilities"); // Stand 2025-02-11: 181 Entitäten

        //queryResults.Add(GetOtherAbilityElements("Ancient_Curses", imagesDirPath,
        //    "//table[@class='wikitable sticky-header align-left-2 align-left-4']", "Curses_", "Curses", "Curses_", 3, true));
        //queryResults.Add(GetOtherAbilityElements("Ancient_Curses", imagesDirPath,
        //    "//table[@class='wikitable sticky-header align-left-2 align-left-4']", "Curses_", "Curses", "", 3, true));
        List<Task<Ability>> tasks_Ancient_Curses = await GetAncientCursesAbilityElements("Ancient_Curses", imagesDirPath, // Stand 2025-02-11: 6 Entitäten
            "//table[@class='wikitable sticky-header align-left-2 align-left-4']", "Curses", "", true);

        ////queryResults.Add(GetOtherAbilityElements("Prayer", imagesDirPath,
        ////    "//table[@class='wikitable sticky-header sortable align-left-7']", "Prayer_", "Prayer", "Prayer_", 1, true));
        //queryResults.Add(GetOtherAbilityElements("Prayer", imagesDirPath,
        //    "//table[@class='wikitable sticky-header sortable align-left-7']", "Prayer_", "Prayer", "", 1, true));
        List<Task<Ability>> tasks_Prayer = await GetPrayersAbilityElements("Prayer", imagesDirPath, // Stand 2025-02-11: 0 Entitäten
            "//table[@class='wikitable align-right-3 align-right-4 align-right-5']", "Prayer", "", true);

        ////queryResults.Add(GetOtherAbilityElements("Standard_spells", imagesDirPath,
        ////    "//table[@class='wikitable sortable align-left-7']", "Spells_", "Standard Spells", "StandardSpells_", 1, true));
        //queryResults.Add(GetOtherAbilityElements("Standard_spells", imagesDirPath,
        //    "//table[@class='wikitable sortable align-left-7']", "Spells_", "Standard Spells", "", 1, true));
        List<Task<Ability>> tasks_Standard_spells = await GetStandardSpellAbilityElements("Standard_spells", imagesDirPath, // Stand 2025-02-11: 0 Entitäten
            "//table[@class='wikitable align-right-1 align-center-2 align-left-3 align-center-4 align-left-5 align-left-6 align-left-7 align-left-8 align-left-9 align-center-10']", "Standard Spells", "", true);

        ////queryResults.Add(GetOtherAbilityElements("Ancient_Magicks", imagesDirPath,
        ////    "//table[@class='wikitable sortable align-left-7']", "Spells_", "Ancient Spells", "AncientSpells_", 1, true));
        //queryResults.Add(GetOtherAbilityElements("Ancient_Magicks", imagesDirPath,
        //    "//table[@class='wikitable sortable align-left-7']", "Spells_", "Ancient Spells", "", 1, true));
        List<Task<Ability>> tasks_Ancient_Magicks = await GetAncientMagicksAbilityElements("Ancient_Magicks", imagesDirPath, // Stand 2025-02-11: 0 Entitäten
            "//table[@class='wikitable align-right-1 align-center-2 align-left-3 align-center-4 align-left-5 align-left-6 align-left-7 align-left-8 align-left-9 align-center-10']", "Ancient Spells", "", true);

        ////queryResults.Add(GetOtherAbilityElements("Lunar_spells", imagesDirPath,
        ////    "//table[@class='wikitable sortable align-center-2 align-center-4 align-center-6']", "Spells_", "Lunar Spells", "LunarSpells_", 1, true));
        //queryResults.Add(GetOtherAbilityElements("Lunar_spells", imagesDirPath,
        //    "//table[@class='wikitable sortable align-center-2 align-center-4 align-center-6']", "Spells_", "Lunar Spells", "", 1, true));
        List<Task<Ability>> tasks_Lunar_spells = await GetLunarSpellsAbilityElements("Lunar_spells", imagesDirPath, // Stand 2025-02-11: 0 Entitäten
            "//table[@class='wikitable align-right-1 align-center-2 align-left-3 align-center-4 align-left-5 align-left-6 align-left-7 align-left-8 align-left-9 align-center-10']", "Lunar Spells", "", true);

        List<Task<Ability>> tasks_Incantations = await GetIncantationsAbilityElements("Incantations", imagesDirPath, // Stand 2025-02-11: 0 Entitäten
            "//table[@class='wikitable align-right-1 align-center-2 align-left-3 align-center-4 align-left-5 align-left-6 align-center-7']", "Incantations", "", true);

        WikiImportReport importReport = new WikiImportReport();
        List<Ability> resultList = new List<Ability>();

        try
        {
            //QueryResult[] result = await Task.WhenAll(queryResults);
            //foreach (QueryResult resultItem in result)
            //{
            //    importReport.AddElement(resultItem.ReportEntry);
            //    importReport.FailedAbilities.AddRange(resultItem.FailedAbilities);
            //    resultList.AddRange(resultItem.Abilities);
            //}

            //Task.WhenAll(wikiImportTasks.ToArray());
            List<Task<Ability>> wikiImportTasks = tasks_Abilities.Concat(tasks_Ancient_Curses).Concat(tasks_Prayer).Concat(tasks_Standard_spells).Concat(tasks_Ancient_Magicks).Concat(tasks_Lunar_spells).Concat(tasks_Incantations).ToList();
            Task<Ability[]> taskResult = Task.WhenAll<Ability>(wikiImportTasks);
            Ability[] taskResult2 = taskResult.Result;
            resultList = taskResult2.ToList();
        }
        catch (Exception ex)
        {
            // Breakpoint relevant
            // ToDo: Add Logging
        }

        AbilityFileAdapter abilityAdapter = new AbilityFileAdapter(abilitiesFilePath, false);
        abilityAdapter.SaveNewObjects(resultList, abilitiesFilePath);

        AbilityKeybindAdapter abilityKeybindingAdapter = new AbilityKeybindAdapter(abilityKeybindingsFilePath);
        abilityKeybindingAdapter.DeleteTFileContent(abilityKeybindingsFilePath, true);

        watch.Stop();
        importReport.DurationInMs = watch.Elapsed;

        return importReport;
    }

    private async Task<string> DownloadHTMlClient(string endpoint)
    {
        string url = "http://runescape.wiki/w/";
        using (HttpClient client = new HttpClient())
        {
            client.BaseAddress = new Uri(url + endpoint);
            client.DefaultRequestHeaders.Add("User-Agent", GlobalApplicationSettings.OtherSettings.UserAgent_WikiAPICalls);
            return await client.GetStringAsync(url + endpoint);
        }
    }

    //private static string getHTMLCode(string endpoint)
    //{
    //    string url = "http://runescape.wiki/w/";
    //    string pageHTML = "";
    //    using (WebClient web = new WebClient())
    //    {
    //        web.Headers.Add("User-Agent", "GlobalApplicationSettings.OtherSettings.UserAgent_WikiAPICalls");
    //        pageHTML = web.DownloadString(url + endpoint);

    //    }
    //    return pageHTML;
    //}

    //private async Task<QueryResult> GetDefaultAbilities(string endpoint, string dirPath, string prefixForName, string friendlyName)
    private async Task<List<Task<Ability>>> GetDefaultAbilities(string endpoint, string dirPath, string prefixForName, string friendlyName)
    {
        string response = await DownloadHTMlClient(endpoint); //string response = getHTMLCode("Abilities");

        HtmlDocument document = new();
        document.LoadHtml(response);
        //HtmlNodeCollection tables = document.DocumentNode.SelectNodes("//table[@class='wikitable sortable']");
        HtmlNodeCollection tables = document.DocumentNode.SelectNodes("//table[@class='wikitable sortable sticky-header']");

        //Dictionary<int, string> categoryMapping = GetCategoryMapping();
        //List<Ability> abilities = new List<Ability>();
        //List<Tuple<string, string>> failedAbilities = new List<Tuple<string, string>>();
        List<Task<Ability>> wikiImportTasks = new List<Task<Ability>>();

        //for (int k = 0; k < tables.Count(); k++)
        //{
        //    if (categoryMapping.ContainsKey(k) == false) continue;

        //    string type = categoryMapping[k];
        //    var table = tables[k];

        //    for (int i = 1; i < table.ChildNodes.Count(); i++)
        //    {
        //        for (int j = 2; j < table.ChildNodes[i].ChildNodes.Count(); j += 2)
        //        {
        //            string name = table.ChildNodes[i].ChildNodes[j].ChildNodes[1].InnerText.Replace("\n", "").Trim();
        //            try
        //            {
        //                string coolDown = table.ChildNodes[i].ChildNodes[j].ChildNodes[13].InnerText.Replace("\n", "").Trim();
        //                if (string.IsNullOrWhiteSpace(name))
        //                {
        //                    failedAbilities.Add(new Tuple<string, string>(name, "The received ability name is null or empty."));
        //                    continue;
        //                }
        //                if (name != "Tuska's Wrath")
        //                {
        //                    if (Double.TryParse(coolDown, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double convertedCD) == false)
        //                    {
        //                        failedAbilities.Add(new Tuple<string, string>(name, $"The received cooldown of \'{coolDown}\' is not valid for conversion."));
        //                        continue;
        //                    }
        //                    abilities.Add(SetAbility(name, dirPath, type, prefixForName, convertedCD, false));
        //                }
        //                else
        //                {
        //                    abilities.Add(SetAbility(name, dirPath, type, prefixForName, 15, false));
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                failedAbilities.Add(new Tuple<string, string>(name, ex.Message));
        //            }
        //        }
        //    }
        //}

        for (int k = 0; k < tables.Count(); k++)
        {

            var table = tables[k];
            string type = "";
            switch (k)
            {
                case 0:
                    type = "Melee_";
                    break;
                case 1:
                    type = "Melee_";
                    break;
                case 2:
                    type = "Range_";
                    break;
                case 3:
                    type = "Mage_";
                    break;
                case 4:
                    type = "Necromancy_";

                    break;
                case 5:
                    type = "Defense_";

                    break;
                case 6:
                    type = "Constitution_";
                    break;
            }
            for (int i = 1; i < table.ChildNodes.Count(); i++)
            {
                for (int j = 2; j < table.ChildNodes[i].ChildNodes.Count(); j += 2)
                {
                    string name = table.ChildNodes[i].ChildNodes[j].ChildNodes[1].InnerText.Replace("\n", "").Trim();

                    string imgURL = "";
                    try
                    {
                        string imgHTML = table.ChildNodes[i].ChildNodes[j].ChildNodes[3].InnerHtml.Replace("\n", "").Trim();
                        var index = imgHTML.IndexOf("srcset");

                        string htmlrest = imgHTML.Substring(index, imgHTML.Length - 1 - index).Replace("srcset=\"", "");
                        index = htmlrest.IndexOf("?");
                        imgURL = htmlrest.Substring(0, index);


                    }
                    catch (Exception ex)
                    {
                        string imgHTML = table.ChildNodes[i].ChildNodes[j].ChildNodes[3].InnerHtml.Replace("\n", "").Trim();
                        var index = imgHTML.IndexOf("src");

                        string htmlrest = imgHTML.Substring(index, imgHTML.Length - 1 - index).Replace("src=\"", "");
                        index = htmlrest.IndexOf("?");
                        imgURL = htmlrest.Substring(0, index);
                    }
                    if (!imgURL.Contains("images") || !imgURL.Contains(".png"))
                    {
                        imgURL = "";
                    }
                    if (type.Equals("Necromancy_") && j.Equals(2))
                    {
                        name = name + "_Auto";
                    }
                    string coolDown = "";
                    if (type.Equals("Necromancy_"))
                    {
                        coolDown = table.ChildNodes[i].ChildNodes[j].ChildNodes[17].InnerText.Replace("\n", "").Trim();
                    }
                    else
                    {
                        coolDown = table.ChildNodes[i].ChildNodes[j].ChildNodes[15].InnerText.Replace("\n", "").Trim();
                    }
                    double CD = 0;
                    try
                    {
                        CD = Convert.ToDouble(coolDown, CultureInfo.InvariantCulture);
                    }
                    catch { }

                    wikiImportTasks.Add(Task.Factory.StartNew(() => SetAbility(name, dirPath, type, prefixForName, CD, false, imgURL)));
                }
            }
        }

        return wikiImportTasks;
    }

    //private async Task<QueryResult> GetAncientCursesAbilityElements(string endpoint, string dirPath, string nodeElement, string friendlyName, string prefixForName, bool canActivateDuringGCD = true)
    private async Task<List<Task<Ability>>> GetAncientCursesAbilityElements(string endpoint, string dirPath, string nodeElement, string friendlyName, string prefixForName, bool canActivateDuringGCD = true)
    {
        string response = await DownloadHTMlClient(endpoint); //string response = getHTMLCode(endpoint);

        HtmlDocument document = new();
        document.LoadHtml(response);
        HtmlNodeCollection tables = document.DocumentNode.SelectNodes(nodeElement);

        //List<Ability> elements = new List<Ability>();
        //List<Tuple<string, string>> failedAbilities = new List<Tuple<string, string>>();
        List<Task<Ability>> wikiImportTasks = new List<Task<Ability>>();

        foreach (var table in tables)
        {
            for (int i = 1; i < table.ChildNodes.Count(); i++)
            {
                for (int j = 4; j < table.ChildNodes[i].ChildNodes.Count(); j += 2)
                {
                    string name = table.ChildNodes[i].ChildNodes[j].ChildNodes[3].InnerText.Replace("\n", "").Trim();
                    string imgURL = "";
                    try
                    {
                        string imgHTML = table.ChildNodes[i].ChildNodes[j].ChildNodes[1].InnerHtml.Replace("\n", "").Trim();
                        var index = imgHTML.IndexOf("srcset");
                        string htmlrest = imgHTML.Substring(index, imgHTML.Length - 1 - index).Replace("srcset=\"", "");
                        index = htmlrest.IndexOf("?");
                        imgURL = htmlrest.Substring(0, index);
                    }
                    catch (Exception ex)
                    {
                        string imgHTML = table.ChildNodes[i].ChildNodes[j].ChildNodes[1].InnerHtml.Replace("\n", "").Trim();
                        var index = imgHTML.IndexOf("src");
                        string htmlrest = imgHTML.Substring(index, imgHTML.Length - 1 - index).Replace("src=\"", "");
                        index = htmlrest.IndexOf("?");
                        imgURL = htmlrest.Substring(0, index);
                    }

                    if (!imgURL.Contains("images") || !imgURL.Contains(".png"))
                    {
                        imgURL = "";
                    }
                    wikiImportTasks.Add(Task.Factory.StartNew(() => SetAbility(name, dirPath, "Curses_", prefixForName, 0, canActivateDuringGCD, imgURL)));
                    //abils.Add(ability);
                }
            }
        }

        //return new QueryResult(elements, failedAbilities, friendlyName, endpoint, prefixForName);
        return wikiImportTasks;
    }

    //private async Task<QueryResult> GetPrayersAbilityElements(string endpoint, string dirPath, string nodeElement, string friendlyName, string prefixForName, bool canActivateDuringGCD = true)
    private async Task<List<Task<Ability>>> GetPrayersAbilityElements(string endpoint, string dirPath, string nodeElement, string friendlyName, string prefixForName, bool canActivateDuringGCD = true)
    {
        string response = await DownloadHTMlClient(endpoint); //string response = getHTMLCode(endpoint);

        HtmlDocument document = new();
        document.LoadHtml(response);
        HtmlNodeCollection tables = document.DocumentNode.SelectNodes(nodeElement);

        //List<Ability> elements = new List<Ability>();
        //List<Tuple<string, string>> failedAbilities = new List<Tuple<string, string>>();
        List<Task<Ability>> wikiImportTasks = new List<Task<Ability>>();

        foreach (var table in tables)
        {
            for (int i = 1; i < table.ChildNodes.Count(); i++)
            {
                for (int j = 2; j < table.ChildNodes[i].ChildNodes.Count(); j += 2)
                {
                    //Ability ability = new Ability();
                    string name = table.ChildNodes[i].ChildNodes[j].ChildNodes[3].InnerText.Replace("\n", "").Trim();
                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }
                    string imgURL = "";
                    try
                    {
                        string imgHTML = table.ChildNodes[i].ChildNodes[j].ChildNodes[1].InnerHtml.Replace("\n", "").Trim();
                        var index = imgHTML.IndexOf("srcset");

                        string htmlrest = imgHTML.Substring(index, imgHTML.Length - 1 - index).Replace("srcset=\"", "");
                        index = htmlrest.IndexOf("?");
                        imgURL = htmlrest.Substring(0, index);


                    }
                    catch (Exception ex)
                    {
                        string imgHTML = table.ChildNodes[i].ChildNodes[j].ChildNodes[1].InnerHtml.Replace("\n", "").Trim();
                        var index = imgHTML.IndexOf("src");

                        string htmlrest = imgHTML.Substring(index, imgHTML.Length - 1 - index).Replace("src=\"", "");
                        index = htmlrest.IndexOf("?");
                        imgURL = htmlrest.Substring(0, index);
                    }
                    if (!imgURL.Contains("images") || !imgURL.Contains(".png"))
                    {
                        imgURL = "";
                    }
                    wikiImportTasks.Add(Task.Factory.StartNew(() => SetAbility(name, dirPath, "Prayer_", prefixForName, 0, canActivateDuringGCD, imgURL)));
                }
            }
        }

        //return new QueryResult(elements, failedAbilities, friendlyName, endpoint, prefixForName);
        return wikiImportTasks;
    }

    //private async Task<QueryResult> GetStandardSpellAbilityElements(string endpoint, string dirPath, string nodeElement, string friendlyName, string prefixForName, bool canActivateDuringGCD = true)
    private async Task<List<Task<Ability>>> GetStandardSpellAbilityElements(string endpoint, string dirPath, string nodeElement, string friendlyName, string prefixForName, bool canActivateDuringGCD = true)
    {
        string response = await DownloadHTMlClient(endpoint); //string response = getHTMLCode(endpoint);

        HtmlDocument document = new();
        document.LoadHtml(response);
        HtmlNodeCollection tables = document.DocumentNode.SelectNodes(nodeElement);

        //List<Ability> elements = new List<Ability>();
        //List<Tuple<string, string>> failedAbilities = new List<Tuple<string, string>>();
        List<Task<Ability>> wikiImportTasks = new List<Task<Ability>>();

        foreach (var table in tables)
        {
            for (int i = 1; i < table.ChildNodes.Count(); i++)
            {
                for (int j = 2; j < table.ChildNodes[i].ChildNodes.Count(); j += 2)
                {
                    //Ability ability = new Ability();
                    string name = table.ChildNodes[i].ChildNodes[j].ChildNodes[1].InnerText.Replace("\n", "").Trim();
                    string imgURL = "";
                    try
                    {
                        string imgHTML = table.ChildNodes[i].ChildNodes[j].ChildNodes[3].InnerHtml.Replace("\n", "").Trim();
                        var index = imgHTML.IndexOf("srcset");

                        string htmlrest = imgHTML.Substring(index, imgHTML.Length - 1 - index).Replace("srcset=\"", "");
                        index = htmlrest.IndexOf("?");
                        imgURL = htmlrest.Substring(0, index);


                    }
                    catch (Exception ex)
                    {
                        string imgHTML = table.ChildNodes[i].ChildNodes[j].ChildNodes[3].InnerHtml.Replace("\n", "").Trim();
                        var index = imgHTML.IndexOf("src");

                        string htmlrest = imgHTML.Substring(index, imgHTML.Length - 1 - index).Replace("src=\"", "");
                        index = htmlrest.IndexOf("?");
                        imgURL = htmlrest.Substring(0, index);
                    }
                    if (!imgURL.Contains("images") || !imgURL.Contains(".png"))
                    {
                        imgURL = "";
                    }
                    wikiImportTasks.Add(Task.Factory.StartNew(() => SetAbility(name, dirPath, "Spells_", prefixForName, 0, canActivateDuringGCD, imgURL)));
                }
            }
        }

        //return new QueryResult(elements, failedAbilities, friendlyName, endpoint, prefixForName);
        return wikiImportTasks;
    }

    //private async Task<QueryResult> GetAncientMagicksAbilityElements(string endpoint, string dirPath, string nodeElement, string friendlyName, string prefixForName, bool canActivateDuringGCD = true)
    private async Task<List<Task<Ability>>> GetAncientMagicksAbilityElements(string endpoint, string dirPath, string nodeElement, string friendlyName, string prefixForName, bool canActivateDuringGCD = true)
    {
        string response = await DownloadHTMlClient(endpoint); //string response = getHTMLCode(endpoint);

        HtmlDocument document = new();
        document.LoadHtml(response);
        HtmlNodeCollection tables = document.DocumentNode.SelectNodes(nodeElement);

        //List<Ability> elements = new List<Ability>();
        //List<Tuple<string, string>> failedAbilities = new List<Tuple<string, string>>();
        List<Task<Ability>> wikiImportTasks = new List<Task<Ability>>();

        foreach (var table in tables)
        {
            for (int i = 1; i < table.ChildNodes.Count(); i++)
            {
                for (int j = 2; j < table.ChildNodes[i].ChildNodes.Count(); j += 2)
                {
                    //Ability ability = new Ability();
                    string name = table.ChildNodes[i].ChildNodes[j].ChildNodes[1].InnerText.Replace("\n", "").Trim();
                    string imgURL = "";
                    try
                    {
                        string imgHTML = table.ChildNodes[i].ChildNodes[j].ChildNodes[3].InnerHtml.Replace("\n", "").Trim();
                        var index = imgHTML.IndexOf("srcset");

                        string htmlrest = imgHTML.Substring(index, imgHTML.Length - 1 - index).Replace("srcset=\"", "");
                        index = htmlrest.IndexOf("?");
                        imgURL = htmlrest.Substring(0, index);


                    }
                    catch (Exception ex)
                    {
                        string imgHTML = table.ChildNodes[i].ChildNodes[j].ChildNodes[3].InnerHtml.Replace("\n", "").Trim();
                        var index = imgHTML.IndexOf("src");

                        string htmlrest = imgHTML.Substring(index, imgHTML.Length - 1 - index).Replace("src=\"", "");
                        index = htmlrest.IndexOf("?");
                        imgURL = htmlrest.Substring(0, index);
                    }
                    if (!imgURL.Contains("images") || !imgURL.Contains(".png"))
                    {
                        imgURL = "";
                    }
                    wikiImportTasks.Add(Task.Factory.StartNew(() => SetAbility(name, dirPath, "Spells_", prefixForName, 0, canActivateDuringGCD, imgURL)));
                }
            }
        }

        //return new QueryResult(elements, failedAbilities, friendlyName, endpoint, prefixForName);
        return wikiImportTasks;
    }

    //private async Task<QueryResult> GetLunarSpellsAbilityElements(string endpoint, string dirPath, string nodeElement, string friendlyName, string prefixForName, bool canActivateDuringGCD = true)
    private async Task<List<Task<Ability>>> GetLunarSpellsAbilityElements(string endpoint, string dirPath, string nodeElement, string friendlyName, string prefixForName, bool canActivateDuringGCD = true)
    {
        string response = await DownloadHTMlClient(endpoint); //string response = getHTMLCode(endpoint);

        HtmlDocument document = new();
        document.LoadHtml(response);
        HtmlNodeCollection tables = document.DocumentNode.SelectNodes(nodeElement);

        //List<Ability> elements = new List<Ability>();
        //List<Tuple<string, string>> failedAbilities = new List<Tuple<string, string>>();
        List<Task<Ability>> wikiImportTasks = new List<Task<Ability>>();

        foreach (var table in tables)
        {
            for (int i = 1; i < table.ChildNodes.Count(); i++)
            {
                for (int j = 2; j < table.ChildNodes[i].ChildNodes.Count(); j += 2)
                {
                    //Ability ability = new Ability();
                    string name = table.ChildNodes[i].ChildNodes[j].ChildNodes[1].InnerText.Replace("\n", "").Trim();

                    string imgURL = "";
                    try
                    {
                        string imgHTML = table.ChildNodes[i].ChildNodes[j].ChildNodes[3].InnerHtml.Replace("\n", "").Trim();
                        var index = imgHTML.IndexOf("srcset");

                        string htmlrest = imgHTML.Substring(index, imgHTML.Length - 1 - index).Replace("srcset=\"", "");
                        index = htmlrest.IndexOf("?");
                        imgURL = htmlrest.Substring(0, index);


                    }
                    catch (Exception ex)
                    {
                        string imgHTML = table.ChildNodes[i].ChildNodes[j].ChildNodes[3].InnerHtml.Replace("\n", "").Trim();
                        var index = imgHTML.IndexOf("src");

                        string htmlrest = imgHTML.Substring(index, imgHTML.Length - 1 - index).Replace("src=\"", "");
                        index = htmlrest.IndexOf("?");
                        imgURL = htmlrest.Substring(0, index);
                    }
                    if (!imgURL.Contains("images") || !imgURL.Contains(".png"))
                    {
                        imgURL = "";
                    }
                    wikiImportTasks.Add(Task.Factory.StartNew(() => SetAbility(name, dirPath, "Spells_", prefixForName, 0, canActivateDuringGCD, imgURL)));
                }
            }
        }
        //return new QueryResult(elements, failedAbilities, friendlyName, endpoint, prefixForName);
        return wikiImportTasks;
    }

    //private async Task<QueryResult> GetIncantationsAbilityElements(string endpoint, string dirPath, string nodeElement, string friendlyName, string prefixForName, bool canActivateDuringGCD = true)
    private async Task<List<Task<Ability>>> GetIncantationsAbilityElements(string endpoint, string dirPath, string nodeElement, string friendlyName, string prefixForName, bool canActivateDuringGCD = true)
    {
        string response = await DownloadHTMlClient(endpoint); //string response = getHTMLCode(endpoint);

        HtmlDocument document = new();
        document.LoadHtml(response);
        HtmlNodeCollection tables = document.DocumentNode.SelectNodes(nodeElement);

        //List<Ability> elements = new List<Ability>();
        //List<Tuple<string, string>> failedAbilities = new List<Tuple<string, string>>();
        List<Task<Ability>> wikiImportTasks = new List<Task<Ability>>();

        foreach (var table in tables)
        {
            for (int i = 1; i < table.ChildNodes.Count(); i++)
            {
                for (int j = 2; j < table.ChildNodes[i].ChildNodes.Count(); j += 2)
                {
                    //Ability ability = new Ability();
                    string name = table.ChildNodes[i].ChildNodes[j].ChildNodes[1].InnerText.Replace("\n", "").Replace("&#160;", "").Trim();
                    string coolDown = "";

                    coolDown = table.ChildNodes[i].ChildNodes[j].ChildNodes[9].InnerText.Replace("\n", "").Replace("seconds", "").Trim();
                    bool itsMinutes = false;
                    if (coolDown.Contains("minute"))
                    {
                        itsMinutes = true;
                        coolDown = coolDown.Replace("minute", "");
                    }
                    double CD = 0;
                    try
                    {
                        CD = Convert.ToDouble(coolDown, CultureInfo.InvariantCulture);
                        if (itsMinutes)
                            CD = CD * 60;
                    }
                    catch { }
                    string imgURL = "";
                    try
                    {
                        string imgHTML = table.ChildNodes[i].ChildNodes[j].ChildNodes[1].InnerHtml.Replace("\n", "").Trim();
                        var index = imgHTML.IndexOf("srcset");

                        string htmlrest = imgHTML.Substring(index, imgHTML.Length - 1 - index).Replace("srcset=\"", "");
                        index = htmlrest.IndexOf("?");
                        imgURL = htmlrest.Substring(0, index);


                    }
                    catch (Exception ex)
                    {
                        string imgHTML = table.ChildNodes[i].ChildNodes[j].ChildNodes[3].InnerHtml.Replace("\n", "").Trim();
                        var index = imgHTML.IndexOf("src");

                        string htmlrest = imgHTML.Substring(index, imgHTML.Length - 1 - index).Replace("src=\"", "");
                        index = htmlrest.IndexOf("?");
                        imgURL = htmlrest.Substring(0, index);
                    }
                    if (!imgURL.Contains("images") || !imgURL.Contains(".png"))
                    {
                        imgURL = "";
                    }

                    wikiImportTasks.Add(Task.Factory.StartNew(() => SetAbility(name, dirPath, "Spells_", prefixForName, CD, canActivateDuringGCD, imgURL)));
                }
            }
        }

        //return new QueryResult(elements, failedAbilities, friendlyName, endpoint, prefixForName);
        return wikiImportTasks;
    }

    //private static async Task<QueryResult> GetOtherAbilityElements(string endpoint, string dirPath, string nodeElement, 
    //    string table, string friendlyName, string prefixForName, int childNodeIndex, bool canActivateDuringGCD = true)
    //{
    //    string response = await DownloadHTMlClient(endpoint); //string response = getHTMLCode(endpoint);

    //    HtmlDocument document = new();
    //    document.LoadHtml(response);
    //    HtmlNodeCollection tables = document.DocumentNode.SelectNodes(nodeElement);

    //    List<Ability> elements = new List<Ability>();
    //    List<Tuple<string, string>> failedAbilities = new List<Tuple<string, string>>();

    //    foreach (var tableElement in tables)
    //    {
    //        for (int i = 1; i < tableElement.ChildNodes.Count(); i++)
    //        {
    //            for (int j = 2; j < tableElement.ChildNodes[i].ChildNodes.Count(); j += 2)
    //            {
    //                string name = tableElement.ChildNodes[i].ChildNodes[j].ChildNodes[childNodeIndex].InnerText.Replace("\n", "").Trim();
    //                try
    //                {
    //                    if (string.IsNullOrWhiteSpace(name))
    //                    {
    //                        failedAbilities.Add(new Tuple<string, string>(name, "The received ability name is null or empty."));
    //                        continue;
    //                    }
    //                    elements.Add(SetAbility(name, dirPath, table, prefixForName, 0, canActivateDuringGCD));
    //                }
    //                catch (Exception ex)
    //                {
    //                    failedAbilities.Add(new Tuple<string, string>(name, ex.Message));
    //                }
    //            }
    //        }
    //    }

    //    return new QueryResult(elements, failedAbilities, friendlyName, endpoint, prefixForName);
    //}

    private Ability SetAbility(string abilityName, string dirPath, string table, string prefixForName, double cooldown = 0, bool canActivateDuringGCD = false, string imgURL = "")
    {
        Ability ability = new Ability();
        string destinationPath = "";

        if (string.IsNullOrEmpty(imgURL))
        {
            if (table.Equals("Spells_"))
                destinationPath = SaveImage(prefixForName, abilityName, dirPath, "_icon");
            else
                destinationPath = SaveImage(prefixForName, abilityName, dirPath);
        }
        else
        {
            destinationPath = SaveImageFromUrl(prefixForName, abilityName, imgURL, dirPath);
        }

        if (string.IsNullOrEmpty(destinationPath))
            return null;

        //string img = table.ChildNodes[i].ChildNodes[2].ChildNodes[3].InnerText.Replace("\n", "");
        if (string.IsNullOrEmpty(prefixForName))
            ability.Name = abilityName;
        else
            ability.Name = prefixForName + ": " + abilityName;

        ability.FriendlyName = table + ability.Name + "_WikiImport";
        ability.CooldownInSec = cooldown; //ggf. stattdessen in ms?
        ability.Img = destinationPath;
        ability.CanActivateDuringGCD = canActivateDuringGCD;
        return ability;
    }

    private string SaveImage(string fileNamePrefix, string abilityName, string dirPath, string abilitySuffix = "")
    {
        using (CustomTimeoutWebClient client = new CustomTimeoutWebClient())
        {
            //if (IsFileLocked(destinationPath)) return null;

            string serverFileName = $"{abilityName.Replace(" ", "_")}{abilitySuffix}.png";

            //Remove all "_"-Symbols, because the UI Controls otherwise can't load the Images
            string destinationPath = Path.Combine(dirPath, fileNamePrefix + serverFileName);
            //string destinationPath = Path.Combine(dirPath, fileNamePrefix.Replace("_", "") + serverFileName.Replace("_", ""));
            //string destinationPath = Path.Combine(dirPath, abilityName + ".png");

            //if (abilityName == "Cease" || abilityName == "Regenerate" || abilityName.Contains("Destroy"))
            if (abilityName.Contains("Destroy"))
                serverFileName = $"{abilityName.Replace(" ", "_")}_(ability).png";

            client.Headers.Add("User-Agent", GlobalApplicationSettings.OtherSettings.UserAgent_WikiAPICalls);
            string url = "http://runescape.wiki/images/" + serverFileName; // "http://runescape.wiki/images/Havoc.png"

            if (IsFileLocked(destinationPath))
            {
                return "";
            }

            //client.DownloadFile(new Uri(url), destinationPath);

            if(url != null && destinationPath != null) 
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                try
                {
                    client.DownloadFile(new Uri(url), destinationPath);
                }
                catch (Exception ex)
                {
                    try
                    {
                        url = url.Replace(".png", "_(Ability).png");
                        client.DownloadFile(new Uri(url), destinationPath);
                    }
                    catch (Exception ex2)
                    {
                        try
                        {

                            url = url.Replace(".png", "_(ability).png");
                            client.DownloadFile(new Uri(url), destinationPath);
                        }
                        catch (Exception ex3)
                        {
                            // ToDo Logging
                        }
                    }
                }
            }
            return destinationPath;
        }
    }

    private string SaveImageFromUrl(string fileNamePrefix, string name, string endpoint, string dirPath)
    {
        if(!string.IsNullOrWhiteSpace(fileNamePrefix))
            name = $"{fileNamePrefix}_{name}";

        string finalName = name.Replace(" ", "_");

        if (File.Exists(@".\Images\Abilities\" + name.Replace(" ", "_") + ".png"))
        {
            return Path.Combine(dirPath, name.Replace(" ", "_") + ".png");
        }
        if (IsFileLocked(@".\Images\Abilities\" + name.Replace(" ", "_") + ".png"))
        {
            return "";
        }

        string url = "https://runescape.wiki" + endpoint;
        using (CustomTimeoutWebClient client = new CustomTimeoutWebClient())
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            client.Headers.Add("user-agent", "PostmanRuntime/7.26.1");
            try
            {
                client.Headers.Add("user-agent", "PostmanRuntime/7.26.1");
                client.DownloadFile(new Uri(url), @".\Images\Abilities\" + name.Replace(" ", "_") + ".png");
            }
            catch (Exception ex)
            {
                try
                {
                    finalName = name.Replace(" ", "_") + "_(Ability)";
                    url = "https://runescape.wiki/images/" + finalName + ".png";
                    client.Headers.Add("user-agent", "PostmanRuntime/7.26.1");
                    client.DownloadFile(new Uri(url), @".\Images\Abilities\" + name.Replace(" ", "_") + ".png");
                }
                catch (Exception ex2)
                {
                    try
                    {

                        finalName = name.Replace(" ", "_") + "_(ability)";
                        url = "https://runescape.wiki/images/" + finalName + ".png";
                        client.Headers.Add("user-agent", "PostmanRuntime/7.26.1");
                        client.DownloadFile(new Uri(url), @".\Images\Abilities\" + name.Replace(" ", "_") + ".png");
                    }
                    catch (Exception ex3)
                    {
                        // ToDo Logging
                    }
                }
            }

        }

        return Path.Combine(dirPath, name.Replace(" ", "_") + ".png");
    }

    private Dictionary<int, string> GetCategoryMapping()
    {
        Dictionary<int, string> categoryMapping = new Dictionary<int, string>();
        categoryMapping.Add(0, "Melee_");
        categoryMapping.Add(1, "Melee_");
        categoryMapping.Add(2, "Mage_");
        categoryMapping.Add(3, "Range_");
        categoryMapping.Add(4, "Defense_");
        categoryMapping.Add(5, "Consition");
        return categoryMapping;
    }

    const int ERROR_SHARING_VIOLATION = 32;
    const int ERROR_LOCK_VIOLATION = 33;
    private bool IsFileLocked(string filePath)
    {
        if (File.Exists(filePath))
        {
            try
            {
                FileInfo file = new FileInfo(filePath);
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException exception)
            {
                int errorCode = System.Runtime.InteropServices.Marshal.GetHRForException(exception) & ((1 << 16) - 1);
                if (errorCode == ERROR_SHARING_VIOLATION || errorCode == ERROR_LOCK_VIOLATION) return true;
            }
        }

        return false;
    }
}

public class CustomTimeoutWebClient : WebClient
{
    protected override WebRequest GetWebRequest(Uri uri)
    {
        WebRequest w = base.GetWebRequest(uri);
        w.Timeout = 20 * 60 * 1000;
        return w;
    }
}