using UnityEngine;
using UnityEngine.AI;

public class EnemyPawn : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private float pillarDetectionRadius = 5f;

    [Header("Combat Stats Base")]
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float baseSpeed = 4f;

    [Header("Ranged Attack")]
    [SerializeField] private GameObject magicProjectilePrefab;
    [SerializeField] private Transform firePoint;

    private Transform mainPlayer;
    private Transform currentTarget;
    private NavMeshAgent agent;
    private float lastAttackTime;
    private EnemyHealth healthScript;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        healthScript = GetComponent<EnemyHealth>();

        agent.speed = baseSpeed;
        agent.stoppingDistance = attackRange - 1f;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            mainPlayer = playerObj.transform;
        }
    }

    private void Update()
    {
        UpdateTarget();

        agent.speed = healthScript.isBuffed ? baseSpeed + 2.5f : baseSpeed;

        if (currentTarget != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

            if (distanceToTarget > attackRange)
            {
                if (agent.isOnNavMesh)
                {
                    agent.SetDestination(currentTarget.position);
                    agent.isStopped = false;
                }
            }
            else
            {
                if (agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                }

                Vector3 lookDirection = (currentTarget.position - transform.position).normalized;
                lookDirection.y = 0;
                if (lookDirection != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * 5f);
                }

                TryAttack();
            }
        }
    }

    private void UpdateTarget()
    {
        if (mainPlayer == null) return;

        currentTarget = mainPlayer;
        Collider[] nearbyObjects = Physics.OverlapSphere(mainPlayer.position, pillarDetectionRadius);

        foreach (Collider col in nearbyObjects)
        {
            if (col.CompareTag("Pilar"))
            {
                currentTarget = col.transform;
                break;
            }
        }
    }

    private void TryAttack()
    {
        float currentCooldown = healthScript.isBuffed ? (attackCooldown / 2f) : attackCooldown;

        if (Time.time >= lastAttackTime + currentCooldown)
        {
            if (magicProjectilePrefab != null && firePoint != null)
            {
                GameObject projectileObj = Instantiate(magicProjectilePrefab, firePoint.position, firePoint.rotation);

                PawnProjectile projectileScript = projectileObj.GetComponent<PawnProjectile>();
                if (projectileScript != null)
                {
                    Vector3 aimPosition = currentTarget.position;

                    if (currentTarget.CompareTag("Pilar"))
                    {
                        aimPosition += Vector3.up * 1f;
                    }

                    float damage = healthScript.isBuffed ? 20f : 10f;
                    float size = healthScript.isBuffed ? 1.5f : 1f;

                    projectileScript.Setup(aimPosition, damage, size);
                }
            }

            lastAttackTime = Time.time;
        }
    }
}