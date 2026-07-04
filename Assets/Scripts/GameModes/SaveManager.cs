using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int floor;
    public int score;
    public float health;
    public string mode;
    public string checkpointScene;
    public bool isBossCheckpoint;
    public string bossDisplayName;
    public int powerUpHealth;
    public int powerUpDamage;
    public int powerUpSpeed;
    public bool hasAnomalousSoul;
    public bool hasSlashAttack;
    public bool hasVortexAttack;
    public bool hasVampirism;
    public bool hasBulletRain;
    public bool hasEchoShot;
    public float echoShotChancePercent;
    public int coins;
}

public abstract class SaveSystem
{
    public abstract void Save(int slot, int floor, int score, float health, string mode);
    public abstract void Save(int slot, SaveData data);
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
        SaveData existingData = Load(slot);
        SaveData data = new SaveData
        {
            floor = floor,
            score = score,
            health = health,
            mode = mode,
            checkpointScene = floor == 0 ? BossCheckpointState.TutorialScene : BossCheckpointState.DefaultLevelScene,
            isBossCheckpoint = false,
            bossDisplayName = string.Empty,
            powerUpHealth = existingData != null ? existingData.powerUpHealth : 0,
            powerUpDamage = existingData != null ? existingData.powerUpDamage : 0,
            powerUpSpeed = existingData != null ? existingData.powerUpSpeed : 0,
            hasAnomalousSoul = existingData != null ? existingData.hasAnomalousSoul : false, hasSlashAttack = existingData != null ? existingData.hasSlashAttack : false, hasVortexAttack = existingData != null ? existingData.hasVortexAttack : false, hasVampirism = existingData != null ? existingData.hasVampirism : false, hasBulletRain = existingData != null ? existingData.hasBulletRain : false, hasEchoShot = existingData != null ? existingData.hasEchoShot : false, echoShotChancePercent = existingData != null ? existingData.echoShotChancePercent : 10f,
            coins = CoinManager.Instance != null ? CoinManager.Instance.Coins : (existingData != null ? existingData.coins : 0)
        };
        Save(slot, data);
    }

    public override void Save(int slot, SaveData data)
    {
        if (data == null)
        {
            return;
        }

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
    private static int currentSlot = 1;

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

    public static void SetCurrentSlot(int slot)
    {
        currentSlot = slot;
    }

    public static int GetCurrentSlot()
    {
        return GameModeManager.CurrentSlot;
    }

    public static void SaveToSlot(int slot, int floor, int score, float health, string mode)
    {
        SaveData existingData = LoadFromSlot(slot);
        SaveData data = new SaveData
        {
            floor = floor,
            score = score,
            health = health,
            mode = mode,
            checkpointScene = floor == 0 ? BossCheckpointState.TutorialScene : BossCheckpointState.DefaultLevelScene,
            isBossCheckpoint = false,
            bossDisplayName = string.Empty,
            powerUpHealth = existingData != null ? existingData.powerUpHealth : 0,
            powerUpDamage = existingData != null ? existingData.powerUpDamage : 0,
            powerUpSpeed = existingData != null ? existingData.powerUpSpeed : 0,
            hasAnomalousSoul = existingData != null ? existingData.hasAnomalousSoul : false,
            hasSlashAttack = existingData != null ? existingData.hasSlashAttack : false,
            hasVortexAttack = existingData != null ? existingData.hasVortexAttack : false,
            hasVampirism = existingData != null ? existingData.hasVampirism : false,
            hasBulletRain = existingData != null ? existingData.hasBulletRain : false,
            hasEchoShot = existingData != null ? existingData.hasEchoShot : false,
            echoShotChancePercent = existingData != null ? existingData.echoShotChancePercent : 10f,
            coins = CoinManager.Instance != null ? CoinManager.Instance.Coins : (existingData != null ? existingData.coins : 0)
        };
        saveSystem.Save(slot, data);
        BossCheckpointState.SetLevelCheckpoint();
    }

    public static void SaveNewGameSlot(int slot, int floor, int score, float health, string mode)
    {
        SaveData data = new SaveData
        {
            floor = floor,
            score = score,
            health = health,
            mode = mode,
            checkpointScene = floor == 0 ? BossCheckpointState.TutorialScene : BossCheckpointState.DefaultLevelScene,
            isBossCheckpoint = false,
            bossDisplayName = string.Empty,
            powerUpHealth = 0,
            powerUpDamage = 0,
            powerUpSpeed = 0,
            hasAnomalousSoul = false,
            hasSlashAttack = false,
            hasVortexAttack = false,
            hasVampirism = false,
            hasBulletRain = false,
            hasEchoShot = false,
            echoShotChancePercent = 10f,
            coins = 0
        };
        saveSystem.Save(slot, data);
        BossCheckpointState.SetLevelCheckpoint();
    }

    public static void SaveBossCheckpointToSlot(int slot, int floor, int score, float health, string mode, string bossSceneName, string bossDisplayName)
    {
        SaveData existingData = LoadFromSlot(slot);
        SaveData data = new SaveData
        {
            floor = floor,
            score = score,
            health = health,
            mode = mode,
            checkpointScene = bossSceneName,
            isBossCheckpoint = true,
            bossDisplayName = bossDisplayName,
            powerUpHealth = existingData != null ? existingData.powerUpHealth : 0,
            powerUpDamage = existingData != null ? existingData.powerUpDamage : 0,
            powerUpSpeed = existingData != null ? existingData.powerUpSpeed : 0,
            hasAnomalousSoul = existingData != null ? existingData.hasAnomalousSoul : false,
            hasSlashAttack = existingData != null ? existingData.hasSlashAttack : false,
            hasVortexAttack = existingData != null ? existingData.hasVortexAttack : false,
            hasVampirism = existingData != null ? existingData.hasVampirism : false,
            hasBulletRain = existingData != null ? existingData.hasBulletRain : false,
            hasEchoShot = existingData != null ? existingData.hasEchoShot : false,
            echoShotChancePercent = existingData != null ? existingData.echoShotChancePercent : 10f,
            coins = CoinManager.Instance != null ? CoinManager.Instance.Coins : (existingData != null ? existingData.coins : 0)
        };

        saveSystem.Save(slot, data);
        BossCheckpointState.SetBossCheckpoint(bossDisplayName, bossSceneName);
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

    public static void BuyPowerUp(string powerUpType, int cost)
    {
        int slot = GameModeManager.CurrentSlot;
        SaveData data = LoadFromSlot(slot);
        if (data == null)
            return;

        if (data.score >= cost)
        {
            data.score -= cost;

            switch(powerUpType)
            {
                case "health":
                    data.powerUpHealth++;
                    break;
                case "damage":
                    data.powerUpDamage++;
                    break;
                case "speed":
                    data.powerUpSpeed++;
                    break;
            }

            saveSystem.Save(slot, data);
        }
    }

    public static int GetPowerUpCount(string powerUpType)
    {
        int slot = GameModeManager.CurrentSlot;
        SaveData data = LoadFromSlot(slot);
        if (data == null)
            return 0;

        switch(powerUpType)
        {
            case "health":
                return data.powerUpHealth;
            case "damage":
                return data.powerUpDamage;
            case "speed":
                return data.powerUpSpeed;
            default:
                return 0;
        }
    }

    public static void SavePowerUpPurchase(string powerUpType)
    {
        int slot = GameModeManager.CurrentSlot;
        SaveData data = LoadFromSlot(slot);
        if (data == null)
            return;

        // Guarda el estado actual de las monedas junto con la compra del power-up
        if (CoinManager.Instance != null)
        {
            data.coins = CoinManager.Instance.Coins;
        }

        switch(powerUpType)
        {
            case "health":
                data.powerUpHealth++;
                break;
            case "damage":
                data.powerUpDamage++;
                break;
            case "speed":
                data.powerUpSpeed++;
                break;
            case "anomalousSoul":
                data.hasAnomalousSoul = true;
                break;
            case "slashAttack":
                data.hasSlashAttack = true;
                data.hasVortexAttack = false;
                break;
            case "vortexAttack":
                data.hasVortexAttack = true;
                data.hasSlashAttack = false;
                break;
            case "vampirism":
                data.hasVampirism = true;
                break;
            case "bulletRain":
                data.hasBulletRain = true;
                break;
            case "echoShot":
                data.hasEchoShot = true;
                break;
        }

        saveSystem.Save(slot, data);
    }

    public static void SavePowerUpState(MagicShooter shooter)
    {
        int slot = GameModeManager.CurrentSlot;
        SaveData data = LoadFromSlot(slot);
        if (data == null) return;

        if (shooter.HasAnomalousSoul()) data.hasAnomalousSoul = true;
        if (shooter.HasAnyChargedAttack())
        {
            if (shooter.GetChargedAttackType() == MagicShooter.ChargedAttackType.Slash)
                data.hasSlashAttack = true;
            else if (shooter.GetChargedAttackType() == MagicShooter.ChargedAttackType.Vortex)
                data.hasVortexAttack = true;
        }
        if (shooter.HasVampirism()) data.hasVampirism = true;
        if (shooter.HasBulletRain()) data.hasBulletRain = true;
        if (shooter.HasEchoShot())
        {
            data.hasEchoShot = true;
            data.echoShotChancePercent = shooter.GetEchoShotChancePercent();
        }

        saveSystem.Save(slot, data);
    }
}