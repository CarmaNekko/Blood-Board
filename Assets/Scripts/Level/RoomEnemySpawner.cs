using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoomEnemySpawner : MonoBehaviour
{
    private TextMeshProUGUI waveText;

    [Header("Wave Setup")]
    [SerializeField] private int idealEnemiesPerWave = 3;
    [SerializeField] private List<Transform> spawnPoints;
    private int totalEnemiesToSpawn = 3;
    private int maxWaves;
    private int currentWave = 0;

    [Header("Composition Rules")]
    [SerializeField] private int maxKnights = 2;
    [SerializeField] private int maxBishops = 1;
    [SerializeField] private int maxRooks = 1;

    private int currentKnights = 0;
    private int currentBishops = 0;
    private int currentRooks = 0;

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

    public IReadOnlyList<Transform> SpawnPoints => spawnPoints;
    public LayerMask GroundMask => groundMask;

    private void Start()
    {
        GameObject uiTextObj = GameObject.Find("WaveCounterText");
        if (uiTextObj != null)
        {
            waveText = uiTextObj.GetComponent<TextMeshProUGUI>();
            waveText.text = "";
        }
    }

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

    private void StartNextWave()
    {
        if (currentWave >= maxWaves)
        {
            roomCleared = true;
            LockAllDoors(false);
            UpdateUI();
            return;
        }

        currentWave++;
        int enemiesThisWave = Mathf.CeilToInt((float)totalEnemiesToSpawn / maxWaves);

        if (enemiesThisWave > spawnPoints.Count) enemiesThisWave = spawnPoints.Count;

        StartCoroutine(SpawnEnemiesRoutine(enemiesThisWave));
    }

    private IEnumerator SpawnEnemiesRoutine(int enemiesToSpawnThisWave)
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

        for (int i = 0; i < enemiesToSpawnThisWave; i++)
        {
            if (availableSpawns.Count == 0) break;

            int randomSpawnIndex = Random.Range(0, availableSpawns.Count);
            Transform chosenSpawn = availableSpawns[randomSpawnIndex];
            chosenSpawns.Add(chosenSpawn);
            availableSpawns.RemoveAt(randomSpawnIndex);

            if (warningVisualPrefab != null)
            {
                GameObject warning = Instantiate(warningVisualPrefab, chosenSpawn.position, Quaternion.identity);
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
            if (currentKnights >= maxKnights) pool.RemoveAll(e => e.GetComponent<KnightAttack>() != null);
            if (currentBishops >= maxBishops) pool.RemoveAll(e => e.GetComponent<BishopAttack>() != null);
            if (currentRooks >= maxRooks) pool.RemoveAll(e => e.GetComponent<RookProtector>() != null);

            if (pool.Count == 0) break;

            GameObject chosenEnemyPrefab = pool[Random.Range(0, pool.Count)];

            if (chosenEnemyPrefab.GetComponent<KnightAttack>() != null) currentKnights++;
            else if (chosenEnemyPrefab.GetComponent<BishopAttack>() != null) currentBishops++;
            else if (chosenEnemyPrefab.GetComponent<RookProtector>() != null) currentRooks++;

            Vector3 spawnPos = spawnLocation.position;
            GameObject spawnedEnemy = Instantiate(chosenEnemyPrefab, spawnPos, spawnLocation.rotation);

            if (spawnedEnemy.GetComponent<EnemyGlow>() == null)
            {
                spawnedEnemy.AddComponent<EnemyGlow>();
            }

            activeEnemiesList.Add(spawnedEnemy);
        }

        isSpawning = false;
        UpdateUI();
    }

    public IEnumerator SpawnExternalEnemiesRoutine(
        int enemiesToSpawn,
        List<GameObject> enemyPool,
        bool buffEnemies,
        List<GameObject> spawnedEnemies)
    {
        if (spawnedEnemies != null)
        {
            spawnedEnemies.Clear();
        }

        if (enemyPool == null || enemyPool.Count == 0 || spawnPoints == null || spawnPoints.Count == 0)
        {
            yield break;
        }

        List<GameObject> pool = new List<GameObject>(enemyPool);
        List<Transform> availableSpawns = new List<Transform>(spawnPoints);
        List<Transform> chosenSpawns = new List<Transform>();
        List<GameObject> activeWarnings = new List<GameObject>();

        int spawnCount = Mathf.Min(enemiesToSpawn, availableSpawns.Count);
        for (int i = 0; i < spawnCount; i++)
        {
            if (availableSpawns.Count == 0) break;

            int randomSpawnIndex = Random.Range(0, availableSpawns.Count);
            Transform chosenSpawn = availableSpawns[randomSpawnIndex];
            chosenSpawns.Add(chosenSpawn);
            availableSpawns.RemoveAt(randomSpawnIndex);

            if (warningVisualPrefab != null)
            {
                Vector3 visualPosition = chosenSpawn.position + new Vector3(0f, 0.05f, 0f);
                GameObject warning = Instantiate(warningVisualPrefab, visualPosition, Quaternion.identity);
                activeWarnings.Add(warning);
            }
        }

        yield return new WaitForSeconds(spawnDelay);

        foreach (GameObject warning in activeWarnings)
        {
            if (warning != null) Destroy(warning);
        }

        int spawnedKnights = 0;
        int spawnedBishops = 0;
        int spawnedRooks = 0;

        foreach (Transform spawnLocation in chosenSpawns)
        {
            if (spawnedKnights >= maxKnights) pool.RemoveAll(e => e.GetComponent<KnightAttack>() != null);
            if (spawnedBishops >= maxBishops) pool.RemoveAll(e => e.GetComponent<BishopAttack>() != null);
            if (spawnedRooks >= maxRooks) pool.RemoveAll(e => e.GetComponent<RookProtector>() != null);

            if (pool.Count == 0) break;

            GameObject chosenEnemyPrefab = pool[Random.Range(0, pool.Count)];

            if (chosenEnemyPrefab.GetComponent<KnightAttack>() != null) spawnedKnights++;
            else if (chosenEnemyPrefab.GetComponent<BishopAttack>() != null) spawnedBishops++;
            else if (chosenEnemyPrefab.GetComponent<RookProtector>() != null) spawnedRooks++;

            Vector3 spawnPos = spawnLocation.position;
            GameObject spawnedEnemy = Instantiate(chosenEnemyPrefab, spawnPos, spawnLocation.rotation);

            EnemyHealth enemyHealth = spawnedEnemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null && buffEnemies)
            {
                enemyHealth.TakeDamage(0, enemyHealth.myColor);
            }

            if (spawnedEnemy.GetComponent<EnemyGlow>() == null)
            {
                spawnedEnemy.AddComponent<EnemyGlow>();
            }

            spawnedEnemies?.Add(spawnedEnemy);
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
                    {
                        totalEnemiesToSpawn = LevelManager.currentEnemiesPerRoom;
                    }
                    else
                    {
                        totalEnemiesToSpawn = 4;
                    }

                    maxWaves = Mathf.CeilToInt((float)totalEnemiesToSpawn / idealEnemiesPerWave);
                    if (maxWaves < 1) maxWaves = 1;

                    StartNextWave();
                }
            }
        }

        if (hasTriggered && !roomCleared && !isWaitingForPlayer && !isSpawning)
        {
            int previousCount = activeEnemiesList.Count;
            activeEnemiesList.RemoveAll(enemy => enemy == null);

            if (activeEnemiesList.Count != previousCount)
            {
                UpdateUI();
            }

            if (activeEnemiesList.Count == 0)
            {
                StartNextWave();
            }
        }
    }

    private void UpdateUI()
    {
        if (waveText == null) return;

        if (roomCleared)
        {
            waveText.text = "";
        }
        else if (hasTriggered)
        {
            waveText.text = $"OLEADA {currentWave}/{maxWaves} - ENEMIGOS: {activeEnemiesList.Count}";
        }
        else
        {
            waveText.text = "";
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

    public void SetDoorsLockedForExternalEvent(bool lockState)
    {
        LockAllDoors(lockState);
    }

    public void DebugClearWaves()
    {
        StopAllCoroutines();

        foreach (GameObject enemy in activeEnemiesList)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }

        activeEnemiesList.Clear();
        isWaitingForPlayer = false;
        isSpawning = false;
        hasTriggered = true;
        roomCleared = true;
        LockAllDoors(false);
        UpdateUI();
    }
}