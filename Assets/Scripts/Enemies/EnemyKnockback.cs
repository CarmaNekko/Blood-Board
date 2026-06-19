using UnityEngine;

public class EnemyKnockback : MonoBehaviour
{
    [Header("Knockback")]
    public float KnockbackDistance = 2f;
    public float KnockbackForce = 20f;

    private void Update()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance < KnockbackDistance)
        {
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();

            if (playerMovement != null)
            {
                Vector3 direction =
                    (player.transform.position - transform.position).normalized;

                playerMovement.ApplyKnockback(direction, KnockbackForce);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, KnockbackDistance);
    }
}