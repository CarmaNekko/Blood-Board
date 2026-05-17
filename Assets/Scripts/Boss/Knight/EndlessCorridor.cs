using System.Collections.Generic;
using UnityEngine;

public class EndlessCorridor : MonoBehaviour
{
    [Header("Configuración Principal")]
    public Transform playerTransform;
    public float chunkLength = 30f;
    public int chunksOnScreen = 4;

    [Header("Los Prefabs")]
    public GameObject[] chunkPrefabs;
    [Tooltip("Arrastra aquí el Prefab de tu Habitación Final")]
    public GameObject finalRoomPrefab;

    [Header("Progreso del Nivel")]
    [Tooltip("¿Cuántos pasillos debe cruzar antes de la meta?")]
    public int chunksToSpawnBeforeEnd = 15;

    private float spawnZ = 0f;
    private List<GameObject> activeChunks = new List<GameObject>();
    private bool isRunning = false;
    private CharacterController playerController;

    private int chunksSpawned = 0;
    private bool isEndRoomSpawned = false;

    void Start()
    {
        if (playerTransform != null)
        {
            playerController = playerTransform.GetComponent<CharacterController>();
        }
    }

    public void IniciarPasillo(float posicionInicioZ)
    {
        spawnZ = posicionInicioZ;
        for (int i = 0; i < chunksOnScreen; i++)
        {
            SpawnChunk();
        }
        isRunning = true;
    }

    void Update()
    {
        if (!isRunning || playerTransform == null || activeChunks.Count == 0) return;

        float finDelBloqueMasViejo = activeChunks[0].transform.position.z + chunkLength;

        if (!isEndRoomSpawned && playerTransform.position.z > finDelBloqueMasViejo + 5f)
        {
            SpawnChunk();
            DeleteOldestChunk();
        }

        if (playerTransform.position.z > 500f)
        {
            ResetearMundo();
        }
    }

    private void ResetearMundo()
    {
        float distanciaARetroceder = 500f;
        playerController.enabled = false;
        playerTransform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, playerTransform.position.z - distanciaARetroceder);
        playerController.enabled = true;

        foreach (GameObject chunk in activeChunks)
        {
            chunk.transform.position = new Vector3(chunk.transform.position.x, chunk.transform.position.y, chunk.transform.position.z - distanciaARetroceder);
        }
        spawnZ -= distanciaARetroceder;
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