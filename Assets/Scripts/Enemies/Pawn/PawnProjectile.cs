using UnityEngine;

public class PawnProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private float explosionForce = 15f;

    private Vector3 targetPosition;
    private Vector3 moveDirection;
    private float damage;
    private bool isSetup = false;

    public void Setup(Vector3 targetPos, float projectileDamage, float sizeMultiplier)
    {
        targetPosition = targetPos;
        damage = projectileDamage;

        transform.localScale *= sizeMultiplier;

        moveDirection = (targetPosition - transform.position).normalized;
        transform.forward = moveDirection;
        isSetup = true;
        Destroy(gameObject, 4f);
    }

    private void Update()
    {
        if (isSetup)
        {
            transform.position += moveDirection * speed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Rooms"))
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage, transform);
            }
        }
        else if (other.CompareTag("Pilar"))
        {
            Destruction pillarDestruction = other.GetComponent<Destruction>();
            if (pillarDestruction != null)
            {
                pillarDestruction.DamageAtPoint(transform.position, explosionRadius, explosionForce);
            }
        }

        Destroy(gameObject);
    }
}