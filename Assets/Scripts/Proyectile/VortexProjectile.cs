using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class VortexProjectile : MonoBehaviour
{
    [Header("Vortex Stats")]
    [SerializeField] private MagicColor projectileColor;
    [SerializeField] private int damage = 5;
    [SerializeField] private float pullRadius = 12f;
    [SerializeField] private float pullSpeed = 8f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private GameObject explosionParticlesPrefab;
    [SerializeField] private float destructionRadius = 1.2f;
    [SerializeField] private float impactForce = 250f;
    [SerializeField] private float attractionDurationOnWallHit = 0.5f;
    [SerializeField] private AudioClip impactSound;

    public bool appliesVampirism = false;
    private Rigidbody myRb;
    private bool hasHit = false;

    void Awake()
    {
        myRb = GetComponent<Rigidbody>();
        myRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit || other.CompareTag("Player") || other.gameObject.layer == LayerMask.NameToLayer("Rooms") || other.GetComponent<Checkpoint>() != null)
        {
            return;
        }

        bool hitTarget = false;

        int wallLayer = LayerMask.NameToLayer("Wall");
        int groundLayer = LayerMask.NameToLayer("Ground");
        if ((wallLayer != -1 && other.gameObject.layer == wallLayer) || (groundLayer != -1 && other.gameObject.layer == groundLayer))
        {
            if (impactSound != null)
            {
                AudioSource.PlayClipAtPoint(impactSound, transform.position);
            }
            hasHit = true;
            StartCoroutine(ApplyAttractionAtPoint(transform.position, pullRadius, pullSpeed, attractionDurationOnWallHit, enemyLayer));
            if (explosionParticlesPrefab != null)
            {
                GameObject explosion = Instantiate(explosionParticlesPrefab, transform.position, Quaternion.identity);
                Destroy(explosion, 1.5f);
            }
            DisableAndDestroyAfter(attractionDurationOnWallHit);
            return;
        }

        Destruction pillar = other.GetComponentInParent<Destruction>();
        if (pillar != null)
        {
            pillar.DamageAtPoint(transform.position, destructionRadius, impactForce);
            if (impactSound != null)
            {
                AudioSource.PlayClipAtPoint(impactSound, transform.position);
            }
            if (explosionParticlesPrefab != null)
            {
                GameObject explosion = Instantiate(explosionParticlesPrefab, transform.position, Quaternion.identity);
                Destroy(explosion, 1.5f);
            }
            Destroy(gameObject);
            return;
        }

        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, projectileColor, appliesVampirism);
            ApplyVerticalKnockback(enemy.transform);
            hitTarget = true;
        }

        PawnBossHealth boss = other.GetComponentInParent<PawnBossHealth>();
        if (boss != null)
        {
            boss.TakeDamage(damage, projectileColor);
            ApplyVerticalKnockback(boss.transform);
            hitTarget = true;
        }

        QueenBossController queen = other.GetComponentInParent<QueenBossController>();
        if (queen != null)
        {
            queen.TakeDamage(damage, projectileColor);
            ApplyVerticalKnockback(queen.transform);
            hitTarget = true;
        }

        if (hitTarget)
        {
            if (impactSound != null)
            {
                AudioSource.PlayClipAtPoint(impactSound, other.ClosestPoint(transform.position));
            }
            hasHit = true;
            StartCoroutine(ApplyAttractionAtPoint(transform.position, pullRadius, pullSpeed, attractionDurationOnWallHit, enemyLayer));
            if (explosionParticlesPrefab != null)
            {
                GameObject explosion = Instantiate(explosionParticlesPrefab, other.ClosestPoint(transform.position), Quaternion.identity);
                Destroy(explosion, 1.5f);
            }
            DisableAndDestroyAfter(attractionDurationOnWallHit);
            return;
        }

        Destroy(gameObject);
    }

    private void DisableAndDestroyAfter(float delay)
    {
        if (myRb != null) myRb.isKinematic = true;
        GetComponent<Collider>().enabled = false;

        foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        foreach (var ps in GetComponentsInChildren<ParticleSystem>()) ps.Stop();

        Destroy(gameObject, delay);
    }

    private void ApplyVerticalKnockback(Transform target)
    {
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            targetRb.AddForce(Vector3.up * impactForce, ForceMode.Impulse);
        }
    }

    private System.Collections.IEnumerator ApplyAttractionAtPoint(Vector3 center, float radius, float speed, float duration, LayerMask layerMask)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Collider[] colliders = Physics.OverlapSphere(center, radius, layerMask);
            foreach (Collider hit in colliders)
            {
                Transform enemyTransform = null;
                if (hit.GetComponentInParent<EnemyHealth>()) enemyTransform = hit.GetComponentInParent<EnemyHealth>().transform;
                else if (hit.GetComponentInParent<PawnBossHealth>()) enemyTransform = hit.GetComponentInParent<PawnBossHealth>().transform;
                else if (hit.GetComponentInParent<QueenBossController>()) enemyTransform = hit.GetComponentInParent<QueenBossController>().transform;

                if (enemyTransform != null)
                {
                    enemyTransform.position = Vector3.MoveTowards(enemyTransform.position, center, speed * Time.deltaTime);
                }
            }
            yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pullRadius);
    }
}
