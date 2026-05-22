using UnityEngine;

public class BishopAttack : MonoBehaviour
{
    [Header("Stats de Ataque")]
    public float damage = 20f;
    public float attackCooldown = 3f;

    [Header("Referencias")]
    public GameObject magicStrikePrefab;
    public GameObject projectilePrefab;
    public LayerMask groundMask = Physics.AllLayers;

    private Transform playerTarget;
    private EnemyHealth healthScript;
    private float lastAttackTime;

    void Start()
    {
        healthScript = GetComponent<EnemyHealth>();
        playerTarget = GameObject.FindGameObjectWithTag("Player")?.transform;
        lastAttackTime = Time.time + Random.Range(-attackCooldown, 0f);
    }

    void Update()
    {
        if (playerTarget == null) return;

        Vector3 lookDirection = (playerTarget.position - transform.position).normalized;
        lookDirection.y = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * 5f);

        TryAttack();
    }

    private void TryAttack()
    {
        float currentCooldown = healthScript.isBuffed ? (attackCooldown / 2f) : attackCooldown;

        if (Time.time >= lastAttackTime + currentCooldown)
        {
            LaunchMagicStrike();
            lastAttackTime = Time.time;
        }
    }

    private void LaunchMagicStrike()
    {
        Vector3 targetPos = playerTarget.position;
        if (Physics.Raycast(playerTarget.position + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 50f, groundMask))
        {
            targetPos = hit.point + new Vector3(0, 0.05f, 0);
        }

        GameObject strike = Instantiate(magicStrikePrefab, targetPos, Quaternion.identity);
        BishopMagicStrike strikeScript = strike.GetComponent<BishopMagicStrike>();

        float currentDamage = healthScript.isBuffed ? damage * 1.5f : damage;
        if (strikeScript != null)
        {
            strikeScript.damage = currentDamage;
        }

        float delay = strikeScript != null ? strikeScript.fallDelay : 1.2f;

        if (projectilePrefab != null)
        {
            GameObject proj = Instantiate(projectilePrefab, transform.position + Vector3.up, Quaternion.identity);
            ProjectileTravel travel = proj.GetComponent<ProjectileTravel>();
            if (travel != null) travel.Setup(targetPos, delay);
        }

        if (healthScript.isBuffed && strikeScript != null)
        {
            strike.transform.localScale *= 1.5f;
            strikeScript.impactRadius *= 1.5f;
        }
    }
}