using BloodBoard.GameManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSkipTool : MonoBehaviour
{
    public string normalLevelScene = "Level_1";
    public string pawnScene = "Pawn_Boss";
    public string knightScene = "Knigh_Boss";
    public string bishopScene = "Bishop_Boss";
    public string rookScene = "Rook_Boss";
    public string kingQueenScene = "KingQueen_Boss";

    private bool godModeActive = false;

    private class BossCheckpointRoute
    {
        public int originFloor;
        public string bossDisplayName;
        public string bossSceneName;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            LevelManager manager = Object.FindFirstObjectByType<LevelManager>();
            if (manager != null)
            {
                manager.AdvanceLevel();
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha0)) SyncDebugSceneLoad(normalLevelScene, 1, false);
        if (Input.GetKeyDown(KeyCode.Alpha1)) SyncDebugBossSceneForFloor(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SyncDebugBossSceneForFloor(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SyncDebugBossSceneForFloor(3);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SyncDebugBossSceneForFloor(4);
        if (Input.GetKeyDown(KeyCode.Alpha5)) SyncDebugBossSceneForFloor(5);

        if (Input.GetKeyDown(KeyCode.O))
        {
            ApplyGodModeStats();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            godModeActive = !godModeActive;
            Debug.Log("Modo OP: " + godModeActive);
        }

        if (godModeActive)
        {
            ApplyGodModeStats();
        }
    }

    private void SyncDebugSceneLoad(string sceneName, int floor, bool isBoss, string bossDisplayName = "", string bossSceneName = "")
    {
        LevelManager.currentLevel = floor;

        PlayerHealth playerHealth = Object.FindFirstObjectByType<PlayerHealth>();
        float health = playerHealth != null ? playerHealth.currentHealth : 100f;
        int currentScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetCurrentScore() : 0;
        string currentModeName = GameModeManager.CurrentMode != null ? GameModeManager.CurrentMode.GetModeName() : "Debug_Mode";

        if (isBoss)
        {
            SaveManager.SaveBossCheckpointToSlot(
                GameModeManager.CurrentSlot,
                floor,
                currentScore,
                health,
                currentModeName,
                bossSceneName,
                bossDisplayName);
        }
        else
        {
            SaveManager.SaveToSlot(GameModeManager.CurrentSlot, floor, currentScore, health, currentModeName);
        }

        CheckerboardTransition.LoadScene(sceneName);
    }

    private void SyncDebugBossSceneForFloor(int floor)
    {
        var route = GetBossRouteForFloor(floor);
        if (route == null)
        {
            Debug.LogWarning($"No existe ruta de boss para el piso {floor}.");
            return;
        }

        SyncDebugSceneLoad(route.bossSceneName, floor, true, route.bossDisplayName, route.bossSceneName);
    }

    private BossCheckpointRoute GetBossRouteForFloor(int floor)
    {
        switch (floor)
        {
            case 1: return new BossCheckpointRoute { originFloor = 1, bossDisplayName = "Peon Campeon", bossSceneName = pawnScene };
            case 2: return new BossCheckpointRoute { originFloor = 2, bossDisplayName = "Caballo Campeon", bossSceneName = knightScene };
            case 3: return new BossCheckpointRoute { originFloor = 3, bossDisplayName = "Alfil Campeon", bossSceneName = bishopScene };
            case 4: return new BossCheckpointRoute { originFloor = 4, bossDisplayName = "Torre Campeona", bossSceneName = rookScene };
            case 5: return new BossCheckpointRoute { originFloor = 5, bossDisplayName = "Rey y Reina", bossSceneName = kingQueenScene };
        }
        return null;
    }

    private void ApplyGodModeStats()
    {
        PlayerHealth playerHealth = Object.FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.RestoreHealth(9999f);
        }

        MagicShooter playerWeapon = Object.FindFirstObjectByType<MagicShooter>();
        if (playerWeapon != null)
        {
            playerWeapon.RefillManaToMax();
        }
    }
}