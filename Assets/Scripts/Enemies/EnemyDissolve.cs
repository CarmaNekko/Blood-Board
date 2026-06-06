using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyDissolve : MonoBehaviour
{
    [Header("Dissolve Settings")]
    public Material dissolveMaterial;
    public float dissolveDuration = 1.0f;
    [SerializeField] private float dissolveEdgeWidth = 0.08f;
    [SerializeField] private float dissolveSoftness = 0.04f;
    [SerializeField] private float finalFadeStart = 0.75f;
    [SerializeField] private AnimationCurve dissolveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Dissolve Colors")]
    [Tooltip("Color del borde de la disolucion para enemigos blancos (amarillento).")]
    [SerializeField] private Color whiteDissolveColor = new Color(1f, 0.95f, 0.7f, 1f);

    [Tooltip("Color del borde de la disolucion para enemigos negros (morado).")]
    [SerializeField] private Color blackDissolveColor = new Color(0.28f, 0.06f, 0.55f, 1f);

    [Header("Magic Effect")]
    [SerializeField] private float whiteMagicIntensity = 1.8f;
    [SerializeField] private float blackMagicIntensity = 0.85f;
    [SerializeField] private float whiteBaseBrightness = 1f;
    [SerializeField] private float blackBaseBrightness = 0.42f;
    [SerializeField] private bool enableMagicPulse = true;
    [SerializeField] private float magicPulseFrequency = 2.2f;
    [SerializeField] private float magicPulseAmplitude = 0.25f;
    [SerializeField] private string outlineShaderName = "Custom/EnemyOutline";

    private EnemyHealth enemyHealth;
    private Renderer[] bodyRenderers;
    private Renderer[] shineRenderers;
    private NavMeshAgent agent;
    private Collider[] allColliders;
    private Rigidbody rb;
    private readonly List<Material> dissolveMaterials = new List<Material>();
    private float activeMagicIntensity = 1f;
    private bool isDissolving;

    private void Awake()
    {
        enemyHealth = GetComponentInChildren<EnemyHealth>();
        agent = GetComponent<NavMeshAgent>();
        allColliders = GetComponentsInChildren<Collider>();
        rb = GetComponent<Rigidbody>();

        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        List<Renderer> bodyRenderersList = new List<Renderer>();
        List<Renderer> shineRenderersList = new List<Renderer>();

        foreach (Renderer renderer in allRenderers)
        {
            if (!IsShineRenderer(renderer))
            {
                bodyRenderersList.Add(renderer);
            }
            else
            {
                shineRenderersList.Add(renderer);
            }
        }

        bodyRenderers = bodyRenderersList.ToArray();
        shineRenderers = shineRenderersList.ToArray();
    }

    public void TriggerDeath()
    {
        if (isDissolving)
        {
            return;
        }

        isDissolving = true;
        MagicColor deathColor = enemyHealth != null ? enemyHealth.myColor : MagicColor.White;

        foreach (Collider col in allColliders)
        {
            if (col != null)
            {
                col.enabled = false;
            }
        }

        if (agent != null)
        {
            agent.enabled = false;
        }

        EnemyPawn enemyPawn = GetComponent<EnemyPawn>();
        if (enemyPawn != null)
        {
            enemyPawn.enabled = false;
        }

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        DisablePersistentEnemyEffects();

        if (enemyHealth != null)
        {
            enemyHealth.enabled = false;
        }

        if (dissolveMaterial == null)
        {
            Destroy(gameObject);
            return;
        }

        bool isWhiteEnemy = deathColor == MagicColor.White;
        Color dissolveEdgeColor = isWhiteEnemy ? whiteDissolveColor : blackDissolveColor;
        float baseBrightness = isWhiteEnemy ? whiteBaseBrightness : blackBaseBrightness;
        activeMagicIntensity = isWhiteEnemy ? whiteMagicIntensity : blackMagicIntensity;

        PrepareDissolveMaterials(dissolveEdgeColor, baseBrightness);
        StartCoroutine(DissolveRoutine());
    }

    private void PrepareDissolveMaterials(Color dissolveEdgeColor, float baseBrightness)
    {
        dissolveMaterials.Clear();

        foreach (Renderer renderer in bodyRenderers)
        {
            if (renderer == null)
            {
                continue;
            }

            Material[] originalMaterials = renderer.sharedMaterials;
            List<Material> newMaterials = new List<Material>(originalMaterials.Length);

            foreach (Material original in originalMaterials)
            {
                if (original == null || IsOutlineMaterial(original))
                {
                    continue;
                }

                Material mat = new Material(dissolveMaterial);
                CopyBaseVisualProperties(original, mat);

                mat.SetColor("_DissolveEdgeColor", dissolveEdgeColor);
                mat.SetFloat("_DissolveAmount", 0f);
                mat.SetFloat("_DissolveEdgeWidth", dissolveEdgeWidth);
                mat.SetFloat("_DissolveSoftness", dissolveSoftness);
                mat.SetFloat("_FinalFadeStart", finalFadeStart);
                mat.SetFloat("_MagicIntensity", activeMagicIntensity);
                mat.SetFloat("_BaseBrightness", baseBrightness);
                mat.SetFloat("_UseProceduralNoise", dissolveMaterial.GetTexture("_DissolveNoiseTex") == null ? 1f : 0f);

                newMaterials.Add(mat);
                dissolveMaterials.Add(mat);
            }

            renderer.materials = newMaterials.ToArray();
        }
    }

    private void CopyBaseVisualProperties(Material original, Material target)
    {
        if (original.HasProperty("_MainTex") && target.HasProperty("_MainTex"))
        {
            target.SetTexture("_MainTex", original.GetTexture("_MainTex"));
            target.SetTextureScale("_MainTex", original.GetTextureScale("_MainTex"));
            target.SetTextureOffset("_MainTex", original.GetTextureOffset("_MainTex"));
        }
        else if (original.HasProperty("_BaseMap") && target.HasProperty("_MainTex"))
        {
            target.SetTexture("_MainTex", original.GetTexture("_BaseMap"));
            target.SetTextureScale("_MainTex", original.GetTextureScale("_BaseMap"));
            target.SetTextureOffset("_MainTex", original.GetTextureOffset("_BaseMap"));
        }

        if (target.HasProperty("_Color"))
        {
            if (original.HasProperty("_Color"))
            {
                target.SetColor("_Color", original.GetColor("_Color"));
            }
            else if (original.HasProperty("_BaseColor"))
            {
                target.SetColor("_Color", original.GetColor("_BaseColor"));
            }
        }
    }

    private bool IsOutlineMaterial(Material material)
    {
        return material.shader != null && material.shader.name == outlineShaderName;
    }

    private bool IsShineRenderer(Renderer renderer)
    {
        if (renderer == null)
        {
            return false;
        }

        Transform current = renderer.transform;
        while (current != null && current != transform)
        {
            if (current.name.Contains("Shine") || current.GetComponent<EnemyShine>() != null)
            {
                return true;
            }

            current = current.parent;
        }

        return renderer.name.Contains("Shine") || renderer.GetComponent<EnemyShine>() != null;
    }

    private void DisablePersistentEnemyEffects()
    {
        EnemyGlow glow = GetComponentInChildren<EnemyGlow>();
        if (glow != null)
        {
            glow.enabled = false;
        }

        EnemyShine shine = GetComponentInChildren<EnemyShine>();
        if (shine != null)
        {
            shine.enabled = false;
        }

        foreach (Renderer shineRenderer in shineRenderers)
        {
            if (shineRenderer != null)
            {
                shineRenderer.enabled = false;
            }
        }
    }

    private IEnumerator DissolveRoutine()
    {
        float elapsed = 0f;

        while (elapsed < dissolveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dissolveDuration);
            t = dissolveCurve != null ? dissolveCurve.Evaluate(t) : t;

            float pulse = enableMagicPulse
                ? 1f + magicPulseAmplitude * Mathf.Sin((Time.time * magicPulseFrequency) * Mathf.PI * 2f)
                : 1f;

            foreach (Material mat in dissolveMaterials)
            {
                if (mat != null)
                {
                    mat.SetFloat("_DissolveAmount", t);
                    mat.SetFloat("_MagicIntensity", activeMagicIntensity * Mathf.Max(0f, pulse));
                }
            }

            yield return null;
        }

        foreach (Material mat in dissolveMaterials)
        {
            if (mat != null)
            {
                Destroy(mat);
            }
        }

        dissolveMaterials.Clear();
        Destroy(gameObject);
    }
}
