using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
            int currentFloor = LevelManager.currentLevel;
            SaveManager.SaveToSlot(GameModeManager.CurrentSlot, currentFloor, 0, currentHealth, GameModeManager.CurrentMode.GetModeName()); // Approximate score, actual health
            Debug.Log("Guardando al morir en piso: " + currentFloor);
            Object.FindFirstObjectByType<GameOver>().ShowGameOver();
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
