using UnityEngine;

public class BossLaser : MonoBehaviour
{
    [Header("Configuración del Láser")]
    [SerializeField] private float damageAmount = 15f;
    [SerializeField] private float knockbackForce = 15f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount, transform);
            }

            PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                Vector3 pushDirection = (other.transform.position - transform.position).normalized;

                playerMovement.ApplyKnockback(pushDirection, knockbackForce);
            }
        }
    }
}