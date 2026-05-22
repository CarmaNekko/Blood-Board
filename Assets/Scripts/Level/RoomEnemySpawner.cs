using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomEnemySpawner : MonoBehaviour
{
    [Header("Enemy Setup")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private int enemiesToSpawn = 3;

    [Header("Spawn Delay & Visuals")]
    [SerializeField] private GameObject warningVisualPrefab;
    [SerializeField] private float spawnDelay = 1.5f;
    public LayerMask groundMask = Physics.AllLayers;

    private bool roomCleared = false;
    private bool hasTriggered = false;
    private bool isWaitingForPlayer = false;
    private bool isSpawning = false;
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
        StartCoroutine(SpawnEnemiesRoutine());
    }

    private IEnumerator SpawnEnemiesRoutine()
    {
        isSpawning = true;

        LevelManager levelManager = Object.FindFirstObjectByType<LevelManager>();
        List<GameObject> pool = levelManager != null ? levelManager.GetAllowedEnemies() : new List<GameObject>();

        if (pool.Count == 0 || spawnPoints.Count == 0)
        {
            isSpawning = false;
            yield break;
        }

        List<Transform> availableSpawns = new List<Transform>(spawnPoints);
        List<Transform> chosenSpawns = new List<Transform>();
        List<GameObject> activeWarnings = new List<GameObject>();

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            if (availableSpawns.Count == 0) break;

            int randomSpawnIndex = Random.Range(0, availableSpawns.Count);
            Transform chosenSpawn = availableSpawns[randomSpawnIndex];
            chosenSpawns.Add(chosenSpawn);
            availableSpawns.RemoveAt(randomSpawnIndex);

            if (warningVisualPrefab != null)
            {
                Vector3 visualPosition = chosenSpawn.position;
                if (Physics.Raycast(chosenSpawn.position, Vector3.down, out RaycastHit hit, 50f, groundMask))
                {
                    visualPosition = hit.point + new Vector3(0, 0.05f, 0);
                }

                GameObject warning = Instantiate(warningVisualPrefab, visualPosition, Quaternion.identity);
                activeWarnings.Add(warning);
            }
        }

        yield return new WaitForSeconds(spawnDelay);

        foreach (GameObject warning in activeWarnings)
        {
            if (warning != null) Destroy(warning);
        }

        foreach (Transform spawnLocation in chosenSpawns)
        {
            GameObject chosenEnemyPrefab = pool[Random.Range(0, pool.Count)];

            Vector3 spawnPos = spawnLocation.position;
            if (Physics.Raycast(spawnLocation.position, Vector3.down, out RaycastHit hitEnemy, 50f, groundMask))
            {
                spawnPos = hitEnemy.point;
            }

            GameObject spawnedEnemy = Instantiate(chosenEnemyPrefab, spawnPos, spawnLocation.rotation);
            activeEnemiesList.Add(spawnedEnemy);
        }

        isSpawning = false;
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

        if (hasTriggered && !roomCleared && !isWaitingForPlayer && !isSpawning)
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