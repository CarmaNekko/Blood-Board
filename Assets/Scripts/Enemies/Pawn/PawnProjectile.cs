using UnityEngine;

public class PawnProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
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

    void Update()
    {
        if (isSetup)
        {
            transform.position += moveDirection * speed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}