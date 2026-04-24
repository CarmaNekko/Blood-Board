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
            Object.FindFirstObjectByType<GameOver>().ShowGameOver();
        }
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
}
