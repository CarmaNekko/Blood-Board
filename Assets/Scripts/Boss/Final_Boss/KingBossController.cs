using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KingBossController : MonoBehaviour
{
    [Header("Escudo de la Reina")]
    public GameObject queenShieldVisual;
    private bool isProtecting = false;

    [Header("Invocación de Tropas Múltiples")]
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;

    [Header("Configuración de la Torre")]
    public float distanceBetweenFloors = 30f;

    [Header("Estado Final (Piso 5)")]
    private bool isVulnerable = false;

    [Header("Barrera del Trono")]
    public GameObject throneBarrier;

    [Header("Final del Juego")]
    public string creditsSceneName = "Creditos";

    private List<GameObject> activeEnemies = new List<GameObject>();
    private QueenBossController queen;

    void Start()
    {
        if (queenShieldVisual != null) queenShieldVisual.SetActive(false);
        queen = FindFirstObjectByType<QueenBossController>();
    }

    void Update()
    {
        if (isProtecting)
        {
            activeEnemies.RemoveAll(item => item == null);

            if (activeEnemies.Count == 0)
            {
                BreakShield();
            }
        }
    }

    public void ActivateDefensePhase(int phase)
    {
        isProtecting = true;

        if (queenShieldVisual != null) queenShieldVisual.SetActive(true);

        if (enemyPrefabs.Length > 0 && spawnPoints.Length > 0)
        {
            float yOffset = (phase - 2) * distanceBetweenFloors;

            int enemiesToSpawn = phase * 2;
            if (enemiesToSpawn > spawnPoints.Length) enemiesToSpawn = spawnPoints.Length;

            List<Transform> availableSpawns = new List<Transform>(spawnPoints);

            for (int i = 0; i < enemiesToSpawn; i++)
            {
                int spawnIndex = Random.Range(0, availableSpawns.Count);
                Transform sp = availableSpawns[spawnIndex];
                availableSpawns.RemoveAt(spawnIndex);

                Vector3 calculatedSpawnPos = new Vector3(sp.position.x, sp.position.y - yOffset, sp.position.z);

                int maxEnemyIndex = Mathf.Min(phase + 1, enemyPrefabs.Length);
                int enemyIndex = Random.Range(0, maxEnemyIndex);

                GameObject newEnemy = Instantiate(enemyPrefabs[enemyIndex], calculatedSpawnPos, sp.rotation);
                activeEnemies.Add(newEnemy);
            }
        }
    }

    private void BreakShield()
    {
        isProtecting = false;
        if (queenShieldVisual != null) queenShieldVisual.SetActive(false);

        if (queen != null)
        {
            queen.EndPhaseTransition();
        }
    }

    public void MakeVulnerable()
    {
        isVulnerable = true;
        isProtecting = false;
        if (queenShieldVisual != null) queenShieldVisual.SetActive(false);
        if (throneBarrier != null) throneBarrier.SetActive(false);
    }

    public void TakeDamage(float amount, MagicColor hitColor)
    {
        if (!isVulnerable) return;

        SceneManager.LoadScene(creditsSceneName);
    }

    public void DescendToNextFloor()
    {
        StartCoroutine(DescendRoutine());
    }

    private System.Collections.IEnumerator DescendRoutine()
    {
        float startY = transform.position.y;
        float targetY = startY - distanceBetweenFloors;
        float fallTime = 1.5f;
        float currentFallTime = 0f;

        while (currentFallTime < fallTime)
        {
            transform.position = new Vector3(transform.position.x, Mathf.Lerp(startY, targetY, currentFallTime / fallTime), transform.position.z);
            currentFallTime += Time.deltaTime;
            yield return null;
        }

        transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
    }
}