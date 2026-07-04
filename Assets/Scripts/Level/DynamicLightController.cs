using UnityEngine;

public class DynamicLightController : MonoBehaviour
{
    [Header("Target Light")]
    [SerializeField] private Light targetLight;

    [Header("Light Intensity")]
    [SerializeField] private float initialIntensity = 0.85f;
    [SerializeField] private float finalIntensity = 1.15f;
    [SerializeField] private float intensityVariationSpeed = 3f;

    [Header("Light Range")]
    [SerializeField] private float baseRange = 5f;
    [SerializeField] private float minRangeMultiplier = 0.9f;
    [SerializeField] private float maxRangeMultiplier = 1.1f;
    [SerializeField] private float rangeVariationSpeed = 2.5f;

    [Header("Darkness Response")]
    [SerializeField] private bool respondToDarkness = true;
    [SerializeField] private float darknessIntensityBoost = 0.5f;
    [SerializeField] private AnimationCurve darknessResponseCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    private DungeonLightingManager lightingManager;

    private void Awake()
    {
        if (respondToDarkness)
        {
            lightingManager = FindFirstObjectByType<DungeonLightingManager>();
        }
    }

    private void Update()
    {
        if (targetLight == null) return;
        float time = Time.time;
        float intensityT = (Mathf.Sin(time * intensityVariationSpeed) + 1f) / 2f;
        float targetIntensity = Mathf.Lerp(initialIntensity, finalIntensity, intensityT);

        if (respondToDarkness && lightingManager != null)
        {
            targetIntensity += darknessResponseCurve.Evaluate(lightingManager.CurrentDarkness) * darknessIntensityBoost;
        }

        targetLight.intensity = Mathf.Clamp(targetIntensity, 0f, 10f);
        float rangeT = (Mathf.Sin(time * rangeVariationSpeed) + 1f) / 2f;
        targetLight.range = baseRange * Mathf.Lerp(minRangeMultiplier, maxRangeMultiplier, rangeT);
    }

    public void SetIntensityRange(float initial, float final)
    {
        initialIntensity = Mathf.Max(0f, initial);
        finalIntensity = Mathf.Max(0f, final);
    }

    public void SetBaseRange(float range)
    {
        baseRange = Mathf.Max(0.1f, range);
    }

    public void SetActive(bool active)
    {
        if (targetLight != null)
        {
            targetLight.enabled = active;
        }
    }
}