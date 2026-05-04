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

    [Header("Buff Status")]
    public bool isBuffed { get; private set; } = false;
    [SerializeField] private GameObject frenzyParticles;

    void Start()
    {
        currentHealth = maxHealth;

        if (frenzyParticles != null)
        {
            frenzyParticles.SetActive(false);
        }
    }

    public void TakeDamage(int damageAmount, MagicColor incomingMagicColor)
    {
        if (myColor != incomingMagicColor)
        {
            currentHealth -= damageAmount;

            if (currentHealth <= 0)
            {
                Die();
            }
        }
        else
        {
            ApplyBuff();
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
}