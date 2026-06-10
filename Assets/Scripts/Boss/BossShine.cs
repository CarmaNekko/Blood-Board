using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public class BossShine : MonoBehaviour
{
    [Header("Shine Position")]
    [SerializeField] private Vector3 shineLocalOffset = Vector3.zero;

    [Header("Shine Prefabs")]
    [SerializeField] private GameObject whiteShinePrefab;
    [SerializeField] private GameObject blackShinePrefab;

    [Header("Shine Settings")]
    [SerializeField] private bool billboardToCamera = true;
    [SerializeField] private bool preserveLocalRotation = true;

    [Header("Shine Effects")]
    [SerializeField] private bool enablePulse = true;
    [SerializeField] private float shinePulseFrequency = 1.5f;
    [SerializeField] private float shinePulseAmplitude = 0.2f;
    [SerializeField] private float shineEmissionIntensity = 1.5f;

    [SerializeField] private Color whiteShineEmissionColor =
        new Color(1f, 0.95f, 0.7f, 1f);

    [SerializeField] private Color blackShineEmissionColor =
        new Color(0.75f, 0.45f, 1f, 1f);

    [Header("Boss References")]
    [SerializeField] private Transform hitboxTransform;
    [SerializeField] private Transform whiteModelTransform;
    [SerializeField] private Transform blackModelTransform;

    [Header("Vulnerable State")]
    [SerializeField] private Color vulnerableShineEmissionColor =
        new Color(1f, 0.5f, 0.5f, 1f);

    [SerializeField] private bool blinkWhenVulnerable = true;

    [Header("Utility")]
    [SerializeField] private bool drawGizmo = true;

    [SerializeField] private bool useShineScaleMultiplier = false;
    [SerializeField] private Vector3 shineCustomScale = Vector3.one;

    [SerializeField] private Color gizmoColor = Color.yellow;

    public bool DrawGizmo => drawGizmo;
    public Color GizmoColor => gizmoColor;

    private EnemyHealth enemyHealth;
    private PawnBossHealth pawnBossHealth;

    private MagicColor? currentColor;

    private readonly List<GameObject> currentShineInstances =
        new List<GameObject>();

    private readonly List<ShineData> currentShineData =
        new List<ShineData>();

    private Vector3 originalShineScale = Vector3.one;

    private struct ShineData
    {
        public Renderer Renderer;
        public SpriteRenderer SpriteRenderer;
        public Material Material;
        public Color OriginalMaterialColor;
        public Color OriginalSpriteColor;
    }

    private void Awake()
    {
        pawnBossHealth = GetComponentInChildren<PawnBossHealth>();

        if (pawnBossHealth == null)
        {
            enemyHealth =
                hitboxTransform != null
                ? hitboxTransform.GetComponent<EnemyHealth>()
                : GetComponentInChildren<EnemyHealth>();
        }

        FindModelTransforms();

        UpdateShineInstance();
    }

    private void Start()
    {
        if (currentShineInstances.Count == 0)
        {
            UpdateShineInstance();
        }
    }

    private void Update()
    {
        MagicColor color = ResolveEnemyColor();

        if (!currentColor.HasValue ||
            currentColor.Value != color)
        {
            UpdateShineInstance();
        }

        if (currentShineInstances.Count > 0)
        {
            float pulse =
                enablePulse
                ? 1f +
                  shinePulseAmplitude *
                  Mathf.Sin(
                      (Time.time * shinePulseFrequency) *
                      Mathf.PI * 2f)
                : 1f;

            if (blinkWhenVulnerable && IsVulnerable())
            {
                pulse =
                    1f +
                    shinePulseAmplitude *
                    Mathf.Sin(
                        (Time.time *
                         shinePulseFrequency *
                         3f) *
                        Mathf.PI * 2f);
            }

            for (int i = 0; i < currentShineInstances.Count; i++)
            {
                GameObject shineInstance =
                    currentShineInstances[i];

                if (shineInstance == null)
                    continue;

                shineInstance.transform.localPosition = shineLocalOffset;
                shineInstance.transform.localScale = originalShineScale * pulse;
            }

            UpdateShineEmission(
                pulse,
                GetEmissionColor(color));
        }

        if (billboardToCamera)
        {
            Camera cam = Camera.main;

            if (cam != null)
            {
                foreach (GameObject shine in currentShineInstances)
                {
                    if (shine == null)
                        continue;

                    shine.transform.rotation =
                        Quaternion.LookRotation(
                            shine.transform.position -
                            cam.transform.position);
                }
            }
        }
    }

    private MagicColor ResolveEnemyColor()
    {
        if (enemyHealth != null)
        {
            return enemyHealth.myColor;
        }

        if (pawnBossHealth != null)
        {
            return pawnBossHealth.myColor;
        }

        if (whiteModelTransform != null &&
            whiteModelTransform.gameObject.activeInHierarchy)
            return MagicColor.White;

        if (blackModelTransform != null &&
            blackModelTransform.gameObject.activeInHierarchy)
            return MagicColor.Black;

        if (GetComponent<BossKnight>() != null)
            return MagicColor.Black;

        if (GetComponent<RookBossController>() != null)
            return MagicColor.Black;

        if (GetComponent<BishopBossController>() != null)
            return MagicColor.White;

        return MagicColor.White;
    }

    private void UpdateShineInstance()
    {
        DestroyCurrentShine();

        MagicColor color = ResolveEnemyColor();

        GameObject prefab =
            color == MagicColor.White
            ? whiteShinePrefab
            : blackShinePrefab;

        if (prefab == null)
            return;

        CreateShineInstance(prefab, shineLocalOffset);

        if (currentShineInstances.Count > 0)
        {
            originalShineScale =
                useShineScaleMultiplier
                ? shineCustomScale
                : currentShineInstances[0]
                    .transform.localScale;
        }

        currentColor = color;
    }

    private void CreateShineInstance(
        GameObject prefab,
        Vector3 localOffset)
    {
        GameObject shine =
            Instantiate(
                prefab,
                transform);

        shine.transform.localPosition =
            localOffset;

        if (preserveLocalRotation)
        {
            shine.transform.localRotation =
                Quaternion.identity;
        }

        shine.name = "_BossShine";

        currentShineInstances.Add(shine);
        currentShineData.Add(
            CacheShineRenderers(shine));
    }

    private ShineData CacheShineRenderers(
        GameObject shine)
    {
        ShineData data = new ShineData();

        data.Renderer =
            shine.GetComponentInChildren<Renderer>();

        data.SpriteRenderer =
            shine.GetComponentInChildren<SpriteRenderer>();

        if (data.Renderer != null)
        {
            data.Material =
                data.Renderer.material;

            data.Renderer.shadowCastingMode =
                ShadowCastingMode.Off;

            data.Renderer.receiveShadows = false;
        }

        if (data.SpriteRenderer != null)
        {
            data.SpriteRenderer.shadowCastingMode =
                ShadowCastingMode.Off;

            data.SpriteRenderer.receiveShadows = false;

            data.OriginalSpriteColor =
                data.SpriteRenderer.color;

            if (data.Material == null)
            {
                data.Material =
                    data.SpriteRenderer.material;
            }
        }

        return data;
    }

    private void FindModelTransforms()
    {
        if (whiteModelTransform == null)
        {
            whiteModelTransform = FindDeepChild(transform, "ModelWhite");
            if (whiteModelTransform == null)
                whiteModelTransform = FindDeepChild(transform, "modelWhite");
        }
        if (blackModelTransform == null)
        {
            blackModelTransform = FindDeepChild(transform, "ModelBlack");
            if (blackModelTransform == null)
                blackModelTransform = FindDeepChild(transform, "modelBlack");
        }
    }

    private Transform FindDeepChild(Transform parent, string name)
    {
        Transform[] children = parent.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name == name)
            {
                return child;
            }
        }
        return null;
    }

    private Vector3 GetAutoShineLocalOffset()
    {
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        List<Renderer> bodyRenderers = new List<Renderer>();
        foreach (var r in allRenderers)
        {
            if (r.gameObject.name.Contains("Shine")) continue;
            bodyRenderers.Add(r);
        }

        if (bodyRenderers.Count == 0)
            return shineLocalOffset;

        Bounds bounds = bodyRenderers[0].bounds;

        for (int i = 1; i < bodyRenderers.Count; i++)
        {
            bounds.Encapsulate(
                bodyRenderers[i].bounds);
        }

        Vector3 topCenter =
            bounds.center +
            Vector3.up * bounds.extents.y;

        return transform.InverseTransformPoint(
            topCenter);
    }

    private bool IsVulnerable()
    {
        if (pawnBossHealth != null)
        {
            PawnBossController controller =
                GetComponent<PawnBossController>();

            if (controller != null)
                return controller.IsFatigued();
        }

        if (enemyHealth != null &&
            GetComponent<RookBossController>() != null)
        {
            return !enemyHealth.isShielded;
        }

        return false;
    }

    private Color GetEmissionColor(
        MagicColor color)
    {
        if (IsVulnerable())
            return vulnerableShineEmissionColor;

        return color == MagicColor.White
            ? whiteShineEmissionColor
            : blackShineEmissionColor;
    }

    private void UpdateShineEmission(
        float pulse,
        Color color)
    {
        float intensity =
            pulse * shineEmissionIntensity;

        foreach (var data in currentShineData)
        {
            if (data.Material != null)
            {
                Color finalColor =
                    color * intensity;

                if (data.Material.HasProperty("_BaseColor"))
                    data.Material.SetColor(
                        "_BaseColor",
                        finalColor);
                else if (data.Material.HasProperty("_Color"))
                    data.Material.SetColor(
                        "_Color",
                        finalColor);
            }

            if (data.SpriteRenderer != null)
            {
                Color c =
                    color * intensity;

                c.a =
                    data.OriginalSpriteColor.a;

                data.SpriteRenderer.color = c;
            }
        }
    }

    private void DestroyCurrentShine()
    {
        foreach (GameObject shine in currentShineInstances)
        {
            if (shine != null)
                Destroy(shine);
        }

        currentShineInstances.Clear();
        currentShineData.Clear();
    }

    private void OnDestroy()
    {
        DestroyCurrentShine();
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmo)
            return;

        Gizmos.color = gizmoColor;

        Vector3 worldPos =
            transform.TransformPoint(shineLocalOffset);

        Gizmos.DrawSphere(worldPos, 0.15f);
        Gizmos.DrawLine(transform.position, worldPos);
    }
}