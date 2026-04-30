using UnityEngine;
using UnityEngine.SceneManagement;

public class Finish_Tuto : MonoBehaviour
{
    [Header("Configuración de Nivel")]
    public string nombreNivel1 = "Level_1";

    private bool isLoading = false;

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player") && !isLoading)
        {
            isLoading = true;

            // Guardar progreso al completar tutorial
            SaveManager.SaveGame(1, 0, 100f); // Piso 1 completado, score 0, health 100

            SceneManager.LoadScene(nombreNivel1);
        }
    }
}
