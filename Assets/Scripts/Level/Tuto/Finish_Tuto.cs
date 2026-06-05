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
            string currentModeName = GameModeManager.CurrentMode != null ? GameModeManager.CurrentMode.GetModeName() : "Normal";
            SaveManager.SaveToSlot(GameModeManager.CurrentSlot, 1, currentScore, 100f, currentModeName);
            PlayerHealth.SetPersistedHealth(100f);
            LevelManager.currentLevel = 1;
            BossCheckpointState.SetLevelCheckpoint();
            ScoreManager.Instance?.ResetCurrentScore();

            SceneManager.LoadScene(nombreNivel1);
        }
    }
}
