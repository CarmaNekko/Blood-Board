using UnityEngine;
using UnityEngine.AI;

public class EnemyPawn : MonoBehaviour
{
    [Header("Targeting")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private float detectionRange = 18f;

    [Header("Combat Stats")]
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Polarity & Health")]
    [SerializeField] private MagicColor pawnColor;
    [SerializeField] private int maxHealth = 10;
    private int currentHealth;

    private NavMeshAgent agent;
    private float lastAttackTime;
    private bool isAwake = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj != null)
            {
                playerTarget = playerObj.transform;
            }
        }

        currentHealth = maxHealth;
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
                agent.SetDestination(playerTarget.position);

                if (distanceToPlayer <= attackRange)
                {
                    TryAttack();
                }
            }
        }
    }

    private void TryAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            playerTarget.GetComponent<PlayerMovement>().TakeDamage(10f);
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
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
