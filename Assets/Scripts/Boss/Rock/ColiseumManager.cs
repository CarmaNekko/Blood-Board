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

    [Header("Evento 1: Caballos (El Muro de la Muerte)")]
    [SerializeField] private GameObject[] knightPrefabs;
    [SerializeField] private Transform arenaCenter;
    [SerializeField] private float arenaSize = 50f;
    [SerializeField] private int horsesPerWave = 12;
    [SerializeField] private float stampedeSpeed = 15f;
    [SerializeField] private int stampedeWaves = 4;
    [SerializeField] private float horseYOffset = 0f;

    [Header("Settings Visuales de Galope")]
    [SerializeField] private float horseJumpHeight = 2.5f;
    [SerializeField] private float horseJumpSpeed = 10f;

    [Header("Evento 2: Peones (Arena)")]
    [SerializeField] private GameObject[] pawnPrefabs;
    [SerializeField] private Transform[] arenaSpawnPoints;

    [Header("Fase de Curación: Alfiles (Gradas)")]
    [SerializeField] private GameObject[] bishopPrefabs;
    [SerializeField] private Transform[] standsSpawnPoints;

    [Header("Curaciones (Recompensas)")]
    [SerializeField] private GameObject healthPrefab;
    [SerializeField] private int healthDropsPerEvent = 2;

    [Header("Tiempos y Balance del Coliseo")]
    [SerializeField] private float vulnerabilityDuration = 8f;
    [SerializeField] private float pauseBetweenEvents = 2f;
    [SerializeField] private float shieldDamagePerEvent = 50f;
    [SerializeField] private float shieldHealPerBishop = 0.5f;

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
            while (!rookBoss.IsShieldBroken() && !rookBoss.isDead)
            {
                int nextEvent = Random.Range(0, 2);
                while (nextEvent == lastEventIndex)
                {
                    nextEvent = Random.Range(0, 2);
                }
                lastEventIndex = nextEvent;

                yield return StartCoroutine(PlayEvent(nextEvent));

                if (rookBoss.isDead) break;

                SpawnHealth();

                if (!rookBoss.IsShieldBroken())
                {
                    yield return new WaitForSeconds(pauseBetweenEvents);
                }
            }

            if (rookBoss.isDead) break;

            if (instructionText != null) instructionText.text = "¡ESCUDOS ABAJO! ¡ATACA AL JEFE!";
            rookBoss.SetShield(false);
            yield return new WaitForSeconds(vulnerabilityDuration);

            if (!rookBoss.isDead)
            {
                rookBoss.SetShield(true);

                if (instructionText != null) instructionText.text = "¡EL PUBLICO CURA EL ESCUDO! ¡DETENLOS!";
                SpawnEnemies(bishopPrefabs, standsSpawnPoints);

                while (!AllEnemiesDead() && !rookBoss.isDead)
                {
                    activeEnemies.RemoveAll(item => item == null);
                    float healAmount = (activeEnemies.Count * shieldHealPerBishop) * Time.deltaTime;
                    rookBoss.HealShield(healAmount);
                    yield return null;
                }

                yield return new WaitForSeconds(pauseBetweenEvents);
            }
        }
    }

    private IEnumerator PlayEvent(int eventIndex)
    {
        activeEnemies.Clear();

        if (eventIndex == 0)
        {
            if (instructionText != null) instructionText.text = "¡LA ESTAMPIDA! BUSCA EL HUECO";

            for (int w = 0; w < stampedeWaves; w++)
            {
                yield return StartCoroutine(RunStampedeWave());
                if (rookBoss.isDead) break;
                yield return new WaitForSeconds(1.5f);
            }

            ClearEnemies();
            rookBoss.DamageShield(shieldDamagePerEvent);
        }
        else if (eventIndex == 1)
        {
            if (instructionText != null) instructionText.text = "¡ROMPE SUS ESCUDOS Y ELIMINA A LOS PEONES!";
            SpawnEnemies(pawnPrefabs, arenaSpawnPoints);

            int initialCount = activeEnemies.Count;
            float damagePerPawn = initialCount > 0 ? shieldDamagePerEvent / initialCount : 0f;
            int currentCount = initialCount;

            while (currentCount > 0)
            {
                activeEnemies.RemoveAll(item => item == null);
                int deadEnemies = currentCount - activeEnemies.Count;

                if (deadEnemies > 0)
                {
                    rookBoss.DamageShield(damagePerPawn * deadEnemies);
                    currentCount = activeEnemies.Count;
                }

                yield return null;
            }
        }
    }

    private IEnumerator RunStampedeWave()
    {
        if (arenaCenter == null) yield break;

        int gapIndex = Random.Range(1, horsesPerWave - 2);
        List<GameObject> waveHorses = new List<GameObject>();

        int dir = Random.Range(0, 4);
        Vector3 startPos = arenaCenter.position;
        Vector3 moveDir = Vector3.zero;
        Vector3 rightDir = Vector3.zero;

        float halfSize = arenaSize / 2f;

        switch (dir)
        {
            case 0: startPos += new Vector3(0, 0, halfSize); moveDir = Vector3.back; rightDir = Vector3.right; break;
            case 1: startPos += new Vector3(0, 0, -halfSize); moveDir = Vector3.forward; rightDir = Vector3.right; break;
            case 2: startPos += new Vector3(halfSize, 0, 0); moveDir = Vector3.left; rightDir = Vector3.forward; break;
            case 3: startPos += new Vector3(-halfSize, 0, 0); moveDir = Vector3.right; rightDir = Vector3.forward; break;
        }

        float spacing = arenaSize / horsesPerWave;
        startPos -= rightDir * (halfSize - (spacing / 2f));

        for (int i = 0; i < horsesPerWave; i++)
        {
            if (i == gapIndex || i == gapIndex + 1) continue;

            Vector3 spawnPos = startPos + (rightDir * (i * spacing));
            spawnPos.y = arenaCenter.position.y + horseYOffset;

            GameObject prefab = knightPrefabs[Random.Range(0, knightPrefabs.Length)];
            GameObject horse = Instantiate(prefab, spawnPos, Quaternion.LookRotation(moveDir));

            waveHorses.Add(horse);
        }

        float distanceMoved = 0f;
        float startTime = Time.time;

        while (distanceMoved < arenaSize + 15f)
        {
            if (rookBoss.isDead) break;

            float step = stampedeSpeed * Time.deltaTime;
            distanceMoved += step;

            float waveTime = Time.time - startTime;
            float currentHopY = Mathf.Abs(Mathf.Sin(waveTime * horseJumpSpeed)) * horseJumpHeight;

            foreach (GameObject horse in waveHorses)
            {
                if (horse != null)
                {
                    horse.transform.position += moveDir * step;
                    Vector3 targetPos = horse.transform.position;
                    targetPos.y = (arenaCenter.position.y + horseYOffset) + currentHopY;
                    horse.transform.position = targetPos;
                }
            }
            yield return null;
        }

        foreach (GameObject horse in waveHorses)
        {
            if (horse != null) Destroy(horse);
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
                    if (enemy.GetComponent<EnemyGlow>() == null) enemy.AddComponent<EnemyGlow>();
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