using UnityEngine;

/// <summary>
/// Script que añade un contorno de luz alrededor del enemigo sin cambiar su color base.
/// Usa un shader de outline que dibuja solo los bordes del modelo.
/// Black enemies: Contorno morado, White enemies: Contorno amarillo clarito.
/// </summary>
[DisallowMultipleComponent]
public class EnemyGlow : MonoBehaviour
{
    [Header("Outline Colors")]
    [Tooltip("Color del outline para enemigos negros (Morado).")]
    [SerializeField] private Color blackOutlineColor = new Color(0.8f, 0.3f, 1f, 1f);

    [Tooltip("Color del outline para enemigos blancos (Amarillo clarito).")]
    [SerializeField] private Color whiteOutlineColor = new Color(1f, 0.95f, 0.4f, 1f);

    [Header("Outline Settings")]
    [Tooltip("Ancho del contorno (0.01 - 0.3 recomendado).")]
    [SerializeField] private float outlineWidth = 0.08f;

    [Tooltip("Shader a usar para el outline (arrastrar desde Assets/Shaders/EnemyOutline.shader).")]
    [SerializeField] private Shader outlineShader;

    // Compatibilidad con prefabs existentes
    [SerializeField, HideInInspector]
    private string outlineShaderName = "Custom/EnemyOutline";

    private EnemyHealth enemyHealth;
    private Renderer[] bodyRenderers;
    private Material[] outlineMaterials;
    private MagicColor? cachedColor;

    private void Awake()
    {
        enemyHealth = GetComponentInChildren<EnemyHealth>();
        if (enemyHealth == null)
        {
            Debug.LogWarning($"[EnemyGlow] No EnemyHealth encontrado en {gameObject.name}.");
            enabled = false;
            return;
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
            Debug.LogWarning($"[EnemyGlow] Shader no asignado. Asignar desde Assets/Shaders/EnemyOutline.shader.");
            enabled = false;
            return;
        }

        // Obtener todos los renderers excepto los del "EnemyShine"
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        System.Collections.Generic.List<Renderer> bodyRenderersList = new System.Collections.Generic.List<Renderer>();

        foreach (Renderer renderer in allRenderers)
        {
            // Excluir renderers que sean parte del shine
            if (!renderer.gameObject.name.Contains("Shine") && renderer.GetComponent<EnemyShine>() == null)
            {
                bodyRenderersList.Add(renderer);
            }
        }

        bodyRenderers = bodyRenderersList.ToArray();

        // Crear materiales de outline para cada renderer
        outlineMaterials = new Material[bodyRenderers.Length];
        for (int i = 0; i < bodyRenderers.Length; i++)
        {
            outlineMaterials[i] = new Material(outlineShader);
        }

        // Aplicar el outline inicial
        UpdateOutline();
    }

    private void Update()
    {
        // Detectar cambio de color
        MagicColor currentColor = enemyHealth.myColor;
        if (!cachedColor.HasValue || cachedColor.Value != currentColor)
        {
            UpdateOutline();
        }
    }

    private void UpdateOutline()
    {
        cachedColor = enemyHealth.myColor;
        Color targetOutlineColor = cachedColor == MagicColor.White ? whiteOutlineColor : blackOutlineColor;

        for (int i = 0; i < bodyRenderers.Length; i++)
        {
            if (bodyRenderers[i] == null) continue;

            // Agregar material de outline
            Material[] newMaterials = new Material[bodyRenderers[i].materials.Length + 1];
            System.Array.Copy(bodyRenderers[i].materials, newMaterials, bodyRenderers[i].materials.Length);
            
            // El último material será el outline
            outlineMaterials[i].SetColor("_OutlineColor", targetOutlineColor);
            outlineMaterials[i].SetFloat("_OutlineWidth", outlineWidth);
            newMaterials[newMaterials.Length - 1] = outlineMaterials[i];
            
            bodyRenderers[i].materials = newMaterials;
        }
    }

    private void OnDestroy()
    {
        // Limpiar materiales de outline
        if (outlineMaterials != null)
        {
            foreach (Material mat in outlineMaterials)
            {
                if (mat != null)
                {
                    DestroyImmediate(mat);
                }
            }
        }
    }
}
