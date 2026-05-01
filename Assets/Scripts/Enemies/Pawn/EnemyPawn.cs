using UnityEngine;
using BloodBoard.GameManagement; // Added for ScoreManager
using UnityEngine.AI;

public class EnemyPawn : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private float detectionRange = 18f;

    [Header("Combat Stats")]
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float attackCooldown = 2f;

    [Header("Ranged Attack")]
    [SerializeField] private GameObject magicProjectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Polarity & Health")]
    [SerializeField] private MagicColor pawnColor;
    [SerializeField] private int maxHealth = 10;
    private int currentHealth;

    [Header("Buff Status (Frenesí)")]
    private bool isBuffed = false;
    [SerializeField] private GameObject frenzyParticles;

    private NavMeshAgent agent;
    private float lastAttackTime;
    private bool isAwake = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = 4f;
        agent.stoppingDistance = attackRange - 1f;

        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj != null)
            {
                playerTarget = playerObj.transform;
            }
        }

        currentHealth = maxHealth;

        if (frenzyParticles != null)
        {
            frenzyParticles.SetActive(false);
        }
    }

    void Update()
    {
        if (playerTarget != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

            if (!isAwake && distanceToPlayer <= detectionRange)
            {
                isAwake = true;
            }

            if (isAwake)
            {
                if (distanceToPlayer > attackRange)
                {
                    agent.SetDestination(playerTarget.position);
                    agent.isStopped = false;
                }

                else
                {
                    agent.isStopped = true;

                    Vector3 lookDirection = (playerTarget.position - transform.position).normalized;
                    lookDirection.y = 0;
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * 5f);

                    TryAttack();
                }
            }
        }
    }

    private void TryAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            if (magicProjectilePrefab != null && firePoint != null)
            {
                GameObject projectileObj = Instantiate(magicProjectilePrefab, firePoint.position, firePoint.rotation);

                PawnProjectile projectileScript = projectileObj.GetComponent<PawnProjectile>();
                if (projectileScript != null)
                {
                    Vector3 targetLastPosition = playerTarget.position;
                    projectileScript.Setup(targetLastPosition, 10f);
                }
            }
            else
            {
                Debug.LogWarning("Falta asignar el MagicProjectilePrefab o el FirePoint en el inspector del Peón.");
            }

            lastAttackTime = Time.time;
        }
    }

    public void TakeDamage(int damageAmount, MagicColor incomingMagicColor)
    {
        if (pawnColor != incomingMagicColor)
        {
            currentHealth -= damageAmount;

            if (currentHealth <= 0)
            {
                Die();
            }
        }
        else
        {
            ApplyBuff();
        }
    }

    private void ApplyBuff()
    {
        if (isBuffed) return;

        isBuffed = true;

        if (agent != null)
        {
            agent.speed += 2.5f;
        }
        attackCooldown /= 2f;

        transform.localScale *= 1.25f;

        if (frenzyParticles != null)
        {
            frenzyParticles.SetActive(true);
        }
    }

    private void Die()
    {
        ScoreManager.Instance?.AddScoreToCurrent(100); // Example: Add 100 points per enemy kill
        Destroy(gameObject);
    }
}