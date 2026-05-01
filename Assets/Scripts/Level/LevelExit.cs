using UnityEngine;
using UnityEngine.SceneManagement;
using BloodBoard.GameManagement; // Added for ScoreManager

public class LevelExit : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("LevelExit adjunto en: " + gameObject.name);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger activado por: " + other.gameObject.name + ", tag: " + other.tag + ", piso actual: " + LevelManager.currentLevel);
        if (other.CompareTag("Player"))
        {
            int currentFloor = LevelManager.currentLevel;
            float playerHealth = other.GetComponent<PlayerHealth>()?.currentHealth ?? 100f;
            int currentScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetCurrentScore() : 0;

            SaveManager.SaveToSlot(GameModeManager.CurrentSlot, currentFloor, currentScore, playerHealth, GameModeManager.CurrentMode.GetModeName());
            Debug.Log($"Guardando en piso: {currentFloor}, Score: {currentScore}");

            // La lógica de avance de nivel ahora está centralizada en Finish_Level.cs
            // para evitar el doble incremento de pisos. Se ha eliminado de este script
            // para corregir el bug.
        }
    }
}