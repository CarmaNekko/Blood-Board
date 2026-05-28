using BloodBoard.GameManagement;
using UnityEngine;
using UnityEngine.UI;

public class PawnBossHealth : MonoBehaviour
{
    [Header("Polarity & Health")]
    public MagicColor myColor;
    public int maxHealth = 300;
    public int currentHealth;

    [Header("Score System")]
    public int scoreValue = 1000;

    [Header("UI (Interfaz)")]
    public Slider healthBarUI;

    public bool isDead { get; private set; } = false;

    private PawnBossController bossController;
    private bool phase2Triggered = false;
    private bool phase3Triggered = false;

    void Start()
    {
        currentHealth = maxHealth;
        bossController = GetComponent<PawnBossController>();

        if (healthBarUI != null)
        {
            healthBarUI.maxValue = maxHealth;
            healthBarUI.value = currentHealth;
        }
    }

    void Update()
    {
        if (isDead) return;

        CheckPhases();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void TakeDamage(int damageAmount, MagicColor incomingMagicColor)
    {
        if (bossController == null || !bossController.IsFatigued()) return;

        if (myColor != incomingMagicColor)
        {
            currentHealth -= damageAmount;

            if (healthBarUI != null)
            {
                healthBarUI.value = currentHealth;
            }
        }
    }

    private void CheckPhases()
    {
        if (bossController == null) return;

        float healthPercent = (float)currentHealth / maxHealth;

        if (healthPercent <= 0.66f && !phase2Triggered)
        {
            phase2Triggered = true;
            bossController.AdvancePhase();
        }
        if (healthPercent <= 0.33f && !phase3Triggered)
        {
            phase3Triggered = true;
            bossController.AdvancePhase();
        }
    }

    private void Die()
    {
        isDead = true;
        ScoreManager.Instance?.AddScoreToCurrent(scoreValue);

        if (healthBarUI != null) healthBarUI.value = 0;

        if (bossController != null) bossController.SetDefeated();
    }
}