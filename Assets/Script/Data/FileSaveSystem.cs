using System.IO;
using UnityEngine;

public static class FileSaveSystem
{
    private static readonly string SaveFolder = Application.persistentDataPath + "/Saves";

    public static void SaveToFile(string filename, object data)
    {
        if (!Directory.Exists(SaveFolder))
            Directory.CreateDirectory(SaveFolder);

        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine(SaveFolder, filename + ".json");
        File.WriteAllText(path, json);
        Debug.Log($"[SAVE] File saved at: {path}\nData:\n{json}");
    }

    public static T LoadFromFile<T>(string filename)
    {
        string path = Path.Combine(SaveFolder, filename + ".json");
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Save file not found: {path}");
            return default;
        }

        string json = File.ReadAllText(path);
        T data = JsonUtility.FromJson<T>(json);

        Debug.Log($"[LOAD] File loaded from: {path}\nData:\n{json}");

        return data;
    }
}
