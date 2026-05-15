using UnityEngine;
using UnityEngine.Events;

public class TutorialStatue : MonoBehaviour
{
    [Header("Polarity Setup")]
    public bool isWhiteStatue;

    [Header("Effects")]
    public GameObject explosionParticles;
    public float growthFactor = 1.2f;

    [Header("Cinematic Connection")]
    public UnityEvent OnStatueDestroyed;

    private void OnTriggerEnter(Collider other)
    {
        if (isWhiteStatue && other.CompareTag("WhiteMagic"))
        {
            DestroyStatue();
        }
        else if (!isWhiteStatue && other.CompareTag("BlackMagic"))
        {
            DestroyStatue();
        }
        else if (isWhiteStatue && other.CompareTag("BlackMagic"))
        {
            GrowStatue();
            Destroy(other.gameObject);
        }
        else if (!isWhiteStatue && other.CompareTag("WhiteMagic"))
        {
            GrowStatue();
            Destroy(other.gameObject);
        }
    }

    private void GrowStatue()
    {
        transform.localScale *= growthFactor;
    }

    private void DestroyStatue()
    {
        if (explosionParticles != null)
        {
            Instantiate(explosionParticles, transform.position, Quaternion.identity);
        }

        OnStatueDestroyed.Invoke();
        Destroy(gameObject);
    }
}
