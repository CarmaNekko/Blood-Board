using UnityEngine;

public class QueenPhysicalDamage : MonoBehaviour
{
    [SerializeField] private float damageAmount = 25f;
    [SerializeField] private float pushForce = 60f;
    private float lastHitTime = 0f;

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time < lastHitTime + 1f) return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null) health.TakeDamage(damageAmount, transform);

            PlayerMovement movement = other.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                Vector3 pushDirection = (other.transform.position - transform.position).normalized;
                pushDirection.y = 0;
                movement.ApplyKnockback(pushDirection, pushForce);
            }
            lastHitTime = Time.time;
        }
    }
}