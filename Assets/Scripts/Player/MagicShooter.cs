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
    [SerializeField] private PlayerCameraEffects cameraEffects;

    [Header("UI Colors")]
    [SerializeField] private Image whiteManaFill;
    [SerializeField] private Image blackManaFill;
    [SerializeField] private Color normalWhiteColor = Color.white;
    [SerializeField] private Color normalBlackColor = new Color(0.3f, 0f, 0.5f);
    [SerializeField] private Color warningColor = new Color(1f, 0.5f, 0f);
    [SerializeField] private Color overheatedColor = Color.red;

    [Header("White Magic System")]
    [SerializeField] private float maxWhiteMana = 100f;
    [SerializeField] private float whiteManaCost = 20f;
    [SerializeField] private float whiteManaRegen = 20f;
    [SerializeField] private Slider whiteManaBarUI;
    private float currentWhiteMana;
    private bool isWhiteOverheated = false;

    [Header("Black Magic System")]
    [SerializeField] private float maxBlackMana = 100f;
    [SerializeField] private float blackManaCost = 20f;
    [SerializeField] private float blackManaRegen = 20f;
    [SerializeField] private Slider blackManaBarUI;
    private float currentBlackMana;
    private bool isBlackOverheated = false;

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

        if (cameraEffects == null) cameraEffects = GetComponentInChildren<PlayerCameraEffects>();
    }

    void Update()
    {
        if (PauseScreen.IsPaused || TutorialMessage.IsTutorialActive) return;

        RegenerateMana();

        if (Input.GetButtonDown("Fire1") && !isWhiteOverheated && currentWhiteMana >= whiteManaCost)
        {
            Shoot(whiteMagicPrefab);
            currentWhiteMana -= whiteManaCost;

            if (currentWhiteMana < whiteManaCost) isWhiteOverheated = true;

            UpdateUI();
        }
        else if (Input.GetButtonDown("Fire2") && !isBlackOverheated && currentBlackMana >= blackManaCost)
        {
            Shoot(blackMagicPrefab);
            currentBlackMana -= blackManaCost;

            if (currentBlackMana < blackManaCost) isBlackOverheated = true;

            UpdateUI();
        }
    }

    private void RegenerateMana()
    {
        float healthMultiplier = GetHealthMultiplier();

        if (currentWhiteMana < maxWhiteMana)
        {
            currentWhiteMana += (whiteManaRegen * healthMultiplier) * Time.deltaTime;
            if (currentWhiteMana >= maxWhiteMana)
            {
                currentWhiteMana = maxWhiteMana;
                isWhiteOverheated = false;
            }
        }

        if (currentBlackMana < maxBlackMana)
        {
            currentBlackMana += (blackManaRegen * healthMultiplier) * Time.deltaTime;
            if (currentBlackMana >= maxBlackMana)
            {
                currentBlackMana = maxBlackMana;
                isBlackOverheated = false;
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

        if (whiteManaFill != null)
        {
            if (isWhiteOverheated)
            {
                whiteManaFill.color = overheatedColor;
            }
            else if (currentWhiteMana <= (whiteManaCost * 1.5f))
            {
                whiteManaFill.color = warningColor;
            }
            else
            {
                whiteManaFill.color = normalWhiteColor;
            }
        }

        if (blackManaFill != null)
        {
            if (isBlackOverheated)
            {
                blackManaFill.color = overheatedColor;
            }
            else if (currentBlackMana <= (blackManaCost * 1.5f))
            {
                blackManaFill.color = warningColor;
            }
            else
            {
                blackManaFill.color = normalBlackColor;
            }
        }
    }

    private void Shoot(GameObject magicPrefab)
    {
        if (magicPrefab != null)
        {
            GameObject projectile = Instantiate(magicPrefab, firePoint.position, firePoint.rotation);
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = firePoint.forward * shootForce;
            Destroy(projectile, 2f);

            if (cameraEffects != null) cameraEffects.ApplyShootRecoil();
        }
    }
}