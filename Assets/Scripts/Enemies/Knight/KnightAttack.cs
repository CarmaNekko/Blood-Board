using UnityEngine;
using System.Collections;

public class KnightAttack : MonoBehaviour
{
    [Header("Target")]
    public float detectionRange = 25f;
    private bool isAwake = false;

    [Header("Daño y Área")]
    public float baseDamage = 40f;
    public float baseRadius = 3f;

    [Header("Cooldown")]
    public float cooldown = 4f;
    public float riseHeight = 6f;
    public float hangTime = 1f;
    public float dropDuration = 0.15f;

    [Header("Ajustes Visuales")]
    public float offsetCentroModelo = 0f;
    public GameObject alertIcon;
    public LayerMask groundMask = Physics.AllLayers;

    [Header("Referencias")]
    public AudioSource audioSource;
    public AudioClip neighSound;
    public KnightDamageArea damageArea;
    public GameObject dangerZonePrefab;

    private Transform playerTarget;
    private EnemyHealth healthScript;
    private bool isAttacking = false;
    private Collider myCollider;

    void Start()
    {
        playerTarget = GameObject.FindGameObjectWithTag("Player")?.transform;
        healthScript = GetComponent<EnemyHealth>();
        myCollider = GetComponent<Collider>();

        if (alertIcon != null) alertIcon.SetActive(false);
    }

    void Update()
    {
        if (!isAwake && playerTarget != null)
        {
            if (Vector3.Distance(transform.position, playerTarget.position) <= detectionRange)
            {
                isAwake = true;
                StartCoroutine(AttackRoutine());
            }
        }
    }

    IEnumerator AttackRoutine()
    {
        while (true)
        {
            if (playerTarget != null && !isAttacking)
            {
                yield return StartCoroutine(PerformSlamAttack());
            }
            float currentCooldown = healthScript.isBuffed ? cooldown / 2f : cooldown;
            yield return new WaitForSeconds(currentCooldown);
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
        Vector3 floorPoint = targetDropPosition;

        if (Physics.Raycast(new Vector3(playerTarget.position.x, peakPosition.y, playerTarget.position.z), Vector3.down, out RaycastHit hit, 50f, groundMask))
        {
            floorPoint = hit.point;
            targetDropPosition.y = hit.point.y + offsetCentroModelo;
        }
        else
        {
            targetDropPosition.y = startPosition.y;
            floorPoint.y = startPosition.y - offsetCentroModelo;
        }

        GameObject dangerZone = null;
        if (dangerZonePrefab != null)
        {
            dangerZone = Instantiate(dangerZonePrefab, floorPoint + new Vector3(0, 0.05f, 0), Quaternion.identity);
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

        if (myCollider != null) myCollider.enabled = false;

        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / dropDuration;
            transform.position = Vector3.Lerp(targetAirPosition, targetDropPosition, t);
            transform.localScale = new Vector3(baseScale.x * 0.6f, baseScale.y * 1.5f, baseScale.z * 0.6f);
            yield return null;
        }

        transform.position = targetDropPosition;
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
        if (myCollider != null) myCollider.enabled = true;
        isAttacking = false;
    }
}