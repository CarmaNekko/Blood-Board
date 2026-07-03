using UnityEngine;
using System.Collections.Generic;

public class BulletRainProjectile : MonoBehaviour
{
    [Header("Particle Bullet Rain Stats")]
    [SerializeField] private MagicColor projectileColor;
    [SerializeField] private int damagePerParticle = 10;
    [SerializeField] private float destructionRadius = 0.6f;
    [SerializeField] private float impactForce = 60f;
    [SerializeField] private AudioClip impactSound;

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
        if (other.CompareTag("Player") || other.gameObject.layer == LayerMask.NameToLayer("Rooms") || other.GetComponent<Checkpoint>() != null)
        {
            return;
        }

        partSystem.GetCollisionEvents(other, collisionEvents);
        Vector3 impactPosition = transform.position;
        if (collisionEvents.Count > 0)
        {
            impactPosition = collisionEvents[0].intersection;
        }

        Destruction pillar = other.GetComponentInParent<Destruction>();
        if (pillar != null)
        {
            pillar.DamageAtPoint(impactPosition, destructionRadius, impactForce);
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

        if (impactSound != null && collisionEvents.Count > 0)
        {
            AudioSource.PlayClipAtPoint(impactSound, impactPosition);
        }
    }
}