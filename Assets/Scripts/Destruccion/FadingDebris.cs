using UnityEngine;

public class FadingDebris : MonoBehaviour
{
    [Header("Configuración de Desvanecimiento")]
    public float timeBeforeFade = 20f;
    public float fadeDuration = 2f;
    public Material transparentMaterial;

    private float timer = 0f;
    private Material mat;
    private Renderer rend;
    private bool isFading = false;
    private bool canStartFading = false;

    private void Start()
    {
        rend = GetComponent<Renderer>();
    }

    public void BeginFadeProcess()
    {
        canStartFading = true;
    }

    private void Update()
    {
        if (!canStartFading) return;

        timer += Time.deltaTime;

        if (timer >= timeBeforeFade)
        {
            if (!isFading)
            {
                isFading = true;
                if (rend != null && transparentMaterial != null)
                {
                    rend.material = transparentMaterial;
                    mat = rend.material;
                }
            }

            float fadeRatio = (timer - timeBeforeFade) / fadeDuration;

            if (mat != null)
            {
                Color c = mat.color;
                c.a = Mathf.Lerp(1f, 0f, fadeRatio);
                mat.color = c;
            }

            if (fadeRatio >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }
}