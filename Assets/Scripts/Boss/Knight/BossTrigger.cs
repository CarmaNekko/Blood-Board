using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    [Header("Referencias")]
    public EndlessCorridor corredor;
    public GameObject salaInicial;

    public float coordenadaDeInicio = 30f;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            corredor.IniciarPasillo(coordenadaDeInicio);

            if (salaInicial != null)
            {
                Destroy(salaInicial, 3f);
            }
            gameObject.SetActive(false);
        }
    }
}