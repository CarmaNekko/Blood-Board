using UnityEngine;
using System.Collections;

public class BishopMagicStrike : MonoBehaviour
{
    [Header("Daño y Área")]
    public float damage = 20f;
    public float impactRadius = 2.5f;
    public float fallDelay = 1.2f;

    [Header("Visuales y Efectos")]
    public GameObject dangerZoneIndicator;
    public GameObject explosionParticles;

    [Header("Telegrafiado")]
    public bool expandirHaciaAfuera = true;
    private Vector3 escalaMaximaIndicator;

    private void Start()
    {
        if (dangerZoneIndicator != null)
        {
            escalaMaximaIndicator = dangerZoneIndicator.transform.localScale;
        }

        StartCoroutine(StrikeRoutine());
    }

    private IEnumerator StrikeRoutine()
    {
        if (dangerZoneIndicator != null)
        {
            dangerZoneIndicator.SetActive(true);
            StartCoroutine(AnimarIndicadorVisual());
        }
        yield return new WaitForSeconds(fallDelay);

        if (dangerZoneIndicator != null)
        {
            dangerZoneIndicator.SetActive(false);
        }

        if (explosionParticles != null)
        {
            Instantiate(explosionParticles, transform.position, Quaternion.identity);
        }

        DealAreaDamage();

        Destroy(gameObject, 0.2f);
    }

    private IEnumerator AnimarIndicadorVisual()
    {
        float timer = 0f;
        Transform indicatorTransform = dangerZoneIndicator.transform;

        while (timer < fallDelay)
        {
            timer += Time.deltaTime;
            float t = timer / fallDelay;
            float scaleValue;
            if (expandirHaciaAfuera)
            {
                scaleValue = t;
            }
            else
            {
                scaleValue = 1f - t;
            }

            indicatorTransform.localScale = new Vector3(
                escalaMaximaIndicator.x * scaleValue,
                escalaMaximaIndicator.y,
                escalaMaximaIndicator.z * scaleValue
            );

            yield return null;
        }

        dangerZoneIndicator.SetActive(false);
    }

    private void DealAreaDamage()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, impactRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage, transform);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, impactRadius);
    }
}