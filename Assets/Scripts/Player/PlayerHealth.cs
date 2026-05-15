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

    [Header("Damage Feedback")]
    public Image damageFlashImage;
    public float flashSpeed = 5f;
    public Color flashColor = new Color(1f, 0f, 0f, 0.4f); // Rojo al 40% de opacidad

    void Start()
    {
        currentHealth = maxHealth;
        if (healthBarUI != null)
        {
            healthBarUI.maxValue = maxHealth;
            healthBarUI.value = currentHealth;
        }

        if (damageFlashImage != null)
        {
            damageFlashImage.color = Color.clear; // Empieza transparente
        }
    }

    void Update()
    {
        // Si la pantalla está roja, la vamos aclarando poco a poco
        if (damageFlashImage != null && damageFlashImage.color != Color.clear)
        {
            damageFlashImage.color = Color.Lerp(damageFlashImage.color, Color.clear, flashSpeed * Time.deltaTime);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (healthBarUI != null)
        {
            healthBarUI.value = currentHealth;
        }

        // === ¡EL FLASH DE DAÑO! ===
        if (damageFlashImage != null)
        {
            damageFlashImage.color = flashColor;
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