using UnityEngine;
using UnityEngine.SceneManagement;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    private static bool persistedCoinsAvailable = false;
    private static int persistedCoins = 0;

    public int Coins = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        ApplyPersistedCoins();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyPersistedCoins();
    }

    private void ApplyPersistedCoins()
    {
        if (persistedCoinsAvailable)
        {
            Coins = persistedCoins;
            persistedCoinsAvailable = false;
        }
    }

    public static void SetPersistedCoins(int coins)
    {
        persistedCoins = coins;
        persistedCoinsAvailable = true;
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
        Debug.Log("Coins: " + Coins);
    }

    public bool CanAffordCoins(int amount)
    {
        return Coins >= amount;
    }

    public bool SpendCoins(int amount)
    {
        if (Coins >= amount)
        {
            Coins -= amount;
            Debug.Log("Coins: " + Coins);
            return true;
        }

        Debug.Log("Not enough coins!");
        return false;
    }
}
