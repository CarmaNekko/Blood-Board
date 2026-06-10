using BloodBoard.GameManagement;
using System.Collections;
using UnityEngine;

[System.Serializable]
public struct DropItem
{
    public GameObject prefab;
    [Range(0f, 1f)]
    public float dropChance;
}

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

    [Header("Drops")]
    [SerializeField] private DropItem[] possibleDrops;

    private bool isBleeding = false;

    void Awake()
    {
        currentHealth = maxHealth;

        if (frenzyParticles != null)
        {
            frenzyParticles.SetActive(false);
        }
        if (shieldVisual != null) shieldVisual.SetActive(false);
    }

    public void TakeDamage(int damageAmount, MagicColor incomingMagicColor, bool applyBleed = false)
    {
        if (isShielded) return;

        if (myColor != incomingMagicColor || incomingMagicColor == MagicColor.Harmonic)
        {
            currentHealth -= damageAmount;

            if (applyBleed && !isBleeding && currentHealth > 0)
            {
                StartCoroutine(BleedRoutine());
            }

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

    private IEnumerator BleedRoutine()
    {
        isBleeding = true;
        PlayerHealth playerObj = Object.FindAnyObjectByType<PlayerHealth>();

        int bleedTicks = 5;
        int bleedDamage = 2;

        for (int i = 0; i < bleedTicks; i++)
        {
            yield return new WaitForSeconds(1f);

            if (currentHealth <= 0) break;

            currentHealth -= bleedDamage;

            if (playerObj != null)
            {
                playerObj.RestoreHealth(bleedDamage);
            }

            if (currentHealth <= 0)
            {
                Die();
                break;
            }
        }
        isBleeding = false;
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

        if (possibleDrops != null)
        {
            foreach (var drop in possibleDrops)
            {
                if (drop.prefab != null && Random.value <= drop.dropChance)
                {
                    Instantiate(drop.prefab, transform.position, Quaternion.identity);
                }
            }
        }

        if (TryGetComponent<EnemyDissolve>(out var dissolveEffect))
        {
            dissolveEffect.TriggerDeath();
        }
        else
        {
            ClearEditorSelectionIfSelected();
            Destroy(gameObject);
        }
    }

    private void ClearEditorSelectionIfSelected()
    {
#if UNITY_EDITOR
        if (UnityEditor.Selection.activeGameObject == gameObject)
        {
            UnityEditor.Selection.activeGameObject = null;
        }
#endif
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