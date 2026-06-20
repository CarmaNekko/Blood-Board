using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Serialization;

public class MagicShooter : MonoBehaviour
{
    [Header("Projectiles")]
    [SerializeField] private GameObject whiteMagicPrefab;
    [SerializeField] private GameObject blackMagicPrefab;
    [SerializeField] private GameObject harmonicMagicPrefab;

    [Header("Slash Attack Setup")]
    [SerializeField] private GameObject whiteSlashPrefab;
    [SerializeField] private GameObject blackSlashPrefab;
    [SerializeField] private GameObject harmonicSlashPrefab;
    [FormerlySerializedAs("maxChargePercent")]
    [SerializeField, Range(0.05f, 1f)] private float slashManaCostPercent = 0.20f;
    [SerializeField] private float chargeSpeed = 40f;
    [SerializeField] private float timeToStartCharge = 0.2f;
    [SerializeField] private bool hasSlashAttack = false;

    [Header("Bullet Rain Attack Setup")]
    [SerializeField] private GameObject whiteBulletRainPrefab;
    [SerializeField] private GameObject blackBulletRainPrefab;
    [SerializeField] private GameObject harmonicBulletRainPrefab;
    [SerializeField] private float bulletRainSpawnHeight = 15f;
    [SerializeField] private float bulletRainDistance = 15f;

    [Header("Shooting Setup")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootForce = 30f;
    [SerializeField] private PlayerCameraEffects cameraEffects;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Arma Pesada (Inercia)")]
    [SerializeField] private float movingShootDelay = 0.25f;
    private bool isPreparingToShoot = false;

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

    private bool isHarmonicActive = false;
    private float harmonicTimer = 0f;

    private bool isVampirismActive = false;

    private bool isBulletRainActive = false;
    private float bulletRainTimer = 0f;

    private bool isChargingSlash = false;
    private float currentChargeManaDrained = 0f;
    private MagicColor chargingMagicColor;
    private float holdTimer = 0f;

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
        if (playerMovement == null) playerMovement = GetComponentInParent<PlayerMovement>();
    }

    void Update()
    {
        if (PauseScreen.IsPaused || TutorialMessage.IsTutorialActive || isPreparingToShoot) return;

        HandleHarmonicTimer();
        HandleBulletRainTimer();

        if (!isChargingSlash)
        {
            RegenerateMana();
        }

        HandleInput();
    }

    private void HandleInput()
    {
        bool fire1Held = Input.GetButton("Fire1");
        bool fire2Held = Input.GetButton("Fire2");
        bool fire1Up = Input.GetButtonUp("Fire1");
        bool fire2Up = Input.GetButtonUp("Fire2");

        if (isBulletRainActive)
        {
            if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Fire2"))
            {
                if (isHarmonicActive)
                {
                    StartCoroutine(HandleBulletRainShoot(MagicColor.Harmonic));
                }
                else if (Input.GetButtonDown("Fire1") && !isWhiteOverheated && currentWhiteMana >= whiteManaCost)
                {
                    StartCoroutine(HandleBulletRainShoot(MagicColor.White));
                }
                else if (Input.GetButtonDown("Fire2") && !isBlackOverheated && currentBlackMana >= blackManaCost)
                {
                    StartCoroutine(HandleBulletRainShoot(MagicColor.Black));
                }
            }
            return;
        }

        if (!hasSlashAttack)
        {
            HandleBaseMagicInput(fire1Up, fire2Up);
            return;
        }

        if (!isChargingSlash)
        {
            if (fire1Held)
            {
                holdTimer += Time.deltaTime;
                if (holdTimer >= timeToStartCharge)
                {
                    StartCharging(isHarmonicActive ? MagicColor.Harmonic : MagicColor.White);
                }
            }
            else if (fire2Held)
            {
                holdTimer += Time.deltaTime;
                if (holdTimer >= timeToStartCharge)
                {
                    StartCharging(isHarmonicActive ? MagicColor.Harmonic : MagicColor.Black);
                }
            }
            else
            {
                holdTimer = 0f;
            }

            if (fire1Up && holdTimer < timeToStartCharge)
            {
                if (isHarmonicActive) StartCoroutine(HandleHarmonicShoot());
                else if (!isWhiteOverheated && currentWhiteMana >= whiteManaCost) StartCoroutine(HandleShootRequest(whiteMagicPrefab, true));
                holdTimer = 0f;
            }
            else if (fire2Up && holdTimer < timeToStartCharge)
            {
                if (isHarmonicActive) StartCoroutine(HandleHarmonicShoot());
                else if (!isBlackOverheated && currentBlackMana >= blackManaCost) StartCoroutine(HandleShootRequest(blackMagicPrefab, false));
                holdTimer = 0f;
            }
        }
        else
        {
            ProcessCharging();

            if ((chargingMagicColor == MagicColor.White && fire1Up) ||
                (chargingMagicColor == MagicColor.Black && fire2Up) ||
                (chargingMagicColor == MagicColor.Harmonic && (fire1Up || fire2Up)))
            {
                FireSlash();
                ResetCharge();
            }
        }
    }

    private void HandleBaseMagicInput(bool fire1Up, bool fire2Up)
    {
        if (fire1Up)
        {
            if (isHarmonicActive) StartCoroutine(HandleHarmonicShoot());
            else if (!isWhiteOverheated && currentWhiteMana >= whiteManaCost) StartCoroutine(HandleShootRequest(whiteMagicPrefab, true));
        }
        else if (fire2Up)
        {
            if (isHarmonicActive) StartCoroutine(HandleHarmonicShoot());
            else if (!isBlackOverheated && currentBlackMana >= blackManaCost) StartCoroutine(HandleShootRequest(blackMagicPrefab, false));
        }

        holdTimer = 0f;
    }

    private void StartCharging(MagicColor color)
    {
        if ((color == MagicColor.White && (isWhiteOverheated || currentWhiteMana < GetSlashManaCost(MagicColor.White))) ||
            (color == MagicColor.Black && (isBlackOverheated || currentBlackMana < GetSlashManaCost(MagicColor.Black))))
        {
            return;
        }

        isChargingSlash = true;
        chargingMagicColor = color;
        currentChargeManaDrained = 0f;
    }

    private void ProcessCharging()
    {
        if (chargingMagicColor == MagicColor.White)
        {
            float maxDrain = maxWhiteMana * slashManaCostPercent;
            float allowedDrain = maxDrain - currentChargeManaDrained;
            if (allowedDrain > 0 && currentWhiteMana > 0)
            {
                float drainThisFrame = Mathf.Min(chargeSpeed * Time.deltaTime, allowedDrain, currentWhiteMana);
                currentWhiteMana -= drainThisFrame;
                currentChargeManaDrained += drainThisFrame;
                if (currentWhiteMana < whiteManaCost) isWhiteOverheated = true;
            }
        }
        else if (chargingMagicColor == MagicColor.Black)
        {
            float maxDrain = maxBlackMana * slashManaCostPercent;
            float allowedDrain = maxDrain - currentChargeManaDrained;
            if (allowedDrain > 0 && currentBlackMana > 0)
            {
                float drainThisFrame = Mathf.Min(chargeSpeed * Time.deltaTime, allowedDrain, currentBlackMana);
                currentBlackMana -= drainThisFrame;
                currentChargeManaDrained += drainThisFrame;
                if (currentBlackMana < blackManaCost) isBlackOverheated = true;
            }
        }
        else if (chargingMagicColor == MagicColor.Harmonic)
        {
            float maxDrain = 100f * slashManaCostPercent;
            float allowedDrain = maxDrain - currentChargeManaDrained;
            if (allowedDrain > 0)
            {
                currentChargeManaDrained += chargeSpeed * Time.deltaTime;
            }
        }

        UpdateUI();
    }

    private void ResetCharge()
    {
        isChargingSlash = false;
        holdTimer = 0f;
        currentChargeManaDrained = 0f;
    }

    private void FireSlash()
    {
        GameObject slashPrefab = null;
        if (chargingMagicColor == MagicColor.White) slashPrefab = whiteSlashPrefab;
        else if (chargingMagicColor == MagicColor.Black) slashPrefab = blackSlashPrefab;
        else if (chargingMagicColor == MagicColor.Harmonic) slashPrefab = harmonicSlashPrefab;

        if (slashPrefab == null || !TryPayRemainingSlashManaCost())
        {
            return;
        }

        GameObject projectile = Instantiate(slashPrefab, firePoint.position, firePoint.rotation * slashPrefab.transform.rotation);

        if (isVampirismActive)
        {
            SlashProjectile sp = projectile.GetComponent<SlashProjectile>();
            if (sp != null) sp.appliesVampirism = true;
        }

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = firePoint.forward * shootForce;
        Destroy(projectile, 2f);

        if (cameraEffects != null) cameraEffects.ApplyShootRecoil();
    }

    private float GetSlashManaCost(MagicColor color)
    {
        if (color == MagicColor.White)
        {
            return maxWhiteMana * slashManaCostPercent;
        }

        if (color == MagicColor.Black)
        {
            return maxBlackMana * slashManaCostPercent;
        }

        return 100f * slashManaCostPercent;
    }

    private bool TryPayRemainingSlashManaCost()
    {
        float remainingCost = Mathf.Max(0f, GetSlashManaCost(chargingMagicColor) - currentChargeManaDrained);

        if (chargingMagicColor == MagicColor.White)
        {
            if (currentWhiteMana < remainingCost)
            {
                return false;
            }

            currentWhiteMana -= remainingCost;
            if (currentWhiteMana < whiteManaCost) isWhiteOverheated = true;
        }
        else if (chargingMagicColor == MagicColor.Black)
        {
            if (currentBlackMana < remainingCost)
            {
                return false;
            }

            currentBlackMana -= remainingCost;
            if (currentBlackMana < blackManaCost) isBlackOverheated = true;
        }

        currentChargeManaDrained += remainingCost;
        UpdateUI();
        return true;
    }

    private IEnumerator HandleBulletRainShoot(MagicColor color)
    {
        isPreparingToShoot = true;
        bool isMoving = (playerMovement != null && playerMovement.CurrentVelocity.magnitude > 0.5f);

        if (isMoving)
        {
            yield return new WaitForSeconds(movingShootDelay);
        }

        if (color == MagicColor.White)
        {
            currentWhiteMana -= whiteManaCost;
            if (currentWhiteMana < whiteManaCost) isWhiteOverheated = true;
        }
        else if (color == MagicColor.Black)
        {
            currentBlackMana -= blackManaCost;
            if (currentBlackMana < blackManaCost) isBlackOverheated = true;
        }

        UpdateUI();

        GameObject prefab = null;
        if (color == MagicColor.White) prefab = whiteBulletRainPrefab;
        else if (color == MagicColor.Black) prefab = blackBulletRainPrefab;
        else if (color == MagicColor.Harmonic) prefab = harmonicBulletRainPrefab;

        if (prefab != null)
        {
            Vector3 targetPoint;
            Camera mainCam = Camera.main;

            if (mainCam != null)
            {
                Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                if (Physics.Raycast(ray, out RaycastHit hit, 100f))
                {
                    targetPoint = hit.point;
                }
                else
                {
                    targetPoint = ray.GetPoint(bulletRainDistance);
                }
            }
            else
            {
                if (Physics.Raycast(firePoint.position, firePoint.forward, out RaycastHit hit, 100f))
                {
                    targetPoint = hit.point;
                }
                else
                {
                    targetPoint = firePoint.position + firePoint.forward * bulletRainDistance;
                }
            }

            Vector3 spawnPos = targetPoint + Vector3.up * bulletRainSpawnHeight;

            GameObject projectile = Instantiate(prefab, spawnPos, Quaternion.identity);

            if (isVampirismActive)
            {
                BulletRainProjectile brp = projectile.GetComponent<BulletRainProjectile>();
                if (brp != null) brp.appliesVampirism = true;
            }

            Destroy(projectile, 4f);

            if (cameraEffects != null) cameraEffects.ApplyShootRecoil();
        }

        isPreparingToShoot = false;
    }

    private void HandleHarmonicTimer()
    {
        if (isHarmonicActive)
        {
            harmonicTimer -= Time.deltaTime;
            if (harmonicTimer <= 0f)
            {
                isHarmonicActive = false;
            }
        }
    }

    private void HandleBulletRainTimer()
    {
        if (isBulletRainActive)
        {
            bulletRainTimer -= Time.deltaTime;
            if (bulletRainTimer <= 0f)
            {
                isBulletRainActive = false;
            }
        }
    }

    private IEnumerator HandleHarmonicShoot()
    {
        isPreparingToShoot = true;
        bool isMoving = (playerMovement != null && playerMovement.CurrentVelocity.magnitude > 0.5f);

        if (isMoving)
        {
            yield return new WaitForSeconds(movingShootDelay);
        }

        Shoot(harmonicMagicPrefab);
        isPreparingToShoot = false;
    }

    private IEnumerator HandleShootRequest(GameObject magicPrefab, bool isWhiteMagic)
    {
        isPreparingToShoot = true;

        bool isMoving = (playerMovement != null && playerMovement.CurrentVelocity.magnitude > 0.5f);

        if (isMoving)
        {
            yield return new WaitForSeconds(movingShootDelay);
        }
        if (isWhiteMagic && currentWhiteMana >= whiteManaCost)
        {
            Shoot(magicPrefab);
            currentWhiteMana -= whiteManaCost;
            if (currentWhiteMana < whiteManaCost) isWhiteOverheated = true;
        }
        else if (!isWhiteMagic && currentBlackMana >= blackManaCost)
        {
            Shoot(magicPrefab);
            currentBlackMana -= blackManaCost;
            if (currentBlackMana < blackManaCost) isBlackOverheated = true;
        }

        UpdateUI();
        isPreparingToShoot = false;
    }

    private void Shoot(GameObject magicPrefab)
    {
        if (magicPrefab != null)
        {
            GameObject projectile = Instantiate(magicPrefab, firePoint.position, firePoint.rotation);

            if (isVampirismActive)
            {
                MagicProjectile mp = projectile.GetComponent<MagicProjectile>();
                if (mp != null) mp.appliesVampirism = true;

                HarmonicProjectile hp = projectile.GetComponent<HarmonicProjectile>();
                if (hp != null) hp.appliesVampirism = true;
            }

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = firePoint.forward * shootForce;
            Destroy(projectile, 2f);

            if (cameraEffects != null) cameraEffects.ApplyShootRecoil();
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

    public void RefillManaToMax()
    {
        currentWhiteMana = maxWhiteMana;
        currentBlackMana = maxBlackMana;
        isWhiteOverheated = false;
        isBlackOverheated = false;
        UpdateUI();
    }

    public void ActivateHarmonicPowerUp(float duration)
    {
        isHarmonicActive = true;
        harmonicTimer = duration;
    }

    public bool UnlockVampirism()
    {
        if (isVampirismActive)
        {
            return false;
        }

        isVampirismActive = true;
        return true;
    }

    public void ActivateBulletRainAttack(float duration)
    {
        isBulletRainActive = true;
        bulletRainTimer = duration;
    }

    public bool HasSlashAttack()
    {
        return hasSlashAttack;
    }

    public bool HasVampirism()
    {
        return isVampirismActive;
    }

    public bool UnlockSlashAttack(float manaCostPercent)
    {
        if (hasSlashAttack)
        {
            return false;
        }

        hasSlashAttack = true;
        slashManaCostPercent = manaCostPercent;
        return true;
    }
}
