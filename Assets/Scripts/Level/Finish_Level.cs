using UnityEngine;
using UnityEngine.SceneManagement;
using BloodBoard.GameManagement; // Added for ScoreManager

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

        // Usamos IsFinalFloor para determinar si el juego debe terminar.
        // Esto devolverá 'true' en el piso 5 del modo Normal, y 'false' siempre en modo Infinito.
        if (GameModeManager.CurrentMode.IsFinalFloor(LevelManager.currentLevel))
        {
            // El juego ha terminado (solo en modo Normal).
            Debug.Log($"Piso final ({LevelManager.currentLevel}) alcanzado en modo {GameModeManager.CurrentMode.GetModeName()}. Fin del juego.");
            // Muestra la pantalla de victoria en lugar de ir directamente a los créditos.
            var gameOverScreen = Object.FindFirstObjectByType<GameOver>();
            if (gameOverScreen != null) gameOverScreen.ShowGameOver(true);
        }
        else
        {
            // Avanzar al siguiente piso.
            Debug.Log($"Piso {LevelManager.currentLevel} completado. Avanzando al siguiente piso.");
            
            // Incrementar el contador de pisos.
            LevelManager.currentLevel++;

            // Guardar el progreso para el nuevo piso, incluyendo el score actual.
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            float health = playerHealth != null ? playerHealth.currentHealth : 100f;
            int currentScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetCurrentScore() : 0;
            SaveManager.SaveToSlot(GameModeManager.CurrentSlot, LevelManager.currentLevel, currentScore, health, GameModeManager.CurrentMode.GetModeName());
            Debug.Log($"Guardando progreso para el piso {LevelManager.currentLevel} en el slot {GameModeManager.CurrentSlot}.");

            // Recargar la escena del nivel. LevelManager se encargará de generar el nuevo piso.
            SceneManager.LoadScene(mainLevelScene);
        }
    }
}