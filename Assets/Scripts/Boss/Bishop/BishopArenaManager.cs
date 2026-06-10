using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BishopArenaManager : MonoBehaviour
{
    [Header("Configuración Base")]
    [SerializeField] private float baseSurvivalDuration = 10f;
    [SerializeField] private float extraTimePerCrystal = 3f;
    [SerializeField] private float vulnerabilityDuration = 15f;
    [SerializeField] private float timeBetweenAttacks = 1.5f;
    [SerializeField] private int maxSimultaneousAttacks = 3;
    [SerializeField] private float timeBetweenPatterns = 4f;

    [Header("UI (Interfaz)")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject survivalUIPanel;
    [SerializeField] private Slider bossHealthBar;

    [Header("Referencias del Mapa")]
    [SerializeField] private GameObject entranceGate;
    [SerializeField] private GameObject exitGate;
    [SerializeField] private BishopBossController bishopBoss;
    [SerializeField] private List<BishopCrystal> arenaCrystals;

    private BishopPlatform[] allPlatforms;
    private Transform playerTransform;
    private PlayerMovement playerMovement;

    private bool battleStarted = false;
    private int totalCrystals;
    private int crystalsRemaining;
    private bool crystalJustDestroyed = false;
    private Vector3 arenaCenter;

    void Start()
    {
        if (survivalUIPanel != null) survivalUIPanel.SetActive(false);
        if (entranceGate != null) entranceGate.SetActive(false);
        if (exitGate != null) exitGate.SetActive(true);

        if (bishopBoss != null) bishopBoss.gameObject.SetActive(false);
        if (bossHealthBar != null) bossHealthBar.gameObject.SetActive(false);

        allPlatforms = FindObjectsByType<BishopPlatform>(FindObjectsSortMode.None);

        if (allPlatforms != null && allPlatforms.Length > 0)
        {
            Vector3 sum = Vector3.zero;
            foreach (var p in allPlatforms)
            {
                sum += p.transform.position;
            }
            arenaCenter = sum / allPlatforms.Length;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerMovement = player.GetComponent<PlayerMovement>();
        }

        totalCrystals = arenaCrystals.Count;
        crystalsRemaining = totalCrystals;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !battleStarted)
        {
            battleStarted = true;
            StartCoroutine(StartBattleSequence());
        }
    }

    private IEnumerator StartBattleSequence()
    {
        if (entranceGate != null) entranceGate.SetActive(true);

        foreach (var crystal in arenaCrystals)
        {
            crystal.Initialize(this, bishopBoss.transform);
        }

        yield return new WaitForSeconds(1.5f);

        if (bishopBoss != null)
        {
            bishopBoss.AppearAndGrow();
            while (!bishopBoss.IsReady) yield return null;
        }

        if (survivalUIPanel != null) survivalUIPanel.SetActive(true);

        if (bossHealthBar != null)
        {
            bossHealthBar.gameObject.SetActive(true);
            bossHealthBar.maxValue = totalCrystals;
            bossHealthBar.value = crystalsRemaining;
        }

        yield return StartCoroutine(BattleLoop());
    }

    private IEnumerator BattleLoop()
    {
        while (crystalsRemaining > 0)
        {
            crystalJustDestroyed = false;
            SetCrystalsState(true);
            yield return StartCoroutine(SurvivalPhase());

            if (crystalsRemaining <= 0) break;

            SetCrystalsState(false);
            yield return StartCoroutine(VulnerabilityPhase());

            if (crystalJustDestroyed && crystalsRemaining > 0)
            {
                yield return StartCoroutine(CounterAttackPhase());
            }
        }

        WinBattle();
    }

    private void SetCrystalsState(bool protect)
    {
        foreach (var crystal in arenaCrystals)
        {
            if (crystal != null && crystal.gameObject.activeSelf)
            {
                if (!protect) crystal.RandomizeColor();
                crystal.SetProtected(protect);
            }
        }
    }

    private IEnumerator SurvivalPhase()
    {
        int destroyedCount = totalCrystals - crystalsRemaining;
        float currentSurvivalDuration = baseSurvivalDuration + (destroyedCount * extraTimePerCrystal);

        int simultaneousAttacks = Mathf.Min(1 + destroyedCount, maxSimultaneousAttacks);

        float timeLeft = currentSurvivalDuration;
        float attackTimer = 0f;

        while (timeLeft > 0 && crystalsRemaining > 0)
        {
            timeLeft -= Time.deltaTime;
            attackTimer -= Time.deltaTime;

            if (attackTimer <= 0)
            {
                LaunchPredictiveAttacks(simultaneousAttacks);
                attackTimer = timeBetweenAttacks;
            }

            UpdateTimerUI("SOBREVIVE", timeLeft);
            yield return null;
        }
    }

    private IEnumerator VulnerabilityPhase()
    {
        float timeLeft = vulnerabilityDuration;

        while (timeLeft > 0 && crystalsRemaining > 0 && !crystalJustDestroyed)
        {
            timeLeft -= Time.deltaTime;
            UpdateTimerUI("¡DESTRUYE UN CRISTAL!", timeLeft);
            yield return null;
        }
    }

    private IEnumerator CounterAttackPhase()
    {
        int attackWaves = Random.Range(2, 4);

        for (int i = 0; i < attackWaves; i++)
        {
            if (timerText != null)
            {
                timerText.text = $"¡CONTRAATAQUE!\nPATRÓN {i + 1}/{attackWaves}";
                timerText.color = Color.red;
            }

            int randomPattern = Random.Range(0, 4);
            ExecuteGeometricPattern(randomPattern);

            yield return new WaitForSeconds(timeBetweenPatterns);
        }

        if (timerText != null) timerText.color = Color.white;
        yield return new WaitForSeconds(1f);
    }

    private void ExecuteGeometricPattern(int patternIndex)
    {
        if (allPlatforms == null || allPlatforms.Length == 0) return;

        float tolerance = 2.0f;

        foreach (var platform in allPlatforms)
        {
            if (platform == null || platform.IsTargeted) continue;

            float dx = Mathf.Abs(platform.transform.position.x - arenaCenter.x);
            float dz = Mathf.Abs(platform.transform.position.z - arenaCenter.z);
            bool triggerThis = false;

            switch (patternIndex)
            {
                case 0:
                    if (Mathf.Abs(dx - dz) < tolerance) triggerThis = true;
                    break;

                case 1:
                    if (dx < tolerance || dz < tolerance) triggerThis = true;
                    break;

                case 2:
                    int row = Mathf.RoundToInt(platform.transform.position.z / 4f);
                    if (row % 2 == 0) triggerThis = true;
                    break;

                case 3:
                    int col = Mathf.RoundToInt(platform.transform.position.x / 4f);
                    if (col % 2 == 0) triggerThis = true;
                    break;
            }

            if (triggerThis)
            {
                platform.TargetPlatform();
            }
        }
    }

    private void LaunchPredictiveAttacks(int count)
    {
        if (allPlatforms == null || allPlatforms.Length == 0 || playerTransform == null) return;

        Vector3 predictedPos = playerTransform.position;
        if (playerMovement != null && playerMovement.CurrentVelocity.magnitude > 1f)
        {
            predictedPos += playerMovement.CurrentVelocity * 1.3f;
        }

        List<BishopPlatform> availablePlats = new List<BishopPlatform>();
        foreach (var p in allPlatforms)
        {
            if (!p.IsTargeted) availablePlats.Add(p);
        }

        availablePlats.Sort((a, b) =>
        {
            float distA = Vector3.Distance(predictedPos, a.transform.position) + Random.Range(-2.5f, 2.5f);
            float distB = Vector3.Distance(predictedPos, b.transform.position) + Random.Range(-2.5f, 2.5f);
            return distA.CompareTo(distB);
        });

        for (int i = 0; i < count && i < availablePlats.Count; i++)
        {
            availablePlats[i].TargetPlatform();
        }
    }

    public void OnCrystalDestroyed()
    {
        crystalsRemaining--;
        if (bossHealthBar != null) bossHealthBar.value = crystalsRemaining;
        crystalJustDestroyed = true;
    }

    private void UpdateTimerUI(string message, float time)
    {
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(Mathf.Max(time, 0));
            timerText.text = $"{message}\n{seconds}s";
            timerText.color = (time <= 5f) ? Color.red : Color.white;
        }
    }

    private void WinBattle()
    {
        StopAllCoroutines();

        if (survivalUIPanel != null) survivalUIPanel.SetActive(false);
        if (exitGate != null) exitGate.SetActive(false);
        if (bossHealthBar != null) bossHealthBar.gameObject.SetActive(false);

        if (bishopBoss != null)
        {
            bishopBoss.DefeatAndFall();
            Destroy(bishopBoss.gameObject, 6f);
        }
    }
}