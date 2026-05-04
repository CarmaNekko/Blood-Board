using UnityEngine;
using System.Collections;

public class KnightAttack : MonoBehaviour
{
    [Header("Daño y Área")]
    public float baseDamage = 40f;
    public float baseRadius = 3f;

    [Header("Cooldowns")]
    public float cooldown = 4f;
    public float riseHeight = 6f;
    public float hangTime = 1f;
    public float dropDuration = 0.15f;

    [Header("Ajustes Visuales")]
    public float yOffsetDelSuelo = 1f;
    public GameObject alertIcon;

    [Header("Referencias")]
    public AudioSource audioSource;
    public AudioClip neighSound;
    public KnightDamageArea damageArea;
    public GameObject dangerZonePrefab;

    private Transform playerTarget;
    private EnemyHealth healthScript;
    private bool isAttacking = false;

    void Start()
    {
        playerTarget = GameObject.FindGameObjectWithTag("Player")?.transform;
        healthScript = GetComponent<EnemyHealth>();

        if (alertIcon != null) alertIcon.SetActive(false);

        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        while (true)
        {
            float currentCooldown = healthScript.isBuffed ? cooldown / 2f : cooldown;
            yield return new WaitForSeconds(currentCooldown);

            if (playerTarget != null && !isAttacking)
            {
                yield return StartCoroutine(PerformSlamAttack());
            }
        }
    }

    IEnumerator PerformSlamAttack()
    {
        isAttacking = true;
        if (alertIcon != null) alertIcon.SetActive(true);

        Vector3 startPosition = transform.position;
        Vector3 peakPosition = startPosition + (Vector3.up * riseHeight);

        Vector3 baseScale = transform.localScale;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / 0.5f;
            transform.position = Vector3.Lerp(startPosition, peakPosition, t);

            float stretchY = Mathf.Lerp(1.2f, 1f, t);
            float squashXZ = Mathf.Lerp(0.9f, 1f, t);
            transform.localScale = new Vector3(baseScale.x * squashXZ, baseScale.y * stretchY, baseScale.z * squashXZ);

            yield return null;
        }

        transform.localScale = baseScale;

        if (audioSource != null && neighSound != null)
        {
            audioSource.pitch = Random.Range(0.85f, 1.15f);
            audioSource.PlayOneShot(neighSound);
        }

        Vector3 targetDropPosition = playerTarget.position;
        targetDropPosition.y = startPosition.y;

        GameObject dangerZone = null;
        if (dangerZonePrefab != null)
        {
            Vector3 zonePos = playerTarget.position;
            zonePos.y -= yOffsetDelSuelo;
            dangerZone = Instantiate(dangerZonePrefab, zonePos, Quaternion.identity);
            if (healthScript.isBuffed) dangerZone.transform.localScale *= 1.5f;
        }

        Vector3 targetAirPosition = new Vector3(targetDropPosition.x, peakPosition.y, targetDropPosition.z);
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / hangTime;
            transform.position = Vector3.Lerp(peakPosition, targetAirPosition, t);
            yield return null;
        }

        if (alertIcon != null) alertIcon.SetActive(false);
        if (dangerZone != null) Destroy(dangerZone);
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / dropDuration;
            transform.position = Vector3.Lerp(targetAirPosition, targetDropPosition, t);
            transform.localScale = new Vector3(baseScale.x * 0.6f, baseScale.y * 1.5f, baseScale.z * 0.6f);

            yield return null;
        }

        transform.localScale = new Vector3(baseScale.x * 1.6f, baseScale.y * 0.4f, baseScale.z * 1.6f);

        float currentDamage = healthScript.isBuffed ? baseDamage * 2f : baseDamage;
        float currentImpactRadius = healthScript.isBuffed ? baseRadius * 1.5f : baseRadius;
        damageArea.DealSlamDamage(currentDamage, currentImpactRadius);

        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / 0.2f;
            transform.localScale = Vector3.Lerp(
                new Vector3(baseScale.x * 1.6f, baseScale.y * 0.4f, baseScale.z * 1.6f),
                baseScale,
                t
            );
            yield return null;
        }

        transform.localScale = baseScale;
        isAttacking = false;
    }
}