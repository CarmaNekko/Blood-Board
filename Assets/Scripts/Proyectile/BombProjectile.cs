using UnityEngine;

public class BombProjectile : MonoBehaviour
{
    [Header("Bomb Stats")]
    [SerializeField] private int directDamage = 10;
    [SerializeField] private int explosionDamage = 5;
    [SerializeField] private float explosionRadius = 3f;
    public MagicColor bombColor;
    [SerializeField] private GameObject explosionParticlesPrefab;
    [SerializeField] private AudioClip impactSound;

    public bool appliesVampirism = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.gameObject.layer == LayerMask.NameToLayer("Rooms") || other.GetComponent<Checkpoint>() != null)
        {
            return;
        }

        ApplyDirectDamage(other);
        ApplyExplosion();

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
    }

    private void ApplyDirectDamage(Collider other)
    {
        Destruction pillar = other.GetComponentInParent<Destruction>();
        if (pillar != null)
        {
            pillar.DamageAtPoint(transform.position, explosionRadius, 150f);
            Destroy(gameObject);
            return;
        }

        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy != null) enemy.TakeDamage(directDamage, bombColor, appliesVampirism);

        PawnBossHealth boss = other.GetComponentInParent<PawnBossHealth>();
        if (boss != null) boss.TakeDamage(directDamage, bombColor);

        BishopCrystal crystal = other.GetComponentInParent<BishopCrystal>();
        if (crystal != null) crystal.TakeDamage(bombColor);

        PawnShield pawnShield = other.GetComponentInParent<PawnShield>();
        if (pawnShield != null)
        {
            pawnShield.TakeDamage(bombColor);
            Destroy(gameObject);
            return;
        }

        QueenBossController queen = other.GetComponentInParent<QueenBossController>();
        if (queen != null) queen.TakeDamage(directDamage, bombColor);
    }

    private void ApplyExplosion()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player") || hit.gameObject.layer == LayerMask.NameToLayer("Rooms") || hit.GetComponent<Checkpoint>() != null)
            {
                continue;
            }

            EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
            if (enemy != null) enemy.TakeDamage(explosionDamage, bombColor, appliesVampirism);

            PawnBossHealth boss = hit.GetComponentInParent<PawnBossHealth>();
            if (boss != null) boss.TakeDamage(explosionDamage, bombColor);

            BishopCrystal crystal = hit.GetComponentInParent<BishopCrystal>();
            if (crystal != null) crystal.TakeDamage(bombColor);

            QueenBossController queen = hit.GetComponentInParent<QueenBossController>();
            if (queen != null) queen.TakeDamage(explosionDamage, bombColor);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}