using UnityEngine;

public class GameSaveManager : MonoBehaviour
{
    public void SaveAll()
    {
        foreach (var saveable in SaveableRegistry.GetAll())
            saveable.Save();

        var sceneObjects = FindObjectsOfType<MonoBehaviour>(true);
        foreach (var obj in sceneObjects)
        {
            if (obj is ISaveableBehaviour saveable)
                saveable.Save();
        }

        Debug.Log("All saveables saved to file.");
    }

    public void LoadAll()
    {
        foreach (var saveable in SaveableRegistry.GetAll())
            saveable.Load();

        var sceneObjects = FindObjectsOfType<MonoBehaviour>(true);
        foreach (var obj in sceneObjects)
        {
            if (obj is ISaveableBehaviour saveable)
                saveable.Load();
        }

        Debug.Log("All saveables loaded from file.");
    }
}
