using UnityEngine;

public class MagicBarrier : MonoBehaviour
{
    [Header("Configuración Aleatoria")]
    public Material materialBlanco;
    public Material materialNegro;

    [Header("Efectos Visuales")]
    public GameObject shatterParticles;

    private bool isWhiteBarrier;
    private MeshRenderer meshRenderer;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
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
            if (!isWhiteBarrier) BreakBarrier();
            else Destroy(other.gameObject);
        }
        else if (other.CompareTag("BlackMagic"))
        {
            if (isWhiteBarrier) BreakBarrier();
            else Destroy(other.gameObject);
        }
    }

    private void BreakBarrier()
    {
        if (shatterParticles != null)
        {
            Instantiate(shatterParticles, transform.position, transform.rotation);
        }

        gameObject.SetActive(false);
    }
}