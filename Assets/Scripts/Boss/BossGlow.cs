using UnityEngine;

[DisallowMultipleComponent]
public class BossGlow : MonoBehaviour
{
    [Header("Outline Colors")]
    [Tooltip("Color del outline para jefes negros (Morado).")]
    [SerializeField] private Color blackOutlineColor = new Color(0.8f, 0.3f, 1f, 1f);

    [Tooltip("Color del outline para jefes blancos (Amarillo clarito).")]
    [SerializeField] private Color whiteOutlineColor = new Color(1f, 0.95f, 0.4f, 1f);

    [Header("Outline Settings")]
    [Tooltip("Ancho del contorno (0.01 - 0.3 recomendado).")]
    [SerializeField] private float outlineWidth = 0.08f;

    [Tooltip("Shader a usar para el outline (arrastrar desde Assets/Shaders/EnemyOutline.shader).")]
    [SerializeField] private Shader outlineShader;

    // Compatibilidad con prefabs existentes
    [SerializeField, HideInInspector]
    private string outlineShaderName = "Custom/EnemyOutline";

    [Header("Model References")]
    [Tooltip("Referencia al modelo blanco del jefe.")]
    [SerializeField] private Transform whiteModelTransform;

    [Tooltip("Referencia al modelo negro del jefe.")]
    [SerializeField] private Transform blackModelTransform;

    [Tooltip("Referencia al hitbox del jefe si usa EnemyHealth.")]
    [SerializeField] private Transform hitboxTransform;

    [Tooltip("Color del outline cuando el jefe está vulnerable.")]
    [SerializeField] private Color vulnerableOutlineColor = new Color(1f, 0.3f, 0.3f, 1f);

    private EnemyHealth enemyHealth;
    private PawnBossHealth pawnBossHealth;
    private Renderer[] bodyRenderers;
    private Material[] outlineMaterials;
    private MagicColor cachedColor;
    private bool initialized = false;

    private void Awake()
    {
        pawnBossHealth = GetComponentInChildren<PawnBossHealth>();

        if (pawnBossHealth == null)
        {
            enemyHealth = hitboxTransform != null ? hitboxTransform.GetComponent<EnemyHealth>() : GetComponentInChildren<EnemyHealth>();
        }

        // Default colors for bosses without health components
        if (enemyHealth == null && pawnBossHealth == null)
        {
            if (GetComponent<BossKnight>() != null)
                cachedColor = MagicColor.Black;
            else if (GetComponent<RookBossController>() != null)
                cachedColor = MagicColor.Black;
            else if (GetComponent<BishopBossController>() != null)
                cachedColor = MagicColor.White;
            else
            {
                Debug.LogWarning($"[BossGlow] No EnemyHealth ni PawnBossHealth encontrado en {gameObject.name}.");
                enabled = false;
                return;
            }
        }

        // Usar shader asignado desde el inspector (garantizado en build)
        // Fallback: buscar en caso de no estar asignado
        if (outlineShader == null)
        {
            outlineShader = Shader.Find(outlineShaderName);
        }
        
        // Fallback: cargar desde Resources (garantizado en build)
        if (outlineShader == null)
        {
            Material outlineMat = Resources.Load<Material>("EnemyOutlineMat");
            if (outlineMat != null) outlineShader = outlineMat.shader;
        }
        
        if (outlineShader == null)
        {
            Debug.LogWarning($"[BossGlow] Shader no asignado. Asignar desde Assets/Shaders/EnemyOutline.shader.");
            enabled = false;
            return;
        }
    }

    private void Start()
    {
        InitializeOutline();
    }

    private void InitializeOutline()
    {
        if (initialized) return;
        initialized = true;

        FindModelTransforms();

        MagicColor color = GetBossColor();
        cachedColor = color;

        FindRenderersForModel(color);
        if (bodyRenderers.Length > 0)
        {
            ApplyOutline(color);
        }
    }

    private void FindModelTransforms()
    {
        if (whiteModelTransform == null)
        {
            whiteModelTransform = FindDeepChild(transform, "ModelWhite");
            if (whiteModelTransform == null) whiteModelTransform = FindDeepChild(transform, "modelWhite");
        }
        if (blackModelTransform == null)
        {
            blackModelTransform = FindDeepChild(transform, "ModelBlack");
            if (blackModelTransform == null) blackModelTransform = FindDeepChild(transform, "modelBlack");
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

    private MagicColor GetBossColor()
    {
        if (pawnBossHealth != null) return pawnBossHealth.myColor;
        if (enemyHealth != null && GetComponent<RookBossController>() != null) return MagicColor.Black;
        if (enemyHealth != null) return enemyHealth.myColor;
        return cachedColor;
    }

    private void FindRenderersForModel(MagicColor color)
    {
        Transform targetModel = color == MagicColor.White ? whiteModelTransform : blackModelTransform;

        // Rook/Knight fallback: use black model when white unavailable
        if (targetModel == null && (GetComponent<BossKnight>() != null || GetComponent<RookBossController>() != null) && blackModelTransform != null)
        {
            targetModel = blackModelTransform;
        }
        // Bishop fallback: use white model when black unavailable
        else if (targetModel == null && GetComponent<BishopBossController>() != null && whiteModelTransform != null)
        {
            targetModel = whiteModelTransform;
        }
        else if (targetModel == null)
        {
            targetModel = transform;
        }

        Transform unionTransform = FindDeepChild(targetModel, "pasted__Pawn_Union");
        if (unionTransform == null) unionTransform = FindDeepChild(targetModel, "Pawn_Union");

        if (unionTransform != null)
        {
            bodyRenderers = new Renderer[] { unionTransform.GetComponent<Renderer>() };
            if (bodyRenderers[0] == null)
            {
                bodyRenderers = unionTransform.GetComponentsInChildren<Renderer>();
            }
        }
        else
        {
            bodyRenderers = targetModel.GetComponentsInChildren<Renderer>(true);
        }

        outlineMaterials = new Material[bodyRenderers.Length];
        for (int i = 0; i < bodyRenderers.Length; i++)
        {
            outlineMaterials[i] = new Material(outlineShader);
        }
    }

    private void ApplyOutline(MagicColor color)
    {
        Color targetOutlineColor = color == MagicColor.White ? whiteOutlineColor : blackOutlineColor;
        if (IsVulnerable())
        {
            targetOutlineColor = vulnerableOutlineColor;
        }

        for (int i = 0; i < bodyRenderers.Length; i++)
        {
            if (bodyRenderers[i] == null) continue;
            Material[] newMaterials = new Material[bodyRenderers[i].materials.Length + 1];
            System.Array.Copy(bodyRenderers[i].materials, newMaterials, bodyRenderers[i].materials.Length);
            outlineMaterials[i].SetColor("_OutlineColor", targetOutlineColor);
            outlineMaterials[i].SetFloat("_OutlineWidth", outlineWidth);
            newMaterials[newMaterials.Length - 1] = outlineMaterials[i];
            bodyRenderers[i].materials = newMaterials;
        }
    }

    private bool IsVulnerable()
    {
        if (pawnBossHealth != null)
        {
            PawnBossController controller = GetComponent<PawnBossController>();
            if (controller != null) return controller.IsFatigued();
        }
        if (enemyHealth != null && GetComponent<RookBossController>() != null)
        {
            return !enemyHealth.isShielded;
        }
        return false;
    }

    private void OnDestroy()
    {
        if (outlineMaterials != null)
        {
            foreach (Material mat in outlineMaterials)
            {
                if (mat != null) DestroyImmediate(mat);
            }
        }
    }
}