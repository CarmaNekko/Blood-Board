using UnityEngine;
using UnityEngine.SceneManagement;
using BloodBoard.GameManagement;

public class Finish_Level : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string mainLevelScene = "Level_1";

    private bool isLoading = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isLoading || !other.CompareTag(playerTag))
        {
            return;
        }

        isLoading = true;

        if (GameModeManager.CurrentMode.IsFinalFloor(LevelManager.currentLevel))
        {
            Debug.Log($"Piso final ({LevelManager.currentLevel}) alcanzado en modo {GameModeManager.CurrentMode.GetModeName()}. Fin del juego.");
            var gameOverScreen = Object.FindFirstObjectByType<GameOver>();
            if (gameOverScreen != null) gameOverScreen.ShowGameOver(true);
        }
        else
        {
            Debug.Log($"Piso {LevelManager.currentLevel} completado. Avanzando al siguiente piso.");
            
            LevelManager.currentLevel++;

            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            float health = playerHealth != null ? playerHealth.currentHealth : 100f;
            int currentScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetCurrentScore() : 0;
            SaveManager.SaveToSlot(GameModeManager.CurrentSlot, LevelManager.currentLevel, currentScore, health, GameModeManager.CurrentMode.GetModeName());
            Debug.Log($"Guardando progreso para el piso {LevelManager.currentLevel} en el slot {GameModeManager.CurrentSlot}.");

            SceneManager.LoadScene(mainLevelScene);
        }
    }
}
