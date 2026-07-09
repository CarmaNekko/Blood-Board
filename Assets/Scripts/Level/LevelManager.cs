using BloodBoard.GameManagement;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static int currentLevel = 1;
    public static int currentEnemiesPerRoom;
    public static DungeonLayout CurrentLayout { get; private set; }
    public static event Action<DungeonLayout> LayoutGenerated;
    public static event Action<int> OnFloorAdvanced;

    [Header("Progression Settings")]
    [SerializeField] private int baseRooms = 6;
    [SerializeField] private int baseEnemiesPerRoom = 4;

    [Header("Difficulty Curve")]
    [SerializeField] private int extraRoomsPerLevel = 1;
    [SerializeField] private int extraEnemiesPerLevel = 1;

    [Header("Enemies (White & Black)")]
    public GameObject pawnWhite;
    public GameObject pawnBlack;
    public GameObject knightWhite;
    public GameObject knightBlack;
    public GameObject bishopWhite;
    public GameObject bishopBlack;
    public GameObject rookWhite;
    public GameObject rookBlack;

    private void Awake()
    {
        CurrentLayout = null;
        currentEnemiesPerRoom = baseEnemiesPerRoom + (extraEnemiesPerLevel * (currentLevel - 1));

        int roomsToGenerate = baseRooms + (extraRoomsPerLevel * (currentLevel - 1));

        Debug.Log($"Iniciando Nivel {currentLevel}. Salas: {roomsToGenerate}, Enemigos por sala: {currentEnemiesPerRoom}");

        ModularGenerator generator = UnityEngine.Object.FindFirstObjectByType<ModularGenerator>();

        if (generator != null)
        {
            CurrentLayout = generator.GenerateLevel(roomsToGenerate);
            RoomVisibilityManager.SetLayout(CurrentLayout);
            LayoutGenerated?.Invoke(CurrentLayout);
        }
        else
        {
            ProceduralTutorial tutorialGenerator = UnityEngine.Object.FindFirstObjectByType<ProceduralTutorial>();

            if (tutorialGenerator != null)
            {
                tutorialGenerator.GenerateTutorialLevel();
            }
            else
            {
                Debug.LogError("No se encontro ni ModularGenerator ni ProceduralTutorial en la escena.");
            }
        }
    }

    private void Start()
    {
        int currentScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetCurrentScore() : 0;

        if (SceneManager.GetActiveScene().name == BossCheckpointState.DefaultLevelScene)
        {
            SaveData data = SaveManager.LoadFromSlot(GameModeManager.CurrentSlot);
            if (data != null && data.score > 0)
            {
                ScoreManager.Instance?.SetCurrentScore(data.score);
            }
            SaveManager.SaveToSlot(GameModeManager.CurrentSlot, currentLevel, currentScore, FindFirstObjectByType<PlayerHealth>()?.currentHealth ?? 100f, GameModeManager.CurrentMode.GetModeName());
        }
    }

    public List<GameObject> GetAllowedEnemies()
    {
        List<GameObject> pool = new List<GameObject>();

        if (pawnWhite != null) pool.Add(pawnWhite);
        if (pawnBlack != null) pool.Add(pawnBlack);

        if (currentLevel >= 2)
        {
            if (knightWhite != null) pool.Add(knightWhite);
            if (knightBlack != null) pool.Add(knightBlack);
        }

        if (currentLevel >= 3)
        {
            if (bishopWhite != null) pool.Add(bishopWhite);
            if (bishopBlack != null) pool.Add(bishopBlack);
        }

        if (currentLevel >= 4)
        {
            if (rookWhite != null) pool.Add(rookWhite);
            if (rookBlack != null) pool.Add(rookBlack);
        }

        return pool;
    }

    public void AdvanceLevel()
    {
        currentLevel++;
        OnFloorAdvanced?.Invoke(currentLevel);
        CheckerboardTransition.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        CurrentLayout = null;
    }
}