using BloodBoard.GameManagement;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Polarity & Health")]
    public MagicColor myColor;
    public int maxHealth = 10;
    private int currentHealth;

    [Header("Score System")]
    public int scoreValue = 100;

    [Header("Protection")]
    public bool isShielded = false;
    [SerializeField] private GameObject shieldVisual;

    [Header("Buff Status")]
    public bool canBeBuffed = true;
    public bool isBuffed { get; private set; } = false;
    [SerializeField] private GameObject frenzyParticles;


    void Awake()
    {
        currentHealth = maxHealth;

        if (frenzyParticles != null)
        {
            frenzyParticles.SetActive(false);
        }
        if (shieldVisual != null) shieldVisual.SetActive(false);
    }

    public void TakeDamage(int damageAmount, MagicColor incomingMagicColor)
    {
        if (isShielded) return;

        if (myColor != incomingMagicColor)
        {
            currentHealth -= damageAmount;
            if (currentHealth <= 0) Die();
        }
        else
        {
            if (canBeBuffed)
            {
                ApplyBuff();
            }
        }
    }

    private void ApplyBuff()
    {
        if (isBuffed) return;

        isBuffed = true;

        transform.localScale *= 1.25f;
        maxHealth *= 2;
        currentHealth = maxHealth;

        if (frenzyParticles != null)
        {
            frenzyParticles.SetActive(true);
        }
    }

    private void Die()
    {
        ScoreManager.Instance?.AddScoreToCurrent(scoreValue);
        Destroy(gameObject);
    }
    public void SetShield(bool status)
    {
        isShielded = status;
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(status);
        }
    }
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}