using UnityEngine.SceneManagement;

public static class BossCheckpointState
{
    public const string DefaultLevelScene = "Level_1";
    public const string TutorialScene = "Tutorial";

    public static bool IsBossCheckpoint { get; private set; }
    public static string BossDisplayName { get; private set; }
    public static string CheckpointScene { get; private set; } = DefaultLevelScene;

    public static void SetLevelCheckpoint()
    {
        IsBossCheckpoint = false;
        BossDisplayName = string.Empty;
        CheckpointScene = LevelManager.currentLevel == 0 ? TutorialScene : DefaultLevelScene;
    }

    public static void SetBossCheckpoint(string bossDisplayName, string bossSceneName)
    {
        IsBossCheckpoint = true;
        BossDisplayName = bossDisplayName;
        CheckpointScene = string.IsNullOrWhiteSpace(bossSceneName) ? SceneManager.GetActiveScene().name : bossSceneName;
    }

    public static void ApplyLoadedSave(SaveData data)
    {
        if (data == null)
        {
            SetLevelCheckpoint();
            return;
        }

        IsBossCheckpoint = data.isBossCheckpoint;
        BossDisplayName = data.bossDisplayName ?? string.Empty;
        CheckpointScene = ResolveSceneName(data);
    }

    public static string ResolveSceneName(SaveData data)
    {
        if (data == null)
        {
            return DefaultLevelScene;
        }

        if (!string.IsNullOrWhiteSpace(data.checkpointScene))
        {
            return data.checkpointScene;
        }

        return data.floor == 0 ? TutorialScene : DefaultLevelScene;
    }
}
