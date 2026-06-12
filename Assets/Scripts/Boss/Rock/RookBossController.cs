using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RookBossController : MonoBehaviour
{
    [Header("Animación")]
    [SerializeField] private float riseHeight = 30f;
    [SerializeField] private float animDuration = 3f;
    private Vector3 hiddenPos;
    private Vector3 visiblePos;

    [Header("Defensa y Salud")]
    public GameObject shieldVisual;
    public GameObject bossHitbox;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private GameObject healthBarUI;

    [Header("Escudo Dinámico")]
    [SerializeField] private float maxShield = 100f;
    [SerializeField] private Slider shieldSlider;
    private float currentShield;

    [HideInInspector] public bool isDead = false;
    private ColiseumManager coliseumManager;
    private bool battleStarted = false;
    private EnemyHealth enemyHealth;

    void Start()
    {
        visiblePos = transform.position;
        hiddenPos = visiblePos + Vector3.down * riseHeight;
        transform.position = hiddenPos;

        currentShield = maxShield;
        if (shieldSlider != null)
        {
            shieldSlider.maxValue = maxShield;
            shieldSlider.value = currentShield;
            shieldSlider.gameObject.SetActive(false);
        }

        if (shieldVisual != null) shieldVisual.SetActive(false);
        if (bossHitbox != null) bossHitbox.SetActive(false);

        if (healthBarUI != null) healthBarUI.SetActive(false);

        if (bossHitbox != null)
        {
            enemyHealth = bossHitbox.GetComponent<EnemyHealth>();
        }
    }

    public void Initialize(ColiseumManager manager)
    {
        coliseumManager = manager;
        battleStarted = true;

        if (healthBarUI != null)
        {
            healthBarUI.SetActive(true);
            if (enemyHealth != null)
            {
                healthSlider.maxValue = enemyHealth.maxHealth;
                healthSlider.value = enemyHealth.maxHealth;
            }
        }

        if (shieldSlider != null) shieldSlider.gameObject.SetActive(true);

        StartCoroutine(RiseRoutine());
    }

    void Update()
    {
        if (battleStarted && !isDead)
        {
            if (bossHitbox == null)
            {
                Die();
            }
            else if (enemyHealth != null && healthSlider != null)
            {
                healthSlider.value = enemyHealth.GetCurrentHealth();
            }
        }
    }

    private IEnumerator RiseRoutine()
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / animDuration;
            transform.position = Vector3.Lerp(hiddenPos, visiblePos, t);
            yield return null;
        }

        SetShield(true);

        if (coliseumManager != null) coliseumManager.StartEvents();
    }

    public void SetShield(bool isActive)
    {
        if (shieldVisual != null) shieldVisual.SetActive(isActive);
        if (bossHitbox != null) bossHitbox.SetActive(!isActive);
    }

    public void DamageShield(float amount)
    {
        currentShield -= amount;
        if (currentShield < 0) currentShield = 0;
        if (shieldSlider != null) shieldSlider.value = currentShield;
    }

    public void HealShield(float amount)
    {
        currentShield += amount;
        if (currentShield > maxShield) currentShield = maxShield;
        if (shieldSlider != null) shieldSlider.value = currentShield;
    }

    public bool IsShieldBroken()
    {
        return currentShield <= 0;
    }

    public bool IsShieldFull()
    {
        return currentShield >= maxShield;
    }

    private void Die()
    {
        isDead = true;
        if (healthSlider != null) healthSlider.value = 0;
        if (healthBarUI != null) healthBarUI.SetActive(false);
        if (shieldSlider != null) shieldSlider.gameObject.SetActive(false);
        if (coliseumManager != null) coliseumManager.BossDefeated();
        StartCoroutine(FallRoutine());
    }

    private IEnumerator FallRoutine()
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / animDuration;
            transform.position = Vector3.Lerp(visiblePos, hiddenPos, t);
            yield return null;
        }
        Destroy(gameObject);
    }
}