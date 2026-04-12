using System.Collections.Generic;
using UnityEngine;

public class RoomEnemySpawner : MonoBehaviour
{
    [Header("Enemy Setup")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private int enemiesToSpawn = 3;

    void Start()
    {
        Invoke("SpawnEnemies", 0.5f);
    }

    private void SpawnEnemies()
    {
        if (enemyPrefabs.Length == 0 || spawnPoints.Count == 0)
        {
            return;
        }

        List<Transform> availableSpawns = new List<Transform>(spawnPoints);

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            if (availableSpawns.Count == 0) break;

            int randomSpawnIndex = Random.Range(0, availableSpawns.Count);
            Transform chosenSpawn = availableSpawns[randomSpawnIndex];

            int randomEnemyIndex = Random.Range(0, enemyPrefabs.Length);
            GameObject chosenEnemyPrefab = enemyPrefabs[randomEnemyIndex];

            Instantiate(chosenEnemyPrefab, chosenSpawn.position, chosenSpawn.rotation);

            availableSpawns.RemoveAt(randomSpawnIndex);
        }
    }
}