using UnityEngine;
using System.Collections;

public class BishopPlatform : MonoBehaviour
{
    [Header("Configuración de Estados")]
    [SerializeField] private Material warningMaterial;
    [SerializeField] private float warningDuration = 1.5f;
    [SerializeField] private float brokenDuration = 4f;

    [Header("Efecto de Temblor")]
    [SerializeField] private float shakeIntensity = 0.1f;
    [SerializeField] private float shakeSpeed = 50f;

    [Header("El Castigo Divino")]
    [SerializeField] private GameObject beamPrefab;
    [SerializeField] private float beamDuration = 1f;

    [Header("Audio")]
    [SerializeField] private AudioClip warningSound;
    [SerializeField] private AudioClip beamStrikeSound;
    [SerializeField, Range(0f, 1f)] private float audioVolume = 0.8f;

    private Material originalMaterial;
    private Renderer platformRenderer;
    private Collider platformCollider;
    private Vector3 originalPosition;
    private AudioSource audioSource;

    public bool IsTargeted { get; private set; } = false;

    void Awake()
    {
        platformRenderer = GetComponent<Renderer>();
        platformCollider = GetComponent<Collider>();
        originalPosition = transform.position;

        if (platformRenderer != null)
        {
            originalMaterial = platformRenderer.material;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
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

        if (warningSound != null)
        {
            audioSource.clip = warningSound;
            audioSource.volume = audioVolume;
            audioSource.loop = true;
            audioSource.Play();
        }

        float elapsed = 0f;
        while (elapsed < warningDuration)
        {
            elapsed += Time.deltaTime;
            float offsetX = Mathf.PerlinNoise(Time.time * shakeSpeed, 0) * 2f - 1f;
            float offsetZ = Mathf.PerlinNoise(0, Time.time * shakeSpeed) * 2f - 1f;
            transform.position = originalPosition + new Vector3(offsetX, 0, offsetZ) * shakeIntensity;
            yield return null;
        }

        transform.position = originalPosition;

        if (audioSource.isPlaying) audioSource.Stop();
        if (beamStrikeSound != null) audioSource.PlayOneShot(beamStrikeSound, audioVolume);

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