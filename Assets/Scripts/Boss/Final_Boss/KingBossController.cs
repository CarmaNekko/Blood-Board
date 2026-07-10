using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KingBossController : MonoBehaviour
{
    [Header("Vulnerabilidad")]
    [SerializeField] private MagicColor bossColor = MagicColor.Black;

    [Header("Recompensas")]
    public GameObject potionPrefab;

    [Header("Escudo de la Reina")]
    public GameObject queenShieldVisual;
    private bool isProtecting = false;

    [Header("Invocación de Tropas Múltiples")]
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;

    [Header("Configuración de Descenso")]
    public float distanceBetweenFloors = 30f;

    [Header("Animación de Flote (Rey)")]
    public Vector2 hoverArenaPosition = new Vector2(0f, 20f);
    public float floatSpeed = 2f;
    public float floatAmplitude = 0.5f;
    private bool isFloating = false;
    private Vector3 floatCenterPos;
    private bool hasEnteredArena = false;

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

        if (isFloating)
        {
            float newY = floatCenterPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
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

        if (potionPrefab != null)
        {
            float nextFloorY = -LevelManager.currentLevel * 27f;

            Vector3 dropPosition = new Vector3(0f, nextFloorY, 0f);
            Instantiate(potionPrefab, dropPosition, Quaternion.identity);
        }

        if (queen != null)
        {
            queen.EndPhaseTransition();
        }
    }

    public void MakeVulnerable()
    {
        isVulnerable = true;
        isProtecting = false;
        isFloating = false;

        if (queenShieldVisual != null) queenShieldVisual.SetActive(false);
        if (throneBarrier != null) throneBarrier.SetActive(false);

        StartCoroutine(FallToGroundRoutine());
    }

    private System.Collections.IEnumerator FallToGroundRoutine()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(startPos.x, startPos.y - 10.5f, startPos.z);

        float fallTime = 0.8f;
        float currentFallTime = 0f;

        while (currentFallTime < fallTime)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, currentFallTime / fallTime);
            currentFallTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
    }

    public void TakeDamage(float amount, MagicColor hitColor)
    {
        if (!isVulnerable) return;
        if (hitColor == bossColor) return;

        SceneManager.LoadScene(creditsSceneName);
    }

    public void DescendToNextFloor()
    {
        StartCoroutine(DescendRoutine());
    }

    private System.Collections.IEnumerator DescendRoutine()
    {
        Vector3 startPos = transform.position;

        float targetY = startPos.y - distanceBetweenFloors;
        if (!hasEnteredArena)
        {
            targetY += 10.5f;
            hasEnteredArena = true;
        }

        Vector3 targetPos = new Vector3(hoverArenaPosition.x, targetY, hoverArenaPosition.y);

        float fallTime = 1.5f;
        float currentFallTime = 0f;

        isFloating = false;

        while (currentFallTime < fallTime)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, currentFallTime / fallTime);
            currentFallTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;

        floatCenterPos = transform.position;
        isFloating = true;
    }
}