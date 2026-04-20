using UnityEngine;
using UnityEngine.UI;

public class MagicShooter : MonoBehaviour
{
    [Header("Projectiles")]
    [SerializeField] private GameObject whiteMagicPrefab;
    [SerializeField] private GameObject blackMagicPrefab;

    [Header("Shooting Setup")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootForce = 30f;

    [Header("White Magic System")]
    [SerializeField] private float maxWhiteMana = 100f;
    [SerializeField] private float whiteManaCost = 20f;
    [SerializeField] private float whiteManaRegen = 20f;
    [SerializeField] private Slider whiteManaBarUI;
    private float currentWhiteMana;
    private bool isWhiteExhausted = false;

    [Header("Black Magic System")]
    [SerializeField] private float maxBlackMana = 100f;
    [SerializeField] private float blackManaCost = 20f;
    [SerializeField] private float blackManaRegen = 20f;
    [SerializeField] private Slider blackManaBarUI;
    private float currentBlackMana;
    private bool isBlackExhausted = false;

    [Header("Mecanicas de Adrenalina")]
    [SerializeField] private float emptyRegenMultiplier = 2.5f;
    [Range(0f, 1f)][SerializeField] private float exhaustThreshold = 0.05f;

    private PlayerHealth playerHealth;

    void Start()
    {
        currentWhiteMana = maxWhiteMana;
        currentBlackMana = maxBlackMana;

        if (whiteManaBarUI != null)
        {
            whiteManaBarUI.maxValue = maxWhiteMana;
            whiteManaBarUI.value = currentWhiteMana;
        }
        if (blackManaBarUI != null)
        {
            blackManaBarUI.maxValue = maxBlackMana;
            blackManaBarUI.value = currentBlackMana;
        }

        playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        RegenerateMana();

        if (Input.GetButtonDown("Fire1") && currentWhiteMana >= whiteManaCost)
        {
            Shoot(whiteMagicPrefab);
            currentWhiteMana -= whiteManaCost;

            if (currentWhiteMana <= (maxWhiteMana * exhaustThreshold))
            {
                isWhiteExhausted = true;
            }

            UpdateUI();
        }
        else if (Input.GetButtonDown("Fire2") && currentBlackMana >= blackManaCost)
        {
            Shoot(blackMagicPrefab);
            currentBlackMana -= blackManaCost;

            if (currentBlackMana <= (maxBlackMana * exhaustThreshold))
            {
                isBlackExhausted = true;
            }

            UpdateUI();
        }
    }

    private void RegenerateMana()
    {
        float healthMultiplier = GetHealthMultiplier();

        if (currentWhiteMana < maxWhiteMana)
        {
            float emptyTurbo = isWhiteExhausted ? emptyRegenMultiplier : 1f;
            currentWhiteMana += (whiteManaRegen * healthMultiplier * emptyTurbo) * Time.deltaTime;

            if (currentWhiteMana >= maxWhiteMana)
            {
                currentWhiteMana = maxWhiteMana;
                isWhiteExhausted = false;
            }
        }

        if (currentBlackMana < maxBlackMana)
        {
            float emptyTurbo = isBlackExhausted ? emptyRegenMultiplier : 1f;
            currentBlackMana += (blackManaRegen * healthMultiplier * emptyTurbo) * Time.deltaTime;

            if (currentBlackMana >= maxBlackMana)
            {
                currentBlackMana = maxBlackMana;
                isBlackExhausted = false;
            }
        }

        UpdateUI();
    }

    private float GetHealthMultiplier()
    {
        if (playerHealth == null) return 1.0f;

        float healthPercent = playerHealth.GetHealthPercentage();

        if (healthPercent <= 0.20f) return 2.0f;
        if (healthPercent <= 0.50f) return 1.5f;
        return 1.0f;
    }

    private void UpdateUI()
    {
        if (whiteManaBarUI != null) whiteManaBarUI.value = currentWhiteMana;
        if (blackManaBarUI != null) blackManaBarUI.value = currentBlackMana;
    }

    private void Shoot(GameObject magicPrefab)
    {
        if (magicPrefab != null)
        {
            GameObject projectile = Instantiate(magicPrefab, firePoint.position, firePoint.rotation);
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = firePoint.forward * shootForce;
            }

            Destroy(projectile, 2f);
        }
    }
}