using UnityEngine;
using System.Collections;

public class BishopPlatform : MonoBehaviour
{
    [Header("Configuración de Estados")]
    [SerializeField] private Material warningMaterial;
    [SerializeField] private float warningDuration = 1.5f;
    [SerializeField] private float brokenDuration = 4f;

    [Header("El Castigo Divino")]
    [SerializeField] private GameObject beamPrefab;
    [SerializeField] private float beamDuration = 1f;

    private Material originalMaterial;
    private Renderer platformRenderer;
    private Collider platformCollider;

    public bool IsTargeted { get; private set; } = false;

    void Awake()
    {
        platformRenderer = GetComponent<Renderer>();
        platformCollider = GetComponent<Collider>();
        if (platformRenderer != null)
        {
            originalMaterial = platformRenderer.material;
        }
    }

    public void TargetPlatform()
    {
        if (!IsTargeted)
        {
            StartCoroutine(DestructionSequence());
        }
    }

    private IEnumerator DestructionSequence()
    {
        IsTargeted = true;

        if (platformRenderer != null && warningMaterial != null)
        {
            platformRenderer.material = warningMaterial;
        }

        yield return new WaitForSeconds(warningDuration);

        GameObject beamInstance = null;
        if (beamPrefab != null)
        {
            beamInstance = Instantiate(beamPrefab, transform.position, Quaternion.identity);
            StartCoroutine(AnimateBeam(beamInstance.transform));
        }

        if (platformRenderer != null) platformRenderer.enabled = false;
        if (platformCollider != null) platformCollider.enabled = false;

        yield return new WaitForSeconds(beamDuration);
        if (beamInstance != null) Destroy(beamInstance);

        yield return new WaitForSeconds(brokenDuration);

        if (platformRenderer != null)
        {
            platformRenderer.material = originalMaterial;
            platformRenderer.enabled = true;
        }
        if (platformCollider != null) platformCollider.enabled = true;

        IsTargeted = false;
    }

    private IEnumerator AnimateBeam(Transform beamTransform)
    {
        float t = 0;
        Vector3 originalScale = beamTransform.localScale;

        beamTransform.position += Vector3.down * 40f;

        while (t < 1f)
        {
            t += Time.deltaTime / 0.15f;
            beamTransform.localScale = new Vector3(originalScale.x, Mathf.Lerp(0, 150f, t), originalScale.z);
            yield return null;
        }
        beamTransform.localScale = new Vector3(originalScale.x, 150f, originalScale.z);
    }
}