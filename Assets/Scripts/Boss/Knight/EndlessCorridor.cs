using System.Collections.Generic;
using UnityEngine;

public class EndlessCorridor : MonoBehaviour
{
    [Header("Main Settings")]
    public Transform playerTransform;
    public float chunkLength = 30f;
    public int chunksOnScreen = 4;
    public float initialSpawnZ = 30f;

    [Header("Prefabs")]
    public GameObject[] chunkPrefabs;
    public GameObject finalRoomPrefab;

    [Header("Level Progress")]
    public int chunksToSpawnBeforeEnd = 15;

    public BossKnight bossKnight;

    private float spawnZ = 0f;
    private List<GameObject> activeChunks = new List<GameObject>();
    private CharacterController playerController;

    private int chunksSpawned = 0;
    private bool isEndRoomSpawned = false;

    void Start()
    {
        if (playerTransform != null)
        {
            playerController = playerTransform.GetComponent<CharacterController>();
        }

        spawnZ = initialSpawnZ;
        for (int i = 0; i < chunksOnScreen; i++)
        {
            SpawnChunk();
        }
    }

    void Update()
    {
        if (playerTransform == null || activeChunks.Count == 0) return;

        float oldestChunkEnd = activeChunks[0].transform.position.z + chunkLength;

        if (!isEndRoomSpawned && playerTransform.position.z > oldestChunkEnd + 5f)
        {
            SpawnChunk();
            DeleteOldestChunk();
        }

        if (playerTransform.position.z > 500f)
        {
            ResetWorld();
        }
    }

    private void ResetWorld()
    {
        float distanceToReset = 500f;
        playerController.enabled = false;
        playerTransform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, playerTransform.position.z - distanceToReset);
        playerController.enabled = true;

        foreach (GameObject chunk in activeChunks)
        {
            chunk.transform.position = new Vector3(chunk.transform.position.x, chunk.transform.position.y, chunk.transform.position.z - distanceToReset);
        }
        spawnZ -= distanceToReset;

        if (bossKnight != null)
        {
            bossKnight.ResetBossPosition(distanceToReset);
        }
    }

    private void SpawnChunk()
    {
        if (isEndRoomSpawned) return;

        GameObject chunkToSpawn;

        if (chunksSpawned < chunksToSpawnBeforeEnd)
        {
            int randomIndex = Random.Range(0, chunkPrefabs.Length);
            chunkToSpawn = chunkPrefabs[randomIndex];
            chunksSpawned++;
        }
        else
        {
            chunkToSpawn = finalRoomPrefab;
            isEndRoomSpawned = true;

            if (bossKnight != null)
            {
                bossKnight.SetFinalDoor(spawnZ);
            }
        }

        GameObject newChunk = Instantiate(chunkToSpawn, new Vector3(0, 0, spawnZ), Quaternion.identity);
        newChunk.transform.SetParent(transform);
        activeChunks.Add(newChunk);

        spawnZ += chunkLength;
    }

    private void DeleteOldestChunk()
    {
        Destroy(activeChunks[0]);
        activeChunks.RemoveAt(0);
    }
}