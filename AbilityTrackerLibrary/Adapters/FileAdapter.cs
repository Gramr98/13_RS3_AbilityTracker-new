using Newtonsoft.Json;

namespace AbilityTrackerLibrary;

public class FileAdapter<T> // where T : INamedObject
{
    protected List<T> _tList = new List<T>();
    protected List<Icon> _tIconList = new List<Icon>();

    public List<T> TList { get { return _tList; } }
    //public List<Icon> TIconList { get { return _tIconList?.OrderBy(e => e.IconFileName).ToList(); } }
    public List<Icon> TIconList { get { return _tIconList.ToList(); } }

    public FileAdapter(string jsonDataPath, bool savePreviousItemsData = true)
    {
        if (savePreviousItemsData && !string.IsNullOrWhiteSpace(jsonDataPath))
            LoadJsonData(jsonDataPath);
    }

    public FileAdapter(string imagesDirPath, string jsonDataPath) : this(jsonDataPath)
    {
        if(!string.IsNullOrWhiteSpace(imagesDirPath))
            LoadIconImages(imagesDirPath);
    }

    public virtual void SaveNewObject(T obj, string TJsonPath)
    {
        if (obj == null) throw new ArgumentNullException($"The received Object of Type {typeof(T)} is NULL");
        if (string.IsNullOrEmpty(TJsonPath)) throw new ArgumentNullException($"The given Json-Filepath for the Object {typeof(T)} is NULL or Empty");

        _tList.Add(obj);

        SaveData(TJsonPath);
    }

    public virtual void SaveNewObjects(List<T> objects, string TJsonPath)
    {
        if (objects == null) throw new ArgumentNullException($"The received object list of Type List<{typeof(T)}> is NULL");

        _tList = objects;

        SaveData(TJsonPath);
    }

    public void DeleteTObject(T objToDelete, string TJsonPath)
    {
        _tList.RemoveAll(obj => obj.Equals(objToDelete));
        SaveData(TJsonPath);
    }

    public void DeleteTObjects(List<T> objsToDelete, string TJsonPath)
    {
        foreach (T objToDelete in objsToDelete)
            _tList.RemoveAll(obj => obj.Equals(objToDelete));

        SaveData(TJsonPath);
    }

    public void DeleteTFileContent(string TJsonPath, bool doEmptyList = false)
    {
        if (doEmptyList) _tList.Clear();

        if (File.Exists(TJsonPath))
            File.Delete(TJsonPath);
    }

    public void SaveData(string TJsonPath)
    {
        DeleteTFileContent(TJsonPath);

        using (FileStream stream = File.Create(TJsonPath))
            stream.Close();

        string json = JsonConvert.SerializeObject(_tList, Formatting.Indented);
        File.WriteAllText(TJsonPath, json);
    }

    public void ReplaceTList(List<T> replaceList)
    {
        _tList = replaceList;
    }

    protected void LoadJsonData(string TJsonPath)
    {
        if (string.IsNullOrEmpty(TJsonPath)) throw new ArgumentNullException($"The given Json-Filepath for the Object {typeof(T)} is NULL or Empty");

        if (File.Exists(TJsonPath) == false)
            File.Create(TJsonPath).Close();
        else
            _tList = JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(TJsonPath)) ?? _tList;
    }

    protected void LoadIconImages(string imagesDirPath)
    {
        if (string.IsNullOrEmpty(imagesDirPath)) throw new ArgumentNullException("The given DirectoyPath for the Images is NULL or Empty");

        if (Directory.Exists(imagesDirPath) == false) Directory.CreateDirectory(imagesDirPath);

        foreach (string file in Directory.EnumerateFiles(imagesDirPath))
            _tIconList.Add(new Icon(file));
    }
}
