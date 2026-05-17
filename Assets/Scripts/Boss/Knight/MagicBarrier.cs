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
        ResetBarrier(); // Llamamos a la función de revivir al inicio
    }

    // Esta es la nueva función mágica que el pasillo llamará
    public void ResetBarrier()
    {
        // 1. Volver a encender el objeto por si estaba apagado
        gameObject.SetActive(true);

        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();

        // 2. Tirar la moneda de nuevo para cambiar el color
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

        // ¡EL CAMBIO CLAVE! En lugar de Destroy, lo APAGAMOS
        gameObject.SetActive(false);
    }
}