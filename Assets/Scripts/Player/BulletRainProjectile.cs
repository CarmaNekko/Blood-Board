using UnityEngine;
using System.Collections.Generic;

public class BulletRainProjectile : MonoBehaviour
{
    [Header("Particle Bullet Rain Stats")]
    [SerializeField] private MagicColor projectileColor;
    [SerializeField] private int damagePerParticle = 10;

    public bool appliesVampirism = false;

    private ParticleSystem partSystem;
    private List<ParticleCollisionEvent> collisionEvents;

    void Start()
    {
        partSystem = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Player") || other.gameObject.layer == LayerMask.NameToLayer("Rooms"))
        {
            return;
        }

        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damagePerParticle, projectileColor, appliesVampirism);
        }

        PawnBossHealth boss = other.GetComponent<PawnBossHealth>();
        if (boss != null)
        {
            boss.TakeDamage(damagePerParticle, projectileColor);
        }

        BishopCrystal crystal = other.GetComponent<BishopCrystal>();
        if (crystal != null)
        {
            crystal.TakeDamage(projectileColor);
        }
    }
}