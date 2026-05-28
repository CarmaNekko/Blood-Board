using UnityEngine;
using System.Collections;

public class PawnBossController : MonoBehaviour
{
    private enum BossState { Attacking, Fatigued, Transitioning, Defeated }
    [SerializeField] private BossState currentState = BossState.Attacking;

    [Header("Modelos")]
    [SerializeField] private GameObject modelWhite;
    [SerializeField] private GameObject modelBlack;

    private Transform lowerLasersPivot;
    private Transform upperLasersPivot;
    private Renderer bossRenderer;
    private Color originalColor;
    private PawnBossHealth healthScript;

    [Header("Configuración de Fases")]
    [SerializeField] private float attackDuration = 15f;
    [SerializeField] private float fatigueDuration = 5f;

    [SerializeField] private float[] lowerLaserSpeeds = { 45f, 60f, 90f };
    [SerializeField] private float[] upperLaserSpeeds = { 0f, -75f, -110f };

    private int currentPhase = 0;

    private void Start()
    {
        bool isWhite = Random.value > 0.5f;

        if (modelWhite != null) modelWhite.SetActive(isWhite);
        if (modelBlack != null) modelBlack.SetActive(!isWhite);

        Transform activeModel = isWhite ? modelWhite.transform : modelBlack.transform;

        lowerLasersPivot = activeModel.Find("Lazers_1");
        upperLasersPivot = activeModel.Find("Lazers_2");

        Transform unionModel = activeModel.Find("pasted__Pawn_Union");
        if (unionModel != null)
        {
            bossRenderer = unionModel.GetComponent<Renderer>();
            if (bossRenderer != null) originalColor = bossRenderer.material.color;
        }

        healthScript = GetComponent<PawnBossHealth>();
        if (healthScript != null)
        {
            healthScript.myColor = isWhite ? MagicColor.White : MagicColor.Black;
        }

        StartCoroutine(BossLoopRoutine());
        SetPhaseProperties(0);
    }

    private IEnumerator BossLoopRoutine()
    {
        while (currentState != BossState.Defeated)
        {
            currentState = BossState.Attacking;
            if (bossRenderer != null) bossRenderer.material.color = originalColor;

            yield return new WaitForSeconds(attackDuration);

            currentState = BossState.Fatigued;
            if (bossRenderer != null) bossRenderer.material.color = Color.gray;

            yield return new WaitForSeconds(fatigueDuration);

            currentState = BossState.Transitioning;
            yield return new WaitForSeconds(1f);
        }
    }

    private void Update()
    {
        if (currentState == BossState.Attacking)
        {
            float currentLowerSpeed = lowerLaserSpeeds[Mathf.Clamp(currentPhase, 0, lowerLaserSpeeds.Length - 1)];
            if (lowerLasersPivot != null && lowerLasersPivot.gameObject.activeSelf)
            {
                lowerLasersPivot.Rotate(Vector3.up * currentLowerSpeed * Time.deltaTime);
            }

            float currentUpperSpeed = upperLaserSpeeds[Mathf.Clamp(currentPhase, 0, upperLaserSpeeds.Length - 1)];
            if (upperLasersPivot != null && upperLasersPivot.gameObject.activeSelf)
            {
                upperLasersPivot.Rotate(Vector3.up * currentUpperSpeed * Time.deltaTime);
            }
        }
    }

    public void AdvancePhase()
    {
        if (currentPhase < 2)
        {
            currentPhase++;
            SetPhaseProperties(currentPhase);
        }
    }

    private void SetPhaseProperties(int phaseIndex)
    {
        switch (phaseIndex)
        {
            case 0:
                if (lowerLasersPivot != null) lowerLasersPivot.gameObject.SetActive(true);
                if (upperLasersPivot != null) upperLasersPivot.gameObject.SetActive(false);
                break;
            case 1:
                if (lowerLasersPivot != null) lowerLasersPivot.gameObject.SetActive(true);
                if (upperLasersPivot != null) upperLasersPivot.gameObject.SetActive(true);
                break;
            case 2:
                if (lowerLasersPivot != null) lowerLasersPivot.gameObject.SetActive(true);
                if (upperLasersPivot != null) upperLasersPivot.gameObject.SetActive(true);
                break;
        }
    }

    public bool IsFatigued()
    {
        return currentState == BossState.Fatigued;
    }
}