using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using BloodBoard.GameManagement;
using BloodBoard.UI;

public class PlayerHealth : MonoBehaviour
{
    private static bool persistedHealthAvailable = false;
    private static float persistedHealth = 0f;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public Slider healthBarUI;

    [Header("Damage Feedback")]
    public Image damageFlashImage;
    public float flashSpeed = 5f;
    public Color flashColor = new Color(1f, 0f, 0f, 0.4f);

    [Header("Directional Damage (Nuevo)")]
    public RectTransform directionalIndicator;
    public Image directionalImage;

    [Header("Damage Scaling")]
    public float comboWindow = 1.0f;
    public float mitigationFactor = 0.5f;
    public float minDamageMultiplier = 0.2f;

    private float lastHitTime = -999f;
    private float currentDamageMultiplier = 1f;

    void Start()
    {
        if (persistedHealthAvailable)
        {
            currentHealth = Mathf.Min(persistedHealth, maxHealth);
        }
        else
        {
            currentHealth = maxHealth;
        }

        PersistHealth(currentHealth);

        if (healthBarUI != null)
        {
            healthBarUI.maxValue = maxHealth;
            healthBarUI.value = currentHealth;
        }

        if (damageFlashImage != null)
        {
            damageFlashImage.color = Color.clear;
        }

        if (directionalImage != null)
        {
            directionalImage.color = Color.clear;
        }
    }

    void Update()
    {
        if (damageFlashImage != null && damageFlashImage.color != Color.clear)
        {
            damageFlashImage.color = Color.Lerp(damageFlashImage.color, Color.clear, flashSpeed * Time.deltaTime);
        }

        if (directionalImage != null && directionalImage.color != Color.clear)
        {
            directionalImage.color = Color.Lerp(directionalImage.color, Color.clear, flashSpeed * Time.deltaTime);
        }
    }

    public void TakeDamage(float amount, Transform attacker = null)
    {
        if (Time.time - lastHitTime > comboWindow)
        {
            currentDamageMultiplier = 1f;
        }
        else
        {
            currentDamageMultiplier *= mitigationFactor;
            currentDamageMultiplier = Mathf.Max(currentDamageMultiplier, minDamageMultiplier);
        }

        float finalDamage = amount * currentDamageMultiplier;
        currentHealth -= finalDamage;
        lastHitTime = Time.time;

        if (healthBarUI != null) healthBarUI.value = currentHealth;
        PersistHealth(currentHealth);
        if (damageFlashImage != null) damageFlashImage.color = flashColor;

        if (attacker != null && directionalIndicator != null && directionalImage != null)
        {
            directionalImage.color = flashColor;

            Vector3 dirToAttacker = attacker.position - transform.position;
            dirToAttacker.y = 0;

            float angle = Vector3.SignedAngle(transform.forward, dirToAttacker, Vector3.up);

            directionalIndicator.localEulerAngles = new Vector3(0, 0, -angle);
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
                // No sobrescribimos el save slot con la salud al morir.
                // El retry debe restaurar la salud del checkpoint anterior al piso.
                Object.FindFirstObjectByType<GameOver>()?.ShowGameOver();
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
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        PersistHealth(currentHealth);
        if (healthBarUI != null) healthBarUI.value = currentHealth;
    }

    private static void PersistHealth(float health)
    {
        persistedHealth = Mathf.Max(0f, health);
        persistedHealthAvailable = true;
    }

    public static void ResetPersistedHealth()
    {
        persistedHealthAvailable = false;
        persistedHealth = 0f;
    }

    public static void SetPersistedHealth(float health)
    {
        persistedHealth = Mathf.Clamp(health, 0f, float.MaxValue);
        persistedHealthAvailable = true;
    }
}
