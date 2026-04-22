using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static int currentLevel = 1;

    public static int currentEnemiesPerRoom;

    [Header("Progression Settings")]
    [SerializeField] private int baseRooms = 6;
    [SerializeField] private int baseEnemiesPerRoom = 4;

    [Header("Difficulty Curve")]
    [SerializeField] private int extraRoomsPerLevel = 1;
    [SerializeField] private int extraEnemiesPerLevel = 1;

    void Awake()
    {
        currentEnemiesPerRoom = baseEnemiesPerRoom + (extraEnemiesPerLevel * (currentLevel - 1));

        int roomsToGenerate = baseRooms + (extraRoomsPerLevel * (currentLevel - 1));

        Debug.Log($"Iniciando Nivel {currentLevel}. Salas: {roomsToGenerate}, Enemigos por sala: {currentEnemiesPerRoom}");

        ModularGenerator generator = Object.FindFirstObjectByType<ModularGenerator>();

        if (generator != null)
        {
            generator.GenerateLevel(roomsToGenerate);
        }
        else
        {
            Debug.LogError("No se encontró el ModularGenerator en la escena.");
        }
    }

    public void AdvanceLevel()
    {
        currentLevel++;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}