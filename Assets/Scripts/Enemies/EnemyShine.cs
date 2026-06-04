using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public class EnemyShine : MonoBehaviour
{
    [Header("Shine Position")]
    [Tooltip("Desplazamiento local donde se colocará el shine respecto al enemigo. Ajusta la posición usando el gizmo de escena.")]
    [SerializeField] private Vector3 shineLocalOffset = Vector3.zero;

    [Header("Shine Prefabs")]
    [Tooltip("Prefab del brillo blanco. Debe ser un plano o sprite configurado para el ojo blanco.")]
    [SerializeField] private GameObject whiteShinePrefab;

    [Tooltip("Prefab del brillo negro. Debe ser un plano o sprite configurado para el ojo negro.")]
    [SerializeField] private GameObject blackShinePrefab;

    [Header("Shine Settings")]
    [Tooltip("Si está activo, el shine girará para mirar a la cámara principal.")]
    [SerializeField] private bool billboardToCamera = true;

    [Tooltip("Si está activo, el shine mantendrá su rotación local original después de instanciarlo.")]
    [SerializeField] private bool preserveLocalRotation = true;

    [Header("Shine Effects")]
    [Tooltip("Activa el pulso del shine.")]
    [SerializeField] private bool enablePulse = true;

    [Tooltip("Frecuencia del pulso del shine en ciclos por segundo.")]
    [SerializeField] private float shinePulseFrequency = 1.5f;

    [Tooltip("Amplitud del pulso. 0.2 = 20% de variación de tamaño y brillo.")]
    [SerializeField] private float shinePulseAmplitude = 0.2f;

    [Tooltip("Multiplicador de brillo/emisión.")]
    [SerializeField] private float shineEmissionIntensity = 1.5f;

    [Tooltip("Color de emisión para el shine blanco (amarillento claro).")]
    [SerializeField] private Color whiteShineEmissionColor = new Color(1f, 0.95f, 0.7f, 1f);

    [Tooltip("Color de emisión para el shine negro (moradito).")]
    [SerializeField] private Color blackShineEmissionColor = new Color(0.75f, 0.45f, 1f, 1f);

    [Tooltip("Dibuja un gizmo en la escena para ver y ajustar la posición del shine.")]
    [SerializeField] private bool drawGizmo = true;

    [Tooltip("Calcula automáticamente el offset del shine usando los bounds del renderer, útil para enemigos con pivote en el suelo.")]
    [SerializeField] private bool autoComputeShineOffset = true;

    [Tooltip("Si está activo, aplica la escala personalizada (shineCustomScale) al shine en lugar de usar la del prefab.")]
    [SerializeField] private bool useShineScaleMultiplier = false;

    [Tooltip("Escala personalizada para el shine cuando useShineScaleMultiplier está activo (X, Y, Z).")]
    [SerializeField] private Vector3 shineCustomScale = Vector3.one;

    [Tooltip("Offsets locales adicionales para instanciar shines extra cuando el enemigo tiene varios ojos.")]
    [SerializeField] private List<Vector3> additionalShineLocalOffsets = new List<Vector3>();

    [SerializeField] private Color gizmoColor = Color.yellow;
    [Tooltip("Activar para imprimir información de depuración sobre renderers/materiales de los shines.")]
    [SerializeField] private bool debugShine = false;

    private EnemyHealth enemyHealth;
    private PawnBossHealth pawnBossHealth;
    private MagicColor? currentColor;
    private readonly List<GameObject> currentShineInstances = new List<GameObject>();
    private readonly List<ShineData> currentShineData = new List<ShineData>();
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
        enemyHealth = GetComponentInChildren<EnemyHealth>();
        pawnBossHealth = GetComponentInChildren<PawnBossHealth>();
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
        if (enemyHealth == null && pawnBossHealth == null)
        {
            return;
        }

        MagicColor color = ResolveEnemyColor();
        if (!currentColor.HasValue || currentColor.Value != color)
        {
            UpdateShineInstance();
        }

        if (currentShineInstances.Count > 0)
        {
            float pulse = enablePulse
                ? 1f + shinePulseAmplitude * Mathf.Sin((Time.time * shinePulseFrequency) * Mathf.PI * 2f)
                : 1f;

            for (int i = 0; i < currentShineInstances.Count; i++)
            {
                GameObject shineInstance = currentShineInstances[i];
                if (shineInstance == null)
                {
                    continue;
                }

                shineInstance.transform.localPosition = i == 0 ? shineLocalOffset : additionalShineLocalOffsets[i - 1];
                shineInstance.transform.localScale = originalShineScale * pulse;
            }

            UpdateShineEmission(pulse, GetEmissionColor(currentColor ?? MagicColor.White));
        }

        if (billboardToCamera && currentShineInstances.Count > 0)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                foreach (GameObject shineInstance in currentShineInstances)
                {
                    if (shineInstance == null)
                    {
                        continue;
                    }

                    shineInstance.transform.rotation = Quaternion.LookRotation(shineInstance.transform.position - mainCamera.transform.position);
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

        return MagicColor.White;
    }

    private void UpdateShineInstance()
    {
        DestroyCurrentShine();

        MagicColor color = ResolveEnemyColor();
        GameObject prefab = color == MagicColor.White ? whiteShinePrefab : blackShinePrefab;
        if (prefab == null)
        {
            return;
        }

        if (autoComputeShineOffset)
        {
            shineLocalOffset = GetAutoShineLocalOffset();
        }

        CreateShineInstance(prefab, shineLocalOffset, color);
        for (int i = 0; i < additionalShineLocalOffsets.Count; i++)
        {
            CreateShineInstance(prefab, additionalShineLocalOffsets[i], color);
        }

        if (currentShineInstances.Count > 0)
        {
            if (useShineScaleMultiplier)
            {
                originalShineScale = shineCustomScale;
            }
            else
            {
                originalShineScale = currentShineInstances[0].transform.localScale;
            }
        }

        float initialPulse = enablePulse
            ? 1f + shinePulseAmplitude * Mathf.Sin((Time.time * shinePulseFrequency) * Mathf.PI * 2f)
            : 1f;
        UpdateShineEmission(initialPulse, GetEmissionColor(color));
        currentColor = color;
    }

    private void CreateShineInstance(GameObject prefab, Vector3 localOffset, MagicColor color)
    {
        Vector3 worldPos = transform.TransformPoint(localOffset);
        GameObject shineInstance = Instantiate(prefab, worldPos, Quaternion.identity, transform);
        if (preserveLocalRotation)
        {
            shineInstance.transform.localRotation = Quaternion.identity;
        }


        shineInstance.transform.localPosition = localOffset;
        shineInstance.name = "EnemyShine";
        currentShineInstances.Add(shineInstance);

        ShineData data = CacheShineRenderers(shineInstance);
        currentShineData.Add(data);
    }

    private ShineData CacheShineRenderers(GameObject shine)
    {
        ShineData data = new ShineData
        {
            Renderer = shine.GetComponentInChildren<Renderer>(),
            SpriteRenderer = shine.GetComponentInChildren<SpriteRenderer>(),
            Material = null,
            OriginalMaterialColor = Color.white,
            OriginalSpriteColor = Color.white
        };

        if (data.Renderer != null)
        {
            data.Material = data.Renderer.material;
            data.Renderer.shadowCastingMode = ShadowCastingMode.Off;
            data.Renderer.receiveShadows = false;
            data.Renderer.enabled = true;
            if (data.Material.HasProperty("_Color"))
            {
                data.OriginalMaterialColor = data.Material.GetColor("_Color");
            }
            if (data.Material != null)
            {
                data.Material.renderQueue = Mathf.Max(data.Material.renderQueue, 3000);
                if (data.OriginalMaterialColor.a <= 0f)
                {
                    data.OriginalMaterialColor.a = 1f;
                }
            }
        }

        if (data.SpriteRenderer != null)
        {
            data.SpriteRenderer.receiveShadows = false;
            data.SpriteRenderer.shadowCastingMode = ShadowCastingMode.Off;
            data.SpriteRenderer.enabled = true;
            try
            {
                data.SpriteRenderer.sortingOrder = 32767;
            }
            catch { }
            data.OriginalSpriteColor = data.SpriteRenderer.color;
            if (data.Material == null)
            {
                data.Material = data.SpriteRenderer.material;
            }
        }

        if (debugShine)
        {
            string rName = data.Renderer != null ? data.Renderer.GetType().Name : "-";
            string matName = data.Material != null ? data.Material.shader.name : "-";
            int rq = data.Material != null ? data.Material.renderQueue : -1;
            bool hasColor = data.Material != null && (data.Material.HasProperty("_Color") || data.Material.HasProperty("_BaseColor") || data.Material.HasProperty("_EmissionColor"));
            Debug.Log($"[EnemyShine] Created shine '{shine.name}' | Renderer:{rName} Enabled:{(data.Renderer!=null?data.Renderer.enabled:false)} Shader:'{matName}' RQ:{rq} HasColorProps:{hasColor} OriginalColor.a:{data.OriginalMaterialColor.a}");
            if (data.SpriteRenderer != null)
            {
                Debug.Log($"[EnemyShine] SpriteRenderer enabled:{data.SpriteRenderer.enabled} sortingLayer:{data.SpriteRenderer.sortingLayerName} order:{data.SpriteRenderer.sortingOrder} color.a:{data.OriginalSpriteColor.a}");
            }
            Debug.Log($"[EnemyShine] Transform localPos:{shine.transform.localPosition} localScale:{shine.transform.localScale} worldPos:{shine.transform.position}");
        }

        return data;
    }

    private Vector3 GetAutoShineLocalOffset()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers == null || renderers.Length == 0)
        {
            return shineLocalOffset;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        Vector3 topCenter = bounds.center + Vector3.up * bounds.extents.y;
        return transform.InverseTransformPoint(topCenter);
    }

    private void UpdateShineEmission(float pulseValue, Color emissionColor)
    {
        float emissionStrength = Mathf.Max(0f, pulseValue) * shineEmissionIntensity;

        for (int i = 0; i < currentShineData.Count; i++)
        {
            ShineData data = currentShineData[i];
            if (data.Material != null)
            {
                Color targetColor = emissionColor * emissionStrength;
                if (data.Material.HasProperty("_BaseColor"))
                {
                    data.Material.SetColor("_BaseColor", targetColor);
                }
                else if (data.Material.HasProperty("_Color"))
                {
                    data.Material.SetColor("_Color", targetColor);
                }
            }

            if (data.SpriteRenderer != null)
            {
                Color spriteColor = emissionColor * emissionStrength;
                spriteColor.a = data.OriginalSpriteColor.a;
                data.SpriteRenderer.color = spriteColor;
            }
        }
    }

    private Color GetEmissionColor(MagicColor color)
    {
        return color == MagicColor.White ? whiteShineEmissionColor : blackShineEmissionColor;
    }

    private void DestroyCurrentShine()
    {
        for (int i = 0; i < currentShineInstances.Count; i++)
        {
            if (currentShineInstances[i] != null)
            {
                Destroy(currentShineInstances[i]);
            }
        }

        currentShineInstances.Clear();
        currentShineData.Clear();
    }

    private void OnDestroy()
    {
        DestroyCurrentShine();
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmo)
        {
            return;
        }

        Gizmos.color = gizmoColor;
        Vector3 shinePosition = transform.TransformPoint(shineLocalOffset);
        Gizmos.DrawSphere(shinePosition, 0.1f);
        Gizmos.DrawLine(transform.position, shinePosition);

        for (int i = 0; i < additionalShineLocalOffsets.Count; i++)
        {
            Vector3 additionalPosition = transform.TransformPoint(additionalShineLocalOffsets[i]);
            Gizmos.DrawSphere(additionalPosition, 0.1f);
            Gizmos.DrawLine(transform.position, additionalPosition);
        }
    }
}
