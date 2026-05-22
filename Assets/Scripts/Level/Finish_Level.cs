using UnityEngine;
using UnityEngine.SceneManagement;
using BloodBoard.GameManagement;

public class Finish_Level : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    [Header("Escenas de Destino")]
    [Tooltip("El nombre exacto de la escena de tu Nivel Procedural base")]
    [SerializeField] private string escenaProcedural = "Level_1";

    [Tooltip("El nombre exacto de la escena de la persecución")]
    [SerializeField] private string escenaJefeCaballo = "Escena_Persecucion";

    private bool isLoading = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isLoading || !other.CompareTag(playerTag))
        {
            return;
        }

        isLoading = true;

        if (GameModeManager.CurrentMode != null && GameModeManager.CurrentMode.IsFinalFloor(LevelManager.currentLevel))
        {
            Debug.Log($"Piso final ({LevelManager.currentLevel}) alcanzado en modo {GameModeManager.CurrentMode.GetModeName()}. Fin del juego.");
            var gameOverScreen = Object.FindFirstObjectByType<GameOver>();
            if (gameOverScreen != null) gameOverScreen.ShowGameOver(true);
        }
        else
        {
            int proximoPiso = LevelManager.currentLevel + 1;

            string proximaEscena = DeterminarProximaEscena(proximoPiso);

            LevelManager.currentLevel = proximoPiso;

            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            float health = playerHealth != null ? playerHealth.currentHealth : 100f;
            int currentScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetCurrentScore() : 0;
            string currentModeName = GameModeManager.CurrentMode != null ? GameModeManager.CurrentMode.GetModeName() : "Modo_Historia";

            SaveManager.SaveToSlot(GameModeManager.CurrentSlot, LevelManager.currentLevel, currentScore, health, currentModeName);

            Debug.Log($"Piso completado. Guardando y avanzando al piso {LevelManager.currentLevel}. Cargando: {proximaEscena}");

            CheckerboardTransition.LoadScene(proximaEscena);
        }
    }

    private string DeterminarProximaEscena(int proximoPiso)
    {
        if (proximoPiso == 2)
        {
            return escenaJefeCaballo;
        }

        return escenaProcedural;
    }
}