using UnityEngine;
using System.Collections;

public class QueenBossController : MonoBehaviour
{
    [Header("Salud de la Reina")]
    public float maxHealth = 4000f;
    private float currentHealth;

    [Header("Feedback de Daño")]
    public Renderer queenRenderer;
    public Color damageFlashColor = Color.red;
    private Color originalColor;
    private Coroutine flashCoroutine;

    [Header("Sistema de Fases")]
    private int currentPhase = 1;
    private bool isInvulnerable = false;
    private float phase2Threshold;
    private float phase3Threshold;
    private float phase4Threshold;

    [Header("Sistema de Rieles (Bordes)")]
    public float arenaLimit = 33f;
    public float slideSpeed = 12f;

    [Header("Referencias y Distancias")]
    public Transform player;
    public float repelRange = 8f;
    public float closeRange = 18f;
    public float farRange = 45f;
    public float attackCooldown = 2.5f;
    private float currentCooldown = 0f;

    [Header("Rechazo (Empuje Solo Físico)")]
    public float repelForce = 60f;

    [Header("Embestida (Torre)")]
    public float dashSpeed = 50f;
    public float forcedDashTimeout = 8f;
    private float timeSinceLastDash = 0f;

    [Header("Captura (Aplastamiento)")]
    public float jumpHeight = 8f;
    public float hoverTime = 0.8f;
    public float smashDamageRadius = 5f;
    public float smashDamage = 30f;

    [Header("Misiles (Alfil)")]
    public GameObject missileWarningPrefab;
    public int totalMissilesPerAttack = 8;
    public int targetedMissilesCount = 3;
    public float targetingPrecisionRadius = 4f;
    public float randomMissileSpawnRadius = 15f;
    public float timeBetweenWarningAndDamage = 2.5f;

    private bool isAttacking = false;

    void Start()
    {
        currentHealth = maxHealth;
        phase2Threshold = maxHealth * 0.75f;
        phase3Threshold = maxHealth * 0.50f;
        phase4Threshold = maxHealth * 0.25f;

        if (queenRenderer != null) originalColor = queenRenderer.material.color;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        SnapToNearestWall();
    }

    void Update()
    {
        if (isInvulnerable || player == null || isAttacking) return;

        timeSinceLastDash += Time.deltaTime;

        if (currentCooldown > 0)
        {
            currentCooldown -= Time.deltaTime;
            PatrolAlongWall();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= repelRange)
        {
            RepelPlayer();
        }
        else if (distanceToPlayer >= farRange || timeSinceLastDash >= forcedDashTimeout)
        {
            StartCoroutine(DashAttack());
        }
        else if (distanceToPlayer <= closeRange)
        {
            StartCoroutine(CaptureSmash());
        }
        else
        {
            StartCoroutine(BishopMissiles());
        }
    }

    private void SnapToNearestWall()
    {
        transform.position = GetNearestWallPosition(transform.position);
    }

    private Vector3 GetNearestWallPosition(Vector3 currentPos)
    {
        float xPos = currentPos.x;
        float zPos = currentPos.z;

        if (Mathf.Abs(xPos) > Mathf.Abs(zPos))
        {
            float sign = xPos == 0 ? 1 : Mathf.Sign(xPos);
            return new Vector3(sign * arenaLimit, currentPos.y, zPos);
        }
        else
        {
            float sign = zPos == 0 ? 1 : Mathf.Sign(zPos);
            return new Vector3(xPos, currentPos.y, sign * arenaLimit);
        }
    }

    private void PatrolAlongWall()
    {
        bool onXWall = Mathf.Abs(transform.position.x) >= arenaLimit - 0.1f;
        bool onZWall = Mathf.Abs(transform.position.z) >= arenaLimit - 0.1f;

        if (onXWall && onZWall)
        {
            if (Mathf.Abs(player.position.x - transform.position.x) > Mathf.Abs(player.position.z - transform.position.z))
                onXWall = false;
            else
                onZWall = false;
        }

        if (onXWall)
        {
            float targetZ = Mathf.MoveTowards(transform.position.z, player.position.z, slideSpeed * Time.deltaTime);
            transform.position = new Vector3(Mathf.Sign(transform.position.x) * arenaLimit, transform.position.y, targetZ);
        }
        else
        {
            float targetX = Mathf.MoveTowards(transform.position.x, player.position.x, slideSpeed * Time.deltaTime);
            transform.position = new Vector3(targetX, transform.position.y, Mathf.Sign(transform.position.z) * arenaLimit);
        }
    }

    private void RepelPlayer()
    {
        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            Vector3 pushDirection = (player.position - transform.position).normalized;
            pushDirection.y = 0;
            pm.ApplyKnockback(pushDirection, repelForce);
        }

        currentCooldown = 1f;
    }

    public void TakeDamage(float amount, MagicColor hitColor)
    {
        if (isInvulnerable) return;

        currentHealth -= amount;

        if (queenRenderer != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(DamageFlash());
        }

        CheckPhaseTransition();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator DamageFlash()
    {
        queenRenderer.material.color = damageFlashColor;
        yield return new WaitForSeconds(0.1f);
        queenRenderer.material.color = originalColor;
    }

    private void CheckPhaseTransition()
    {
        if (currentPhase == 1 && currentHealth <= phase2Threshold) StartPhaseTransition(2);
        else if (currentPhase == 2 && currentHealth <= phase3Threshold) StartPhaseTransition(3);
        else if (currentPhase == 3 && currentHealth <= phase4Threshold) StartPhaseTransition(4);
    }

    private void StartPhaseTransition(int nextPhase)
    {
        currentPhase = nextPhase;
        isInvulnerable = true;
    }

    public void EndPhaseTransition()
    {
        isInvulnerable = false;
        currentCooldown = attackCooldown;
    }

    private IEnumerator CaptureSmash()
    {
        isAttacking = true;
        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(player.position.x, startPos.y, player.position.z);

        Vector3 highPos = startPos + Vector3.up * jumpHeight;
        Vector3 highTargetPos = targetPos + Vector3.up * jumpHeight;

        float upTimer = 0f;
        while (upTimer < 0.3f)
        {
            transform.position = Vector3.Lerp(startPos, highPos, upTimer / 0.3f);
            upTimer += Time.deltaTime;
            yield return null;
        }

        float moveTimer = 0f;
        while (moveTimer < hoverTime)
        {
            transform.position = Vector3.Lerp(highPos, highTargetPos, moveTimer / hoverTime);
            moveTimer += Time.deltaTime;
            yield return null;
        }

        float fallTimer = 0f;
        while (fallTimer < 0.15f)
        {
            transform.position = Vector3.Lerp(highTargetPos, targetPos, fallTimer / 0.15f);
            fallTimer += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;

        Collider[] hits = Physics.OverlapSphere(transform.position, smashDamageRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                hit.GetComponent<PlayerHealth>()?.TakeDamage(smashDamage, transform);
            }
        }

        yield return new WaitForSeconds(0.5f);

        Vector3 railPos = GetNearestWallPosition(targetPos);
        float returnTimer = 0f;
        while (returnTimer < 0.4f)
        {
            transform.position = Vector3.Lerp(targetPos, railPos, returnTimer / 0.4f);
            returnTimer += Time.deltaTime;
            yield return null;
        }
        transform.position = railPos;

        currentCooldown = attackCooldown;
        isAttacking = false;
    }

    private IEnumerator DashAttack()
    {
        isAttacking = true;
        timeSinceLastDash = 0f;

        Vector3 directionToPlayer = player.position - transform.position;
        Vector3 dashDirection = Vector3.zero;

        if (Mathf.Abs(directionToPlayer.x) > Mathf.Abs(directionToPlayer.z))
        {
            dashDirection = new Vector3(Mathf.Sign(directionToPlayer.x), 0, 0);
        }
        else
        {
            dashDirection = new Vector3(0, 0, Mathf.Sign(directionToPlayer.z));
        }

        Vector3 targetPos = transform.position;
        if (dashDirection.x != 0) targetPos.x = dashDirection.x * arenaLimit;
        if (dashDirection.z != 0) targetPos.z = dashDirection.z * arenaLimit;

        while (Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, dashSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPos;
        currentCooldown = attackCooldown;
        isAttacking = false;
    }

    private IEnumerator BishopMissiles()
    {
        isAttacking = true;

        int actualTargeted = Mathf.Min(targetedMissilesCount, totalMissilesPerAttack);
        int actualRandom = totalMissilesPerAttack - actualTargeted;

        for (int i = 0; i < actualTargeted; i++)
        {
            SpawnMissileWarningAround(player.position, targetingPrecisionRadius);
        }

        for (int i = 0; i < actualRandom; i++)
        {
            SpawnMissileWarningAround(new Vector3(0, transform.position.y, 0), randomMissileSpawnRadius);
        }

        yield return new WaitForSeconds(timeBetweenWarningAndDamage);

        currentCooldown = attackCooldown;
        isAttacking = false;
    }

    private void SpawnMissileWarningAround(Vector3 center, float radius)
    {
        Vector3 spawnPos = new Vector3(
            center.x + Random.Range(-radius, radius),
            0.5f,
            center.z + Random.Range(-radius, radius)
        );

        if (missileWarningPrefab != null)
        {
            Instantiate(missileWarningPrefab, spawnPos, Quaternion.identity);
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}