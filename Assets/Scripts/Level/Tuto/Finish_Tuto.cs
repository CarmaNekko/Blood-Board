using UnityEngine;
using UnityEngine.SceneManagement;

public class Finish_Tuto : MonoBehaviour
{
    [Header("Configuración de Nivel")]
    public string nombreNivel1 = "Nivel_1";

    private bool isLoading = false;

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player") && !isLoading)
        {
            isLoading = true;

            SceneManager.LoadScene(nombreNivel1);
        }
    }
}
