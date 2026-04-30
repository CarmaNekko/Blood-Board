using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    [SerializeField] private int score; // Asigna score actual

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

            Debug.Log("Guardando en piso: " + currentFloor);
            SaveManager.SaveToSlot(GameModeManager.CurrentSlot, currentFloor, score, playerHealth, GameModeManager.CurrentMode.GetModeName());

            if (GameModeManager.CurrentMode.IsBossFloor(currentFloor))
            {
                SceneManager.LoadScene("TitleScreen"); // Fin de juego en jefe
            }
            else
            {
                LevelManager manager = Object.FindFirstObjectByType<LevelManager>();
                if (manager != null)
                {
                    manager.AdvanceLevel();
                }
            }
        }
    }
}