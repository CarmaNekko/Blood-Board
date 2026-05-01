using UnityEngine;
using UnityEngine.SceneManagement;
using BloodBoard.GameManagement; // Added for ScoreManager

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
            int currentScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetCurrentScore() : 0;
            ScoreManager.Instance?.ResetCurrentScore(); // Reset score after tutorial completion
            SaveManager.SaveGame(1, currentScore, 100f); // Piso 1 completado, score, health 100

            SceneManager.LoadScene(nombreNivel1);
        }
    }
}
