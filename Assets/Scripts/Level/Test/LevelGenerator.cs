using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Room Prefabs")]
    [SerializeField] private GameObject startRoomPrefab;
    [SerializeField] private List<GameObject> randomRoomPrefabs;
    [SerializeField] private GameObject finalRoomPrefab;

    [Header("Settings")]
    [SerializeField] private int roomsToGenerate = 5;
    [SerializeField] private NavMeshSurface navMeshSurface;

    private Transform currentExitPoint;

    void Start()
    {
        GenerateLevel();
    }

    private void GenerateLevel()
    {
        GameObject startRoom = Instantiate(startRoomPrefab, Vector3.zero, Quaternion.identity);
        currentExitPoint = FindExitPoint(startRoom);

        for (int i = 0; i < roomsToGenerate; i++)
        {
            int randomIndex = Random.Range(0, randomRoomPrefabs.Count);
            GameObject selectedRoom = randomRoomPrefabs[randomIndex];
            GameObject spawnedRoom = Instantiate(selectedRoom, currentExitPoint.position, currentExitPoint.rotation);
            currentExitPoint = FindExitPoint(spawnedRoom);
        }

        Instantiate(finalRoomPrefab, currentExitPoint.position, currentExitPoint.rotation);

        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
        }
    }

    private Transform FindExitPoint(GameObject room)
    {
        Transform exit = room.transform.Find("ExitPoint");
        return exit;
    }
}