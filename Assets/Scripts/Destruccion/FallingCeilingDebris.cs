using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FallingCeilingDebris : MonoBehaviour
{
    [Header("Physics and Damage")]
    [SerializeField] private int damage = 25;
    [SerializeField] private float velocityThreshold = 2f;

    [Header("Enemy Specific Damage Parameters")]
    [SerializeField] private MagicColor debrisColor;
    [SerializeField] private bool additionalDamageFlag = false;

    private bool hasLanded = false;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddForce(Vector3.down * 5f, ForceMode.VelocityChange);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasLanded) return;

        if (rb.linearVelocity.magnitude > velocityThreshold || rb.angularVelocity.magnitude > velocityThreshold)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage, transform);
                }
            }
            else if (collision.gameObject.CompareTag("Enemy"))
            {
                EnemyHealth enemyHealth = collision.gameObject.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage, debrisColor, additionalDamageFlag);
                }
            }
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("Debris"))
        {
            SetLanded();
        }
    }

    private void SetLanded()
    {
        hasLanded = true;
        rb.mass = 500f;
    }
}