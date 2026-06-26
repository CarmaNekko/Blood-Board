using UnityEngine;
using System.Collections;

public class QueenLightPillar : MonoBehaviour
{
    public float warningDuration = 2.5f;
    public float activeDuration = 0.5f;
    public float damage = 35f;

    public GameObject warningVisual;
    public GameObject activePillarVisual;
    public Collider damageCollider;

    private bool hasDealtDamage = false;

    void Start()
    {
        if (warningVisual != null) warningVisual.SetActive(true);
        if (activePillarVisual != null) activePillarVisual.SetActive(false);
        if (damageCollider != null) damageCollider.enabled = false;

        StartCoroutine(PillarRoutine());
    }

    private IEnumerator PillarRoutine()
    {
        if (warningVisual != null)
        {
            Vector3 originalScale = warningVisual.transform.localScale;
            float timer = 0f;

            while (timer < warningDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / warningDuration;

                warningVisual.transform.localScale = new Vector3(
                    originalScale.x * progress,
                    originalScale.y,
                    originalScale.z * progress
                );
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(warningDuration);
        }

        if (warningVisual != null) warningVisual.SetActive(false);
        if (activePillarVisual != null) activePillarVisual.SetActive(true);
        if (damageCollider != null) damageCollider.enabled = true;

        yield return new WaitForSeconds(activeDuration);

        Destroy(gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !hasDealtDamage)
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage, transform);
                hasDealtDamage = true;
            }
        }
    }
}