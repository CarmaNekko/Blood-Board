using UnityEngine;

public enum MagicColor { White, Black, Harmonic }

public class MagicProjectile : MonoBehaviour
{
    [Header("Projectile Stats")]
    [SerializeField] private MagicColor projectileColor;
    [SerializeField] private int damage = 10;
    [SerializeField] private GameObject explosionParticlesPrefab;
    [SerializeField] private float destructionRadius = 1.2f;
    [SerializeField] private float impactForce = 150f;
    [SerializeField] private AudioClip impactSound;

    public bool appliesVampirism = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.gameObject.layer == LayerMask.NameToLayer("Rooms") || other.GetComponent<Checkpoint>() != null)
        {
            return;
        }

        Destruction pillar = other.GetComponentInParent<Destruction>();
        if (pillar != null)
        {
            pillar.DamageAtPoint(transform.position, destructionRadius, impactForce);
            if (explosionParticlesPrefab != null)
            {
                GameObject explosion = Instantiate(explosionParticlesPrefab, transform.position, Quaternion.identity);
                Destroy(explosion, 1.5f);
            }

            if (impactSound != null)
            {
                AudioSource.PlayClipAtPoint(impactSound, transform.position);
            }

            Destroy(gameObject);
            return;
        }

        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy != null) enemy.TakeDamage(damage, projectileColor, appliesVampirism);

        PawnBossHealth boss = other.GetComponentInParent<PawnBossHealth>();
        if (boss != null) boss.TakeDamage(damage, projectileColor);

        BishopCrystal crystal = other.GetComponentInParent<BishopCrystal>();
        if (crystal != null) crystal.TakeDamage(projectileColor);

        if (explosionParticlesPrefab != null)
        {
            GameObject explosion = Instantiate(explosionParticlesPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 1.5f);
        }

        if (impactSound != null)
        {
            AudioSource.PlayClipAtPoint(impactSound, transform.position);
        }

        PawnShield pawnShield = other.GetComponentInParent<PawnShield>();
        if (pawnShield != null)
        {
            pawnShield.TakeDamage(projectileColor);
            if (impactSound != null)
            {
                AudioSource.PlayClipAtPoint(impactSound, transform.position);
            }
            Destroy(gameObject);
            return;
        }

        QueenBossController queen = other.GetComponentInParent<QueenBossController>();
        if (queen != null) queen.TakeDamage(damage, projectileColor);

        Destroy(gameObject);
    }
}