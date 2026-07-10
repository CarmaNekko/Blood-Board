using UnityEngine;

public class LaserProjectile : MonoBehaviour
{
    [Header("Laser Stats")]
    [SerializeField] private int damage = 5;
    public MagicColor laserColor;
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
            pillar.DamageAtPoint(transform.position, 1.2f, 150f);
            Destroy(gameObject);
            return;
        }

        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy != null) enemy.TakeDamage(damage, laserColor, appliesVampirism);

        PawnBossHealth boss = other.GetComponentInParent<PawnBossHealth>();
        if (boss != null) boss.TakeDamage(damage, laserColor);

        BishopCrystal crystal = other.GetComponentInParent<BishopCrystal>();
        if (crystal != null) crystal.TakeDamage(laserColor);

        PawnShield pawnShield = other.GetComponentInParent<PawnShield>();
        if (pawnShield != null)
        {
            pawnShield.TakeDamage(laserColor);
            Destroy(gameObject);
            return;
        }

        QueenBossController queen = other.GetComponentInParent<QueenBossController>();
        if (queen != null) queen.TakeDamage(damage, laserColor);

        Destroy(gameObject);
    }
}