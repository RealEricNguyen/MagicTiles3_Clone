using System.Collections.Generic;
using System.Linq;

public static class SaveableRegistry
{
    private static readonly object lockObject = new object();
    private static List<ISaveableBehaviour> saveables = new List<ISaveableBehaviour>();

    public static void Register(ISaveableBehaviour saveable)
    {
        if (saveable == null) return;
        
        lock (lockObject)
        {
            if (!saveables.Contains(saveable))
                saveables.Add(saveable);
        }
    }

    public static void Unregister(ISaveableBehaviour saveable)
    {
        if (saveable == null) return;
        
        lock (lockObject)
        {
            if (saveables.Contains(saveable))
                saveables.Remove(saveable);
        }
    }

    public static IEnumerable<ISaveableBehaviour> GetAll()
    {
        lock (lockObject)
        {
            return saveables.ToList(); // Return a copy to avoid modification during iteration
        }
    }

    public static void Clear()
    {
        lock (lockObject)
        {
            saveables.Clear();
        }
    }
}