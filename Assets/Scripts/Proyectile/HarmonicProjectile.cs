using UnityEngine;

public class HarmonicProjectile : MonoBehaviour
{
    [Header("Projectile Stats")]
    [SerializeField] private int damage = 15;
    [SerializeField] private GameObject explosionParticlesPrefab;
    [SerializeField] private float destructionRadius = 1.6f;
    [SerializeField] private float impactForce = 200f;

    public bool appliesVampirism = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.gameObject.layer == LayerMask.NameToLayer("Rooms"))
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
            Destroy(gameObject);
            return;
        }

        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null) enemy.TakeDamage(damage, MagicColor.Harmonic, appliesVampirism);

        PawnBossHealth boss = other.GetComponent<PawnBossHealth>();
        if (boss != null) boss.TakeDamage(damage, MagicColor.Harmonic);

        BishopCrystal crystal = other.GetComponent<BishopCrystal>();
        if (crystal != null) crystal.TakeDamage(MagicColor.Harmonic);

        if (explosionParticlesPrefab != null)
        {
            GameObject explosion = Instantiate(explosionParticlesPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 1.5f);
        }

        Destroy(gameObject);
    }
}