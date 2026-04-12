using UnityEngine;

public enum MagicColor { White, Black }
public class MagicProjectile : MonoBehaviour
{
    [Header("Projectile Stats")]
    [SerializeField] private MagicColor projectileColor;
    [SerializeField] private int damage = 10;

    private void OnTriggerEnter(Collider other)
    {
        EnemyPawn enemy = other.GetComponent<EnemyPawn>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage, projectileColor);
        }

        Destroy(gameObject);
    }
}
