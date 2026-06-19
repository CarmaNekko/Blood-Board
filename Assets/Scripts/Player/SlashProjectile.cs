using UnityEngine;

public class SlashProjectile : MonoBehaviour
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

        bool hitTarget = false;

        DestructiblePillar pillar = other.GetComponent<DestructiblePillar>();
        if (pillar != null)
        {
            pillar.TakeDamage(1);
            hitTarget = true;
        }

        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, projectileColor, appliesVampirism);
            hitTarget = true;
        }

        PawnBossHealth boss = other.GetComponent<PawnBossHealth>();
        if (boss != null)
        {
            boss.TakeDamage(damage, projectileColor);
            hitTarget = true;
        }

        BishopCrystal crystal = other.GetComponent<BishopCrystal>();
        if (crystal != null)
        {
            crystal.TakeDamage(projectileColor);
            hitTarget = true;
        }

        if (hitTarget && explosionParticlesPrefab != null)
        {
            GameObject explosion = Instantiate(explosionParticlesPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 1.5f);
        }
    }
}