using Newtonsoft.Json;

public class FileAdapter<T> // where T : INamedObject
{
    public List<T> TList { get; set; }

    public void SaveData(string TJsonPath)
    {
        using (FileStream stream = File.Create(TJsonPath))
            stream.Close();

        string json = JsonConvert.SerializeObject(TList, Formatting.Indented);
        File.WriteAllText(TJsonPath, json);
    }

    public void LoadJsonData(string TJsonPath)
    {
        TList = JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(TJsonPath)) ?? TList;
    }
}
