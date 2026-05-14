using UnityEngine;

public enum MagicColor { White, Black }
public class MagicProjectile : MonoBehaviour
{
    [Header("Projectile Stats")]
    [SerializeField] private MagicColor projectileColor;
    [SerializeField] private int damage = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Rooms"))
        {
            return;
        }

        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, projectileColor);
        }

        Destroy(gameObject);
    }
}