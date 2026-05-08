using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int floor;
    public int score;
    public float health;
    public string mode;
}

public abstract class SaveSystem
{
    public abstract void Save(int slot, int floor, int score, float health, string mode);
    public abstract SaveData Load(int slot);
    public abstract void Delete(int slot);
    public abstract void DeleteAll();
    public abstract bool IsSlotEmpty(int slot);
}

public class PlayerPrefsSaveSystem : SaveSystem
{
    private string GetKey(int slot)
    {
        return $"Save_Slot{slot}";
    }

    public override void Save(int slot, int floor, int score, float health, string mode)
    {
        SaveData data = new SaveData { floor = floor, score = score, health = health, mode = mode };
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(GetKey(slot), json);
        PlayerPrefs.Save();
    }

    public override SaveData Load(int slot)
    {
        string key = GetKey(slot);
        if (PlayerPrefs.HasKey(key))
        {
            string json = PlayerPrefs.GetString(key);
            return JsonUtility.FromJson<SaveData>(json);
        }
        return null;
    }

    public override void Delete(int slot)
    {
        PlayerPrefs.DeleteKey(GetKey(slot));
    }

    public override void DeleteAll()
    {
        for (int i = 1; i <= 3; i++) Delete(i);
    }

    public override bool IsSlotEmpty(int slot)
    {
        return !PlayerPrefs.HasKey(GetKey(slot));
    }
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    private static SaveSystem saveSystem;

    static SaveManager()
    {
        saveSystem = new PlayerPrefsSaveSystem();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void SaveToSlot(int slot, int floor, int score, float health, string mode)
    {
        saveSystem.Save(slot, floor, score, health, mode);
    }

    public static SaveData LoadFromSlot(int slot)
    {
        return saveSystem.Load(slot);
    }

    public static void DeleteSlot(int slot)
    {
        saveSystem.Delete(slot);
    }

    public static void DeleteAllSlots()
    {
        saveSystem.DeleteAll();
    }

    public static bool IsSlotEmpty(int slot)
    {
        return saveSystem.IsSlotEmpty(slot);
    }

    public static void SaveGame(int floor, int score, float health)
    {
    }

    public static SaveData LoadGame(int floor)
    {
        return null;
    }

    public static List<int> GetAvailableSaves()
    {
        List<int> availableSaves = new List<int>();
        for (int i = 1; i <= 3; i++)
        {
            if (!IsSlotEmpty(i))
            {
                availableSaves.Add(i);
            }
        }
        return availableSaves;
    }

    public static void DeleteSave(int floor)
    {
    }
}
