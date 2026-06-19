using UnityEngine;

public class DestructiblePillar : MonoBehaviour
{
    [Header("Configuración del Pilar")]
    public int health = 3;
    public float explosionForce = 200f;
    public float explosionRadius = 2f;
    public Material transparentMaterial;

    private bool isDestroyed = false;

    public void TakeDamage(int damage)
    {
        if (isDestroyed) return;

        health -= damage;
        if (health <= 0)
        {
            Shatter();
        }
    }

    private void Shatter()
    {
        isDestroyed = true;

        Collider mainCollider = GetComponent<Collider>();
        if (mainCollider != null) mainCollider.enabled = false;

        foreach (Transform child in transform)
        {
            Rigidbody rb = child.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }

            FadingDebris debrisScript = child.gameObject.GetComponent<FadingDebris>();
            if (debrisScript == null)
            {
                debrisScript = child.gameObject.AddComponent<FadingDebris>();
            }

            debrisScript.transparentMaterial = transparentMaterial;
            debrisScript.BeginFadeProcess();
        }
    }
}