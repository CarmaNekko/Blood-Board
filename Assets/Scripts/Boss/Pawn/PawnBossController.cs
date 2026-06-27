using UnityEngine;
using System.Collections;


public class PawnBossController : MonoBehaviour
{
    private enum BossState { Idle, Attacking, Fatigued, Transitioning, Defeated }
    [SerializeField] private BossState currentState = BossState.Idle;

    [Header("Modelos")]
    [SerializeField] private GameObject modelWhite;
    [SerializeField] private GameObject modelBlack;
    [SerializeField] private GameObject shieldVisual;

    private Transform lowerLasersPivot;
    private Transform upperLasersPivot;

    private Transform[] lowerLaserPivots;
    private Vector3[] lowerLaserOriginalScales;
    private Transform[] upperLaserPivots;
    private Vector3[] upperLaserOriginalScales;

    [Header("Ajustes de Animación Visual")]
    [SerializeField] private float expansionSpeed = 6f;

    private Renderer bossRenderer;
    private Color originalColor;
    private PawnBossHealth healthScript;

    [Header("Configuración de Fases")]
    [SerializeField] private float attackDuration = 15f;
    [SerializeField] private float fatigueDuration = 5f;

    [SerializeField] private float[] lowerLaserSpeeds = { 45f, 60f, 90f };
    [SerializeField] private float[] upperLaserSpeeds = { 0f, -75f, -110f };

    private int currentPhase = 0;

    [Header("Audio")]
    [SerializeField] private AudioClip laserDeploySound;
    [SerializeField] private AudioClip laserHumSound;
    [SerializeField, Range(0f, 1f)] private float audioVolume = 0.8f;

    private AudioSource deployAudioSource;
    private AudioSource humAudioSource;

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

        SetupAutoPivots(lowerLasersPivot, ref lowerLaserPivots, ref lowerLaserOriginalScales);
        SetupAutoPivots(upperLasersPivot, ref upperLaserPivots, ref upperLaserOriginalScales);

        if (shieldVisual != null) shieldVisual.SetActive(false);

        deployAudioSource = gameObject.AddComponent<AudioSource>();
        deployAudioSource.playOnAwake = false;
        deployAudioSource.spatialBlend = 1f;

        humAudioSource = gameObject.AddComponent<AudioSource>();
        humAudioSource.playOnAwake = false;
        humAudioSource.spatialBlend = 1f;
        humAudioSource.loop = true;
    }

    private void SetupAutoPivots(Transform pivotContainer, ref Transform[] pivotArray, ref Vector3[] scalesArray)
    {
        if (pivotContainer == null) return;

        pivotContainer.gameObject.SetActive(true);
        int count = pivotContainer.childCount;
        pivotArray = new Transform[count];
        scalesArray = new Vector3[count];

        Transform[] tempLasers = new Transform[count];
        for (int i = 0; i < count; i++) tempLasers[i] = pivotContainer.GetChild(i);

        for (int i = 0; i < count; i++)
        {
            Transform laser = tempLasers[i];

            GameObject autoPivot = new GameObject("AutoPivot_" + i);
            autoPivot.transform.SetParent(pivotContainer);

            autoPivot.transform.localPosition = new Vector3(0, laser.localPosition.y, 0);

            autoPivot.transform.localRotation = laser.localRotation;

            laser.SetParent(autoPivot.transform, true);

            pivotArray[i] = autoPivot.transform;
            scalesArray[i] = Vector3.one;

            autoPivot.transform.localScale = new Vector3(1, 0, 1);
        }
    }

    public void SetupIntroVisuals()
    {
        if (shieldVisual != null) shieldVisual.SetActive(true);
        currentState = BossState.Transitioning;

        if (laserDeploySound != null) deployAudioSource.PlayOneShot(laserDeploySound, audioVolume);
    }

    public void StartBattle()
    {
        StartCoroutine(BossLoopRoutine());
    }

    private IEnumerator BossLoopRoutine()
    {
        while (currentState != BossState.Defeated)
        {
            currentState = BossState.Attacking;
            if (bossRenderer != null) bossRenderer.material.color = originalColor;
            if (shieldVisual != null) shieldVisual.SetActive(true);

            if (laserHumSound != null && !humAudioSource.isPlaying)
            {
                humAudioSource.clip = laserHumSound;
                humAudioSource.volume = audioVolume;
                humAudioSource.Play();
            }

            yield return new WaitForSeconds(attackDuration);

            currentState = BossState.Fatigued;
            if (bossRenderer != null) bossRenderer.material.color = Color.gray;
            if (shieldVisual != null) shieldVisual.SetActive(false);

            if (humAudioSource != null) humAudioSource.Stop();

            yield return new WaitForSeconds(fatigueDuration);

            currentState = BossState.Transitioning;
            if (bossRenderer != null) bossRenderer.material.color = originalColor;
            if (shieldVisual != null) shieldVisual.SetActive(true);

            if (laserDeploySound != null) deployAudioSource.PlayOneShot(laserDeploySound, audioVolume);

            yield return new WaitForSeconds(1.5f);
        }
    }

    private void Update()
    {
        float targetLowerScale = 0f;
        float targetUpperScale = 0f;

        if (currentState == BossState.Attacking || currentState == BossState.Transitioning)
        {
            if (currentPhase >= 0) targetLowerScale = 1f;
            if (currentPhase >= 1) targetUpperScale = 1f;
        }

        if (lowerLaserPivots != null)
        {
            for (int i = 0; i < lowerLaserPivots.Length; i++)
            {
                Vector3 orig = lowerLaserOriginalScales[i];
                float newY = Mathf.Lerp(lowerLaserPivots[i].localScale.y, orig.y * targetLowerScale, Time.deltaTime * expansionSpeed);
                lowerLaserPivots[i].localScale = new Vector3(orig.x, newY, orig.z);
            }
        }

        if (upperLaserPivots != null)
        {
            for (int i = 0; i < upperLaserPivots.Length; i++)
            {
                Vector3 orig = upperLaserOriginalScales[i];
                float newY = Mathf.Lerp(upperLaserPivots[i].localScale.y, orig.y * targetUpperScale, Time.deltaTime * expansionSpeed);
                upperLaserPivots[i].localScale = new Vector3(orig.x, newY, orig.z);
            }
        }

        if (currentState == BossState.Attacking)
        {
            float currentLowerSpeed = lowerLaserSpeeds[Mathf.Clamp(currentPhase, 0, lowerLaserSpeeds.Length - 1)];
            if (lowerLasersPivot != null) lowerLasersPivot.Rotate(Vector3.up * currentLowerSpeed * Time.deltaTime);

            float currentUpperSpeed = upperLaserSpeeds[Mathf.Clamp(currentPhase, 0, upperLaserSpeeds.Length - 1)];
            if (upperLasersPivot != null) upperLasersPivot.Rotate(Vector3.up * currentUpperSpeed * Time.deltaTime);
        }
    }

    public void AdvancePhase()
    {
        if (currentPhase < 2) currentPhase++;
    }

    public bool IsFatigued()
    {
        return currentState == BossState.Fatigued;
    }

    public void SetDefeated()
    {
        currentState = BossState.Defeated;
        if (shieldVisual != null) shieldVisual.SetActive(false);

        if (humAudioSource != null) humAudioSource.Stop();

        if (lowerLaserPivots != null)
        {
            for (int i = 0; i < lowerLaserPivots.Length; i++)
            {
                lowerLaserPivots[i].localScale = new Vector3(lowerLaserOriginalScales[i].x, 0f, lowerLaserOriginalScales[i].z);
            }
        }
        if (upperLaserPivots != null)
        {
            for (int i = 0; i < upperLaserPivots.Length; i++)
            {
                upperLaserPivots[i].localScale = new Vector3(upperLaserOriginalScales[i].x, 0f, upperLaserOriginalScales[i].z);
            }
        }
    }
}