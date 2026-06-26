using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeTransition : MonoBehaviour
{
    public static FadeTransition Instance { get; private set; }

    private Image fadeImage;
    private Canvas canvas;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupCanvasAndImage();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetupCanvasAndImage()
    {
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        GameObject panel = new GameObject("FadePanel");
        panel.transform.SetParent(canvas.transform, false);
        fadeImage = panel.AddComponent<Image>();

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        fadeImage.color = Color.clear;
        fadeImage.raycastTarget = false;
    }

    public Coroutine FadeToBlack(float duration)
    {
        return StartCoroutine(Fade(Color.black, duration));
    }

    public Coroutine FadeFromBlack(float duration)
    {
        return StartCoroutine(Fade(Color.clear, duration));
    }

    private IEnumerator Fade(Color targetColor, float duration)
    {
        fadeImage.raycastTarget = true;
        Color startColor = fadeImage.color;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            fadeImage.color = Color.Lerp(startColor, targetColor, timer / duration);
            yield return null;
        }

        fadeImage.color = targetColor;
        if (targetColor == Color.clear)
        {
            fadeImage.raycastTarget = false;
        }
    }
}