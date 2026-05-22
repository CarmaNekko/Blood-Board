using BloodBoard.GameManagement;
using UnityEngine;

public class Boss_Exit : MonoBehaviour
{
    public string mainLevelScene = "Level_1";
    public bool advanceLevel = true;

    private bool isLoading = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isLoading || !other.CompareTag("Player"))
        {
            return;
        }

        isLoading = true;

        bool isFinalFloor = false;
        string currentModeName = "Debug_Mode";

        if (GameModeManager.CurrentMode != null)
        {
            isFinalFloor = GameModeManager.CurrentMode.IsFinalFloor(LevelManager.currentLevel);
            currentModeName = GameModeManager.CurrentMode.GetModeName();
        }

        if (isFinalFloor)
        {
            var gameOverScreen = Object.FindFirstObjectByType<GameOver>();
            if (gameOverScreen != null) gameOverScreen.ShowGameOver(true);
        }
        else
        {
            if (advanceLevel)
            {
                LevelManager.currentLevel++;
            }

            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            float health = playerHealth != null ? playerHealth.currentHealth : 100f;
            int currentScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetCurrentScore() : 0;

            SaveManager.SaveToSlot(GameModeManager.CurrentSlot, LevelManager.currentLevel, currentScore, health, currentModeName);

            CheckerboardTransition.LoadScene(mainLevelScene);
        }
    }
}