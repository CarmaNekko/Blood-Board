using UnityEngine;
using UnityEngine.SceneManagement;

public class Finish_Level : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string mainLevelScene = "Level_1";
    [SerializeField] private string endScene = "Credits"; // Escena final para el modo Normal

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
            SceneManager.LoadScene(endScene);
        }
        else
        {
            // Avanzar al siguiente piso.
            Debug.Log($"Piso {LevelManager.currentLevel} completado. Avanzando al siguiente piso.");
            
            // Incrementar el contador de pisos.
            LevelManager.currentLevel++;

            // Guardar el progreso para el nuevo piso.
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            float health = playerHealth != null ? playerHealth.currentHealth : 100f;
            // Nota: El score se guarda como 0, siguiendo la lógica existente. Si tienes un ScoreManager, deberías obtener el score actual aquí.
            SaveManager.SaveToSlot(GameModeManager.CurrentSlot, LevelManager.currentLevel, 0, health, GameModeManager.CurrentMode.GetModeName());
            Debug.Log($"Guardando progreso para el piso {LevelManager.currentLevel} en el slot {GameModeManager.CurrentSlot}.");

            // Recargar la escena del nivel. LevelManager se encargará de generar el nuevo piso.
            SceneManager.LoadScene(mainLevelScene);
        }
    }
}