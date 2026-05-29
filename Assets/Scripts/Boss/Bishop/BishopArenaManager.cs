using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BishopArenaManager : MonoBehaviour
{
    [Header("Configuración de Supervivencia")]
    [SerializeField] private float survivalTime = 60f;
    [SerializeField] private float timeBetweenAttacks = 5f;
    private float currentTime;
    private bool battleStarted = false;

    [Header("UI (Interfaz)")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject survivalUIPanel;

    [Header("Puertas y Jefe")]
    [SerializeField] private GameObject entranceGate;
    [SerializeField] private GameObject exitGate;
    [SerializeField] private BishopBossController bishopBoss;

    private BishopPlatform[] allPlatforms;
    private Coroutine attackCoroutine;

    void Start()
    {
        currentTime = survivalTime;
        if (survivalUIPanel != null) survivalUIPanel.SetActive(false);
        if (entranceGate != null) entranceGate.SetActive(false);
        if (exitGate != null) exitGate.SetActive(true);

        if (bishopBoss != null)
        {
            bishopBoss.gameObject.SetActive(false);
        }

        allPlatforms = FindObjectsByType<BishopPlatform>(FindObjectsSortMode.None);

        UpdateTimerUI();
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

        if (bishopBoss != null)
        {
            bishopBoss.AppearAndGrow();

            while (!bishopBoss.IsReady)
            {
                yield return null;
            }
        }

        if (survivalUIPanel != null) survivalUIPanel.SetActive(true);

        StartCoroutine(SurvivalRoutine());
        attackCoroutine = StartCoroutine(AttackOrchestratorRoutine());
    }

    private IEnumerator SurvivalRoutine()
    {
        while (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerUI();
            yield return null;
        }

        currentTime = 0;
        UpdateTimerUI();
        WinSurvival();
    }

    private IEnumerator AttackOrchestratorRoutine()
    {
        yield return new WaitForSeconds(2f);

        while (currentTime > 0)
        {
            int attackCount = CalculateAttackCount();
            LaunchAttacks(attackCount);

            yield return new WaitForSeconds(timeBetweenAttacks);
        }
    }

    private int CalculateAttackCount()
    {
        float elapsed = survivalTime - currentTime;

        if (elapsed < 15f) return 1;
        if (elapsed < 30f) return 2;
        if (elapsed < 45f) return 3;
        return 4;
    }

    private void LaunchAttacks(int count)
    {
        if (allPlatforms == null || allPlatforms.Length == 0) return;

        List<BishopPlatform> availablePlatforms = new List<BishopPlatform>(allPlatforms);
        int triggered = 0;

        while (triggered < count && availablePlatforms.Count > 0)
        {
            int randomIndex = Random.Range(0, availablePlatforms.Count);
            BishopPlatform selected = availablePlatforms[randomIndex];

            selected.TargetPlatform();
            availablePlatforms.RemoveAt(randomIndex);
            triggered++;
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(Mathf.Max(currentTime, 0) / 60F);
            int seconds = Mathf.FloorToInt(Mathf.Max(currentTime, 0) - minutes * 60);
            timerText.text = string.Format("SOBREVIVE\n{0:00}:{1:00}", minutes, seconds);

            if (currentTime <= 10f) timerText.color = Color.red;
            else timerText.color = Color.white;
        }
    }

    private void WinSurvival()
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);

        if (survivalUIPanel != null) survivalUIPanel.SetActive(false);
        if (exitGate != null) exitGate.SetActive(false);

        if (bishopBoss != null)
        {
            bishopBoss.DefeatAndFall();
            Destroy(bishopBoss.gameObject, 6f);
        }
    }
}