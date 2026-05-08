using UnityEngine;
using UnityEngine.SceneManagement;
using BloodBoard.GameManagement;

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

            int currentScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetCurrentScore() : 0;
            ScoreManager.Instance?.ResetCurrentScore();
            SaveManager.SaveGame(1, currentScore, 100f);

            SceneManager.LoadScene(nombreNivel1);
        }
    }
}
