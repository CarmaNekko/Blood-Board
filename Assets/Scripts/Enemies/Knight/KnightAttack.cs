using UnityEngine;
using System.Collections;

public class KnightAttack : MonoBehaviour
{
    [Header("Daño")]
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

    [Header("Colisiones de Salto")]
    public LayerMask wallMask;

    private GameObject currentDangerZone;

    void Start()
    {
        playerTarget = GameObject.FindGameObjectWithTag("Player")?.transform;
        healthScript = GetComponent<EnemyHealth>();
        myCollider = GetComponent<Collider>();

        if (alertIcon != null) alertIcon.SetActive(false);

        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        yield return new WaitForSeconds(Random.Range(0f, 2.5f));

        while (true)
        {
            if (healthScript != null && healthScript.GetCurrentHealth() <= 0) yield break;

            if (playerTarget != null && !isAttacking)
            {
                yield return StartCoroutine(PerformSlamAttack());
            }
            float currentCooldown = healthScript != null && healthScript.isBuffed ? cooldown / 2f : cooldown;
            yield return new WaitForSeconds(currentCooldown + Random.Range(0f, 1f));
        }
    }

    private Vector3 GetValidJumpPosition(Vector3 startPos, Vector3 targetPos)
    {
        Vector3 rayStart = startPos + Vector3.up * 1f;
        Vector3 rayEnd = targetPos + Vector3.up * 1f;

        Vector3 direction = rayEnd - rayStart;
        float distance = direction.magnitude;

        if (Physics.SphereCast(rayStart, 0.5f, direction.normalized, out RaycastHit hit, distance, wallMask))
        {
            Vector3 safePosition = rayStart + direction.normalized * Mathf.Max(0, hit.distance - 0.5f);
            safePosition.y = targetPos.y;
            return safePosition;
        }

        return targetPos;
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
            if (healthScript != null && healthScript.GetCurrentHealth() <= 0) { CleanUpAttack(); yield break; }
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
        float spread = 1.5f;
        targetDropPosition.x += Random.Range(-spread, spread);
        targetDropPosition.z += Random.Range(-spread, spread);

        targetDropPosition = GetValidJumpPosition(startPosition, targetDropPosition);

        Vector3 floorPoint = targetDropPosition;

        if (Physics.Raycast(new Vector3(targetDropPosition.x, peakPosition.y, targetDropPosition.z), Vector3.down, out RaycastHit hit, 50f, groundMask))
        {
            floorPoint = hit.point;
            targetDropPosition.y = hit.point.y + offsetCentroModelo;
        }
        else
        {
            targetDropPosition.y = startPosition.y;
            floorPoint.y = startPosition.y - offsetCentroModelo;
        }

        if (dangerZonePrefab != null)
        {
            currentDangerZone = Instantiate(dangerZonePrefab, floorPoint + new Vector3(0, 0.05f, 0), Quaternion.identity);
            if (healthScript.isBuffed) currentDangerZone.transform.localScale *= 1.5f;
        }

        Vector3 targetAirPosition = new Vector3(targetDropPosition.x, peakPosition.y, targetDropPosition.z);

        t = 0;
        while (t < 1)
        {
            if (healthScript != null && healthScript.GetCurrentHealth() <= 0) { CleanUpAttack(); yield break; }
            t += Time.deltaTime / hangTime;
            transform.position = Vector3.Lerp(peakPosition, targetAirPosition, t);
            yield return null;
        }

        if (alertIcon != null) alertIcon.SetActive(false);
        if (currentDangerZone != null) Destroy(currentDangerZone);

        if (myCollider != null) myCollider.enabled = false;

        t = 0;
        while (t < 1)
        {
            if (healthScript != null && healthScript.GetCurrentHealth() <= 0) { CleanUpAttack(); yield break; }
            t += Time.deltaTime / dropDuration;
            transform.position = Vector3.Lerp(targetAirPosition, targetDropPosition, t);
            transform.localScale = new Vector3(baseScale.x * 0.6f, baseScale.y * 1.5f, baseScale.z * 0.6f);
            yield return null;
        }

        transform.position = targetDropPosition;
        transform.localScale = new Vector3(baseScale.x * 1.6f, baseScale.y * 0.4f, baseScale.z * 1.6f);

        if (healthScript != null && healthScript.GetCurrentHealth() > 0)
        {
            float currentDamage = healthScript.isBuffed ? baseDamage * 2f : baseDamage;
            float currentImpactRadius = healthScript.isBuffed ? baseRadius * 1.5f : baseRadius;
            damageArea.DealSlamDamage(currentDamage, currentImpactRadius, transform);
        }

        t = 0;
        while (t < 1)
        {
            if (healthScript != null && healthScript.GetCurrentHealth() <= 0) { CleanUpAttack(); yield break; }
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

    private void CleanUpAttack()
    {
        if (alertIcon != null) alertIcon.SetActive(false);
        if (currentDangerZone != null) Destroy(currentDangerZone);
        if (myCollider != null) myCollider.enabled = true;
        isAttacking = false;
    }
}