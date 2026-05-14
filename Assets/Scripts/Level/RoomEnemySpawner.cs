using System.Collections.Generic;
using UnityEngine;

public class RoomEnemySpawner : MonoBehaviour
{
    [Header("Enemy Setup")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private int enemiesToSpawn = 3;

    private bool roomCleared = false;
    private bool hasTriggered = false;

    private bool isWaitingForPlayer = false;
    private Transform playerTransform;

    private RoomInstance myRoom;
    private DoorConnector[] myDoors;

    private List<GameObject> activeEnemiesList = new List<GameObject>();

    public void TriggerRoomEvent()
    {
        if (hasTriggered || roomCleared || isWaitingForPlayer) return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            isWaitingForPlayer = true;
        }
    }

    private void SpawnEnemies()
    {
        if (enemyPrefabs.Length == 0 || spawnPoints.Count == 0) return;

        List<Transform> availableSpawns = new List<Transform>(spawnPoints);

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            if (availableSpawns.Count == 0) break;

            int randomSpawnIndex = Random.Range(0, availableSpawns.Count);
            Transform chosenSpawn = availableSpawns[randomSpawnIndex];

            int randomEnemyIndex = Random.Range(0, enemyPrefabs.Length);
            GameObject chosenEnemyPrefab = enemyPrefabs[randomEnemyIndex];

            GameObject spawnedEnemy = Instantiate(chosenEnemyPrefab, chosenSpawn.position, chosenSpawn.rotation);
            activeEnemiesList.Add(spawnedEnemy);

            availableSpawns.RemoveAt(randomSpawnIndex);
        }
    }

    private void Update()
    {
        if (isWaitingForPlayer && playerTransform != null)
        {
            if (myRoom == null) myRoom = GetComponent<RoomInstance>();
            if (myDoors == null || myDoors.Length == 0) myDoors = GetComponentsInChildren<DoorConnector>();

            if (myRoom != null && myRoom.IsCurrentArea)
            {
                bool isFarFromAllDoors = true;

                foreach (DoorConnector door in myDoors)
                {
                    if (door.isConnected)
                    {
                        Vector2 playerPos2D = new Vector2(playerTransform.position.x, playerTransform.position.z);
                        Vector2 doorPos2D = new Vector2(door.transform.position.x, door.transform.position.z);

                        if (Vector2.Distance(playerPos2D, doorPos2D) < 7f)
                        {
                            isFarFromAllDoors = false;
                            break;
                        }
                    }
                }

                if (isFarFromAllDoors)
                {
                    isWaitingForPlayer = false;
                    hasTriggered = true;

                    LockAllDoors(true);

                    if (LevelManager.currentEnemiesPerRoom > 0)
                        enemiesToSpawn = LevelManager.currentEnemiesPerRoom;
                    if (enemiesToSpawn > spawnPoints.Count)
                        enemiesToSpawn = spawnPoints.Count;

                    SpawnEnemies();
                }
            }
        }

        if (hasTriggered && !roomCleared && !isWaitingForPlayer)
        {
            activeEnemiesList.RemoveAll(enemy => enemy == null);

            if (activeEnemiesList.Count == 0)
            {
                roomCleared = true;
                LockAllDoors(false);
                Debug.Log("¡Habitación limpiada!");
            }
        }
    }

    private void LockAllDoors(bool lockState)
    {
        if (myDoors == null) myDoors = GetComponentsInChildren<DoorConnector>();

        foreach (DoorConnector door in myDoors)
        {
            if (door.isConnected)
            {
                door.SetLock(lockState);
            }
        }
    }
}