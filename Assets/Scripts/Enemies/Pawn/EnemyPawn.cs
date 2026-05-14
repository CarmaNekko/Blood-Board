using UnityEngine;
using UnityEngine.AI;
public class EnemyPawn : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private Transform playerTarget;
    // ¡Adiós al detectionRange!

    [Header("Combat Stats Base")]
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float baseSpeed = 4f;

    [Header("Ranged Attack")]
    [SerializeField] private GameObject magicProjectilePrefab;
    [SerializeField] private Transform firePoint;

    private NavMeshAgent agent;
    private float lastAttackTime;
    private EnemyHealth healthScript;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        healthScript = GetComponent<EnemyHealth>();

        agent.speed = baseSpeed;
        agent.stoppingDistance = attackRange - 1f;

        if (playerTarget == null)
        {
            // Usamos el Tag en lugar del nombre, es más seguro y rápido
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTarget = playerObj.transform;
            }
        }
    }

    void Update()
    {
        // Actualizamos la velocidad por si recibe un buff
        agent.speed = healthScript.isBuffed ? baseSpeed + 2.5f : baseSpeed;

        if (playerTarget != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

            // ¡Ya no hay fase de sueño! Si están lejos, corren; si están cerca, disparan.
            if (distanceToPlayer > attackRange)
            {
                agent.SetDestination(playerTarget.position);
                agent.isStopped = false;
            }
            else
            {
                agent.isStopped = true;

                // Mirar fijamente al jugador
                Vector3 lookDirection = (playerTarget.position - transform.position).normalized;
                lookDirection.y = 0;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * 5f);

                TryAttack();
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
                    Vector3 targetLastPosition = playerTarget.position;

                    float damage = healthScript.isBuffed ? 20f : 10f;
                    float size = healthScript.isBuffed ? 1.5f : 1f;

                    projectileScript.Setup(targetLastPosition, damage, size);
                }
            }
            else
            {
                Debug.LogWarning("Falta asignar el MagicProjectilePrefab o el FirePoint en el inspector del Peón.");
            }

            lastAttackTime = Time.time;
        }
    }
}