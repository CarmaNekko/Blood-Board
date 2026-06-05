using BloodBoard.GameManagement;
using UnityEngine;

[System.Serializable]
public class BossCheckpointRoute
{
    public int originFloor = 1;
    public string bossDisplayName = "Caballo Campeon";
    public string bossSceneName = "Knigh_Boss";
}

public class Finish_Level : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    [Header("Escenas de Destino")]
    [Tooltip("El nombre exacto de la escena de tu Nivel Procedural base")]
    [SerializeField] private string escenaProcedural = "Level_1";

    [Tooltip("El nombre exacto de tu escena del Menú Principal (Parche Final)")]
    [SerializeField] private string menuSceneName = "MainMenu";

    [Tooltip("Bosses que aparecen al terminar cada piso. El piso guardado sigue siendo el de origen.")]
    [SerializeField]
    private BossCheckpointRoute[] bossRoutes =
    {
        new BossCheckpointRoute { originFloor = 1, bossDisplayName = "Peon Campeon", bossSceneName = "Pawn_Boss" },
        new BossCheckpointRoute { originFloor = 2, bossDisplayName = "Caballo Campeon", bossSceneName = "Knigh_Boss" },
        new BossCheckpointRoute { originFloor = 3, bossDisplayName = "Alfil Campeon", bossSceneName = "Bishop_Boss" },
        new BossCheckpointRoute { originFloor = 4, bossDisplayName = "Torre Campeona", bossSceneName = "Rook_Boss" },
        new BossCheckpointRoute { originFloor = 5, bossDisplayName = "Rey y Reina", bossSceneName = "KingQueen_Boss" }
    };

    private bool isLoading;

    private void OnTriggerEnter(Collider other)
    {
        if (isLoading || !other.CompareTag(playerTag))
        {
            return;
        }

        isLoading = true;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        float health = playerHealth != null ? playerHealth.currentHealth : 100f;
        int currentScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetCurrentScore() : 0;
        string currentModeName = GameModeManager.CurrentMode != null ? GameModeManager.CurrentMode.GetModeName() : "Modo_Historia";

        // Elimar esta cosa cuando tengamos el nivel del jefe final listo :,u
        if (LevelManager.currentLevel >= 5)
        {
            Debug.Log("Fin de la demo. Regresando al menú para evitar el crash.");
            CheckerboardTransition.LoadScene(menuSceneName);
            return;
        }
        //

        if (TryGetBossRoute(LevelManager.currentLevel, out BossCheckpointRoute bossRoute))
        {
            SaveManager.SaveBossCheckpointToSlot(
                GameModeManager.CurrentSlot,
                LevelManager.currentLevel,
                currentScore,
                health,
                currentModeName,
                bossRoute.bossSceneName,
                bossRoute.bossDisplayName);

            Debug.Log($"Piso {LevelManager.currentLevel} completado. Guardando checkpoint de boss: {bossRoute.bossDisplayName}. Cargando: {bossRoute.bossSceneName}");
            CheckerboardTransition.LoadScene(bossRoute.bossSceneName);
            return;
        }

        if (GameModeManager.CurrentMode != null && GameModeManager.CurrentMode.IsFinalFloor(LevelManager.currentLevel))
        {
            Debug.Log($"Piso final ({LevelManager.currentLevel}) alcanzado en modo {GameModeManager.CurrentMode.GetModeName()}. Fin del juego.");
            GameOver gameOverScreen = Object.FindFirstObjectByType<GameOver>();
            if (gameOverScreen != null) gameOverScreen.ShowGameOver(true);
            return;
        }

        LevelManager.currentLevel++;
        SaveManager.SaveToSlot(GameModeManager.CurrentSlot, LevelManager.currentLevel, currentScore, health, currentModeName);

        Debug.Log($"Piso completado. Guardando y avanzando al piso {LevelManager.currentLevel}. Cargando: {escenaProcedural}");
        CheckerboardTransition.LoadScene(escenaProcedural);
    }

    private bool TryGetBossRoute(int originFloor, out BossCheckpointRoute route)
    {
        if (bossRoutes != null)
        {
            for (int i = 0; i < bossRoutes.Length; i++)
            {
                BossCheckpointRoute candidate = bossRoutes[i];
                if (candidate != null && candidate.originFloor == originFloor && !string.IsNullOrWhiteSpace(candidate.bossSceneName))
                {
                    route = candidate;
                    return true;
                }
            }
        }

        route = null;
        return false;
    }
}