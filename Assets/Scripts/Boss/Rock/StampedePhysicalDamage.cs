using UnityEngine;

public class StampedePhysicalDamage : MonoBehaviour
{
    [SerializeField] private float damageAmount = 25f;
    [SerializeField] private float pushForce = 60f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damageAmount, transform);
            }

            PlayerMovement movement = other.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                Vector3 pushDirection = transform.forward;
                movement.ApplyKnockback(pushDirection, pushForce);
            }
        }
    }
}