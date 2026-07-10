using UnityEngine;
using System.Collections;

public class QueenBossController : MonoBehaviour
{
    [Header("Destrucción de Pisos")]
    public GameObject[] floorsToDestroy;
    public float distanceBetweenFloors = 30f;
    private int currentFloorIndex = 0;
    private float baseFloorY;

    [Header("Salud de la Reina")]
    public float maxHealth = 4000f;
    private float currentHealth;

    [Header("Feedback de Daño")]
    public Renderer queenRenderer;
    public Color damageFlashColor = Color.red;
    private Color originalColor;
    private Coroutine flashCoroutine;

    [Header("Sistema de Fases (5 Pisos)")]
    private int currentPhase = 1;
    private bool isInvulnerable = false;
    private float phase2Threshold;
    private float phase3Threshold;
    private float phase4Threshold;
    private float phase5Threshold;

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
    public float repelDamage = 15f;

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

    [Header("Lasers (Peón Campeón)")]
    public GameObject[] spinLasers;

    private bool isAttacking = false;

    void Start()
    {
        baseFloorY = transform.position.y;
        currentHealth = maxHealth;

        phase2Threshold = maxHealth * 0.80f;
        phase3Threshold = maxHealth * 0.60f;
        phase4Threshold = maxHealth * 0.40f;
        phase5Threshold = maxHealth * 0.20f;

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
            if (currentPhase == 2)
            {
                StartCoroutine(UltimateSpinAttack());
            }
            else if (currentPhase >= 3)
            {
                if (Random.value > 0.4f) StartCoroutine(UltimateSpinAttack());
                else StartCoroutine(BishopMissiles());
            }
            else
            {
                StartCoroutine(BishopMissiles());
            }
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

        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph != null) ph.TakeDamage(repelDamage, transform);

        currentCooldown = Mathf.Max(0.2f, 1f - (currentPhase * 0.1f));
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
        else if (currentPhase == 4 && currentHealth <= phase5Threshold) StartPhaseTransition(5);
    }

    private void StartPhaseTransition(int nextPhase)
    {
        currentPhase = nextPhase;
        isInvulnerable = true;
        isAttacking = false;

        StopAllCoroutines();

        float correctY = baseFloorY - ((currentPhase - 2) * distanceBetweenFloors);
        transform.position = new Vector3(transform.position.x, correctY, transform.position.z);
        SnapToNearestWall();

        Collider queenCol = GetComponent<Collider>();
        if (queenCol != null) queenCol.isTrigger = false;

        transform.rotation = Quaternion.identity;
        if (queenRenderer != null) queenRenderer.material.color = originalColor;

        if (spinLasers != null)
        {
            foreach (GameObject laser in spinLasers)
            {
                if (laser != null) laser.SetActive(false);
            }
        }

        KingBossController king = FindFirstObjectByType<KingBossController>();
        if (king != null) king.ActivateDefensePhase(nextPhase);
    }

    public void EndPhaseTransition()
    {
        StartCoroutine(ShatterFloorRoutine());
    }

    private IEnumerator ShatterFloorRoutine()
    {
        isInvulnerable = true;

        float currentFloorY = baseFloorY - ((currentPhase - 2) * distanceBetweenFloors);
        Vector3 startPos = transform.position;
        Vector3 centerPos = new Vector3(0, currentFloorY, 0);
        Vector3 highPos = centerPos + Vector3.up * (jumpHeight * 1.5f);

        float upTimer = 0f;
        while (upTimer < 0.5f)
        {
            transform.position = Vector3.Lerp(startPos, highPos, upTimer / 0.5f);
            upTimer += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        KingBossController king = FindFirstObjectByType<KingBossController>();
        if (king != null) king.DescendToNextFloor();

        if (currentFloorIndex < floorsToDestroy.Length)
        {
            if (floorsToDestroy[currentFloorIndex] != null)
            {
                floorsToDestroy[currentFloorIndex].SetActive(false);
            }
            currentFloorIndex++;
        }

        float targetY = currentFloorY - distanceBetweenFloors;
        float fallTime = 1.5f;
        float currentFallTime = 0f;

        while (currentFallTime < fallTime)
        {
            transform.position = new Vector3(transform.position.x, Mathf.Lerp(highPos.y, targetY, currentFallTime / fallTime), transform.position.z);
            currentFallTime += Time.deltaTime;
            yield return null;
        }

        transform.position = new Vector3(transform.position.x, targetY, transform.position.z);

        SnapToNearestWall();
        isInvulnerable = false;
        currentCooldown = Mathf.Max(0.5f, attackCooldown - (currentPhase * 0.25f));
    }

    private IEnumerator UltimateSpinAttack()
    {
        isAttacking = true;
        Vector3 centerPos = new Vector3(0, transform.position.y, 0);

        while (Vector3.Distance(transform.position, centerPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, centerPos, dashSpeed * Time.deltaTime);
            yield return null;
        }

        if (spinLasers != null)
        {
            foreach (GameObject laser in spinLasers)
            {
                if (laser != null) laser.SetActive(true);
            }
        }

        float spinDuration = 4f + (currentPhase * 0.5f);
        float timer = 0f;
        float spawnTimer = 0f;

        float currentSpinSpeed = currentPhase == 2 ? 30f : (currentPhase == 3 ? 45f : (currentPhase == 4 ? 55f : 60f));
        float spawnInterval = Mathf.Max(0.15f, 0.9f - (currentPhase * 0.15f));

        Vector3 movementTarget = centerPos;
        bool needsNewTarget = true;
        int patternIndex = 0;

        Vector3[] phase4Points = new Vector3[] {
            new Vector3(arenaLimit - 5, centerPos.y, arenaLimit - 5),
            new Vector3(-arenaLimit + 5, centerPos.y, -arenaLimit + 5),
            new Vector3(-arenaLimit + 5, centerPos.y, arenaLimit - 5),
            new Vector3(arenaLimit - 5, centerPos.y, -arenaLimit + 5)
        };

        while (timer < spinDuration)
        {
            transform.Rotate(0, currentSpinSpeed * Time.deltaTime, 0);
            timer += Time.deltaTime;
            spawnTimer += Time.deltaTime;
            if (spawnTimer > spawnInterval && currentPhase > 2)
            {
                SpawnMissileWarningAround(player.position, targetingPrecisionRadius);
                SpawnMissileWarningAround(transform.position, randomMissileSpawnRadius);
                spawnTimer = 0f;
            }

            if (currentPhase == 4)
            {
                if (needsNewTarget)
                {
                    movementTarget = phase4Points[patternIndex % phase4Points.Length];
                    needsNewTarget = false;
                }
                transform.position = Vector3.MoveTowards(transform.position, movementTarget, slideSpeed * 1.5f * Time.deltaTime);
                if (Vector3.Distance(transform.position, movementTarget) < 0.5f)
                {
                    patternIndex++;
                    needsNewTarget = true;
                }
            }
            else if (currentPhase == 5)
            {
                if (needsNewTarget)
                {
                    movementTarget = new Vector3(Random.Range(-arenaLimit + 6, arenaLimit - 6), centerPos.y, Random.Range(-arenaLimit + 6, arenaLimit - 6));
                    needsNewTarget = false;
                }
                transform.position = Vector3.MoveTowards(transform.position, movementTarget, dashSpeed * 0.6f * Time.deltaTime);
                if (Vector3.Distance(transform.position, movementTarget) < 1f)
                {
                    needsNewTarget = true;
                }
            }

            yield return null;
        }

        if (spinLasers != null)
        {
            foreach (GameObject laser in spinLasers)
            {
                if (laser != null) laser.SetActive(false);
            }
        }

        transform.rotation = Quaternion.identity;
        currentCooldown = Mathf.Max(0.5f, attackCooldown - (currentPhase * 0.3f));
        isAttacking = false;
    }

    private IEnumerator CaptureSmash()
    {
        isAttacking = true;

        Collider queenCol = GetComponent<Collider>();
        if (queenCol != null) queenCol.isTrigger = true;

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
                PlayerHealth ph = hit.GetComponent<PlayerHealth>();
                if (ph != null) ph.TakeDamage(smashDamage, transform);
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

        if (queenCol != null) queenCol.isTrigger = false;

        currentCooldown = Mathf.Max(0.5f, attackCooldown - (currentPhase * 0.25f));
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

        bool hitPlayer = false;

        while (Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, dashSpeed * Time.deltaTime);

            if (!hitPlayer && Vector3.Distance(transform.position, player.position) < 4f)
            {
                PlayerHealth ph = player.GetComponent<PlayerHealth>();
                if (ph != null) ph.TakeDamage(smashDamage, transform);
                hitPlayer = true;
            }

            yield return null;
        }

        transform.position = targetPos;
        currentCooldown = Mathf.Max(0.5f, attackCooldown - (currentPhase * 0.25f));
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

        currentCooldown = Mathf.Max(0.5f, attackCooldown - (currentPhase * 0.25f));
        isAttacking = false;
    }

    private void SpawnMissileWarningAround(Vector3 center, float radius)
    {
        Vector3 spawnPos = new Vector3(
            center.x + Random.Range(-radius, radius),
            transform.position.y + 0.5f,
            center.z + Random.Range(-radius, radius)
        );

        if (missileWarningPrefab != null)
        {
            Instantiate(missileWarningPrefab, spawnPos, Quaternion.identity);
        }
    }

    private void Die()
    {
        KingBossController king = FindFirstObjectByType<KingBossController>();
        if (king != null) king.MakeVulnerable();

        Destroy(gameObject);
    }
}