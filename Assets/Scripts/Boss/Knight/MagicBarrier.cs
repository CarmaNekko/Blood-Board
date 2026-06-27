using UnityEngine;

public class MagicBarrier : MonoBehaviour
{
    [Header("Configuración Aleatoria")]
    public Material materialBlanco;
    public Material materialNegro;

    [Header("Efectos Visuales")]
    public GameObject shatterParticles;

    [Header("Audio")]
    public AudioClip breakSound;
    [Range(0f, 1f)] public float breakSoundVolume = 1f;
    public AudioClip wrongMagicSound;
    [Range(0f, 1f)] public float wrongMagicVolume = 0.5f;

    private bool isWhiteBarrier;
    private MeshRenderer meshRenderer;
    private AudioSource audioSource;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;

        ResetBarrier();
    }

    public void ResetBarrier()
    {
        gameObject.SetActive(true);

        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();

        int randomColor = Random.Range(0, 2);
        if (randomColor == 0)
        {
            isWhiteBarrier = true;
            meshRenderer.material = materialBlanco;
        }
        else
        {
            isWhiteBarrier = false;
            meshRenderer.material = materialNegro;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        ProcessHit(other);
    }

    private void OnTriggerStay(Collider other)
    {
        ProcessHit(other);
    }

    private void ProcessHit(Collider other)
    {
        if (other.CompareTag("WhiteMagic"))
        {
            if (!isWhiteBarrier)
            {
                BreakBarrier();
            }
            else
            {
                PlayDeflectSound();
                Destroy(other.gameObject);
            }
        }
        else if (other.CompareTag("BlackMagic"))
        {
            if (isWhiteBarrier)
            {
                BreakBarrier();
            }
            else
            {
                PlayDeflectSound();
                Destroy(other.gameObject);
            }
        }
    }

    private void PlayDeflectSound()
    {
        if (wrongMagicSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(wrongMagicSound, wrongMagicVolume);
        }
    }

    private void BreakBarrier()
    {
        if (shatterParticles != null)
        {
            Instantiate(shatterParticles, transform.position, transform.rotation);
        }

        if (breakSound != null)
        {
            Vector3 soundPos = Camera.main != null ? Camera.main.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(breakSound, soundPos, breakSoundVolume);
        }

        gameObject.SetActive(false);
    }
}