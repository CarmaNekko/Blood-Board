using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using BloodBoard.GameManagement; // Added for ScoreManager
using BloodBoard.UI; // Added for EndlessScoreInputUI

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
            // Save on death
            int currentFloor = LevelManager.currentLevel; // Get current floor before potential scene load
            float finalHealth = currentHealth; // Use currentHealth as final health
            int finalScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetCurrentScore() : 0;

            if (GameModeManager.CurrentMode is EndlessMode)
            {
                // For Endless mode, prompt for name before showing game over
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
