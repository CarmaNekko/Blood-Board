using System;
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
            LayoutGenerated?.Invoke(CurrentLayout);

            // Save checkpoint at floor start
            float initialHealth = FindFirstObjectByType<PlayerHealth>()?.maxHealth ?? 100f;
            SaveManager.SaveToSlot(GameModeManager.CurrentSlot, currentLevel, 0, initialHealth, GameModeManager.CurrentMode.GetModeName()); // Score 0, full health
            Debug.Log("Guardado checkpoint al iniciar piso: " + currentLevel);
        }
        else
        {
            Debug.LogError("No se encontro el ModularGenerator en la escena.");
        }
    }

    public void AdvanceLevel()
    {
        currentLevel++;
        OnFloorAdvanced?.Invoke(currentLevel);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnDestroy()
    {
        CurrentLayout = null;
    }
}
