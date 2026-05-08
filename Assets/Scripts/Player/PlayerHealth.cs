using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using BloodBoard.GameManagement;
using BloodBoard.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public Slider healthBarUI;
    void Start()
    {
        currentHealth = maxHealth;
        if (healthBarUI != null)
        {
            healthBarUI.maxValue = maxHealth;
            healthBarUI.value = currentHealth;
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (healthBarUI != null)
        {
            healthBarUI.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            int currentFloor = LevelManager.currentLevel;
            float finalHealth = currentHealth;
            int finalScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetCurrentScore() : 0;

            if (GameModeManager.CurrentMode is EndlessMode)
            {
                EndlessScoreInputUI.Instance?.Show(currentFloor, finalScore, finalHealth);
            }
            else
            {
                SaveManager.SaveToSlot(GameModeManager.CurrentSlot, currentFloor, finalScore, finalHealth, GameModeManager.CurrentMode.GetModeName());
                Debug.Log("Guardando al morir en piso: " + currentFloor);
                Object.FindFirstObjectByType<GameOver>().ShowGameOver();
            }
        }
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
    public void RestoreHealth(float amount)
    {
        currentHealth += amount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        if (healthBarUI != null)
        {
            healthBarUI.value = currentHealth;
        }
    }
}
