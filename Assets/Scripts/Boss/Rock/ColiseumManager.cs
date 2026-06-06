using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ColiseumManager : MonoBehaviour
{
    [Header("Referencias Principales")]
    [SerializeField] private RookBossController rookBoss;
    [SerializeField] private GameObject exitGate;
    [SerializeField] private GameObject entranceGate;

    [Header("UI de Eventos")]
    [SerializeField] private TextMeshProUGUI instructionText;

    [Header("Evento 1: Caballos (Supervivencia)")]
    [SerializeField] private GameObject[] knightPrefabs;
    [SerializeField] private Transform[] knightSpawnPoints;
    [SerializeField] private float survivalDuration = 15f;

    [Header("Evento 2: Peones (Arena)")]
    [SerializeField] private GameObject[] pawnPrefabs;
    [SerializeField] private Transform[] arenaSpawnPoints;

    [Header("Evento 3: Alfiles (Gradas)")]
    [SerializeField] private GameObject[] bishopPrefabs;
    [SerializeField] private Transform[] standsSpawnPoints;

    [Header("Curaciones (Recompensas)")]
    [SerializeField] private GameObject healthPrefab;
    [SerializeField] private int healthDropsPerEvent = 2;

    [Header("Tiempos del Coliseo")]
    [SerializeField] private float vulnerabilityDuration = 8f;
    [SerializeField] private float pauseBetweenEvents = 2f;

    private int lastEventIndex = -1;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool battleStarted = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !battleStarted)
        {
            battleStarted = true;
            if (entranceGate != null) entranceGate.SetActive(true);
            if (exitGate != null) exitGate.SetActive(true);

            if (instructionText != null) instructionText.text = "¡JEFE DEL COLISEO ENTRA AL COMBATE!";
            rookBoss.Initialize(this);
        }
    }

    public void StartEvents()
    {
        StartCoroutine(ColiseumLoop());
    }

    private IEnumerator ColiseumLoop()
    {
        yield return new WaitForSeconds(pauseBetweenEvents);

        while (!rookBoss.isDead)
        {
            int nextEvent = Random.Range(0, 3);
            while (nextEvent == lastEventIndex)
            {
                nextEvent = Random.Range(0, 3);
            }
            lastEventIndex = nextEvent;

            yield return StartCoroutine(PlayEvent(nextEvent));

            if (rookBoss.isDead) break;

            SpawnHealth();

            if (instructionText != null) instructionText.text = "¡ESCUDOS ABAJO! ¡ATACA AL JEFE!";
            rookBoss.SetShield(false);
            yield return new WaitForSeconds(vulnerabilityDuration);

            if (!rookBoss.isDead)
            {
                rookBoss.SetShield(true);
                yield return new WaitForSeconds(pauseBetweenEvents);
            }
        }
    }

    private IEnumerator PlayEvent(int eventIndex)
    {
        activeEnemies.Clear();

        if (eventIndex == 0)
        {
            SpawnEnemies(knightPrefabs, knightSpawnPoints);
            float timeLeft = survivalDuration;

            while (timeLeft > 0)
            {
                if (instructionText != null)
                {
                    instructionText.text = $"¡SOBREVIVE A LA ESTAMPIDA INVULNERABLE! ({Mathf.CeilToInt(timeLeft)}s)";
                }
                timeLeft -= Time.deltaTime;
                yield return null;
            }

            ClearEnemies();
        }
        else if (eventIndex == 1)
        {
            if (instructionText != null) instructionText.text = "¡ELIMINA A TUS ENEMIGOS!";
            SpawnEnemies(pawnPrefabs, arenaSpawnPoints);
            yield return new WaitUntil(() => AllEnemiesDead());
        }
        else if (eventIndex == 2)
        {
            if (instructionText != null) instructionText.text = "¡ACABA CON EL PUBLICO DESCONTENTO!";
            SpawnEnemies(bishopPrefabs, standsSpawnPoints);
            yield return new WaitUntil(() => AllEnemiesDead());
        }
    }

    private void SpawnEnemies(GameObject[] prefabs, Transform[] points)
    {
        if (prefabs == null || prefabs.Length == 0) return;

        foreach (Transform spawnPoint in points)
        {
            if (spawnPoint != null)
            {
                GameObject randomColorPrefab = prefabs[Random.Range(0, prefabs.Length)];

                if (randomColorPrefab != null)
                {
                    GameObject enemy = Instantiate(randomColorPrefab, spawnPoint.position, spawnPoint.rotation);
                    
                    // Agregar componente EnemyGlow para brillo sutil
                    if (enemy.GetComponent<EnemyGlow>() == null)
                    {
                        enemy.AddComponent<EnemyGlow>();
                    }
                    
                    activeEnemies.Add(enemy);
                }
            }
        }
    }

    private void SpawnHealth()
    {
        if (healthPrefab == null || arenaSpawnPoints.Length == 0) return;

        for (int i = 0; i < healthDropsPerEvent; i++)
        {
            Transform randomPoint = arenaSpawnPoints[Random.Range(0, arenaSpawnPoints.Length)];
            Instantiate(healthPrefab, randomPoint.position, randomPoint.rotation);
        }
    }

    private bool AllEnemiesDead()
    {
        activeEnemies.RemoveAll(item => item == null);
        return activeEnemies.Count == 0;
    }

    private void ClearEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null) Destroy(enemy);
        }
        activeEnemies.Clear();
    }

    public void BossDefeated()
    {
        StopAllCoroutines();
        ClearEnemies();
        if (instructionText != null) instructionText.text = "¡COLISEO SUPERADO!";
        if (exitGate != null) exitGate.SetActive(false);
    }
}