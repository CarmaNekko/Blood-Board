using UnityEngine;

public class KnightDamageArea : MonoBehaviour
{
    [Header("Fuerza del Impacto")]
    public float knockbackForce = 60f;

    public void DealSlamDamage(float damageAmount, float radius)
    {
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.4f, 0.5f);
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damageAmount);
                }

                PlayerMovement movement = hit.GetComponent<PlayerMovement>();
                if (movement != null)
                {
                    Vector3 pushDirection = hit.transform.position - transform.position;
                    movement.ApplyKnockback(pushDirection, knockbackForce);
                }
            }
        }
    }
}
