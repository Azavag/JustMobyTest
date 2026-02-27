using UnityEngine;
using System.IO;

public static class TowerSaveSystem
{
    private static string SaveFileName => Path.Combine(Application.persistentDataPath, "tower_save.json");

    public static void SaveTower(TowerSaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SaveFileName, json);
        Debug.Log($"Башня сохранена: {SaveFileName}");
    }

    public static TowerSaveData LoadTower()
    {
        if (!File.Exists(SaveFileName))
            return new TowerSaveData(); // пустая башня

        string json = File.ReadAllText(SaveFileName);
        TowerSaveData data = JsonUtility.FromJson<TowerSaveData>(json);
        Debug.Log($"Башня загружена: {SaveFileName}");
        return data;
    }

    public static void DeleteSave()
    {
        if (File.Exists(SaveFileName))
            File.Delete(SaveFileName);
    }
}