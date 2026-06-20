using UnityEngine;

public enum MagicColor { White, Black, Harmonic }

public class MagicProjectile : MonoBehaviour
{
    [Header("Projectile Stats")]
    [SerializeField] private MagicColor projectileColor;
    [SerializeField] private int damage = 10;
    [SerializeField] private GameObject explosionParticlesPrefab;

    public bool appliesVampirism = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.gameObject.layer == LayerMask.NameToLayer("Rooms"))
        {
            return;
        }

        DestructiblePillar pillar = other.GetComponent<DestructiblePillar>();
        if (pillar != null)
        {
            pillar.TakeDamage(1);
            if (explosionParticlesPrefab != null)
            {
                GameObject explosion = Instantiate(explosionParticlesPrefab, transform.position, Quaternion.identity);
                Destroy(explosion, 1.5f);
            }
            Destroy(gameObject);
            return;
        }

        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null) enemy.TakeDamage(damage, projectileColor, appliesVampirism);

        PawnBossHealth boss = other.GetComponent<PawnBossHealth>();
        if (boss != null) boss.TakeDamage(damage, projectileColor);

        BishopCrystal crystal = other.GetComponent<BishopCrystal>();
        if (crystal != null) crystal.TakeDamage(projectileColor);

        if (explosionParticlesPrefab != null)
        {
            GameObject explosion = Instantiate(explosionParticlesPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 1.5f);
        }

        PawnShield pawnShield = other.GetComponent<PawnShield>();
        if (pawnShield != null)
        {
            pawnShield.TakeDamage(projectileColor);
            Destroy(gameObject);
            return;
        }

        QueenBossController queen = other.GetComponent<QueenBossController>();
        if (queen != null) queen.TakeDamage(damage, projectileColor);

        Destroy(gameObject);
    }
}