using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventAnnouncementUI : MonoBehaviour
{
    public static EventAnnouncementUI Instance { get; private set; }

    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI subtitleText;
    private TextMeshProUGUI timerText;
    private Coroutine activeRoutine;

    public static void ShowMessage(string title, string subtitle, float duration)
    {
        EventAnnouncementUI ui = GetOrCreate();
        ui.PlayMessage(title, subtitle, duration);
    }

    public static IEnumerator ShowCountdown(string title, string subtitle, float duration)
    {
        EventAnnouncementUI ui = GetOrCreate();
        yield return ui.PlayCountdown(title, subtitle, duration);
    }

    private static EventAnnouncementUI GetOrCreate()
    {
        if (Instance != null)
        {
            return Instance;
        }

        GameObject uiObject = new GameObject("Event Announcement UI");
        EventAnnouncementUI ui = uiObject.AddComponent<EventAnnouncementUI>();
        DontDestroyOnLoad(uiObject);
        return ui;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUi();
        HideImmediate();
    }

    private void PlayMessage(string title, string subtitle, float duration)
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
        }

        activeRoutine = StartCoroutine(MessageRoutine(title, subtitle, duration));
    }

    private IEnumerator PlayCountdown(string title, string subtitle, float duration)
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }

        SetText(title, subtitle, Mathf.CeilToInt(duration));
        Show();

        float remaining = duration;
        while (remaining > 0f)
        {
            SetTimer(Mathf.CeilToInt(remaining));
            remaining -= Time.deltaTime;
            yield return null;
        }

        SetTimer(0);
        yield return new WaitForSeconds(0.35f);
        HideImmediate();
    }

    private IEnumerator MessageRoutine(string title, string subtitle, float duration)
    {
        SetText(title, subtitle, -1);
        Show();
        yield return new WaitForSeconds(duration);
        HideImmediate();
        activeRoutine = null;
    }

    private void BuildUi()
    {
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        GameObject panelObject = new GameObject("EventPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        panelObject.transform.SetParent(transform, false);

        RectTransform panel = panelObject.GetComponent<RectTransform>();
        panel.anchorMin = new Vector2(0.5f, 1f);
        panel.anchorMax = new Vector2(0.5f, 1f);
        panel.pivot = new Vector2(0.5f, 1f);
        panel.anchoredPosition = new Vector2(0f, -28f);
        panel.sizeDelta = new Vector2(720f, 96f);

        Image panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color(0.03f, 0.02f, 0.025f, 0.78f);
        panelImage.raycastTarget = false;

        canvasGroup = panelObject.GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        titleText = CreateText(panel, "Title", new Vector2(0f, -14f), 28, FontStyles.Bold);
        subtitleText = CreateText(panel, "Subtitle", new Vector2(0f, -48f), 18, FontStyles.Normal);
        timerText = CreateText(panel, "Timer", new Vector2(0f, -74f), 22, FontStyles.Bold);
    }

    private TextMeshProUGUI CreateText(RectTransform parent, string objectName, Vector2 anchoredPosition, int fontSize, FontStyles style)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(-32f, 28f);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private void SetText(string title, string subtitle, int seconds)
    {
        titleText.text = title;
        subtitleText.text = subtitle;
        SetTimer(seconds);
    }

    private void SetTimer(int seconds)
    {
        if (seconds < 0)
        {
            timerText.text = string.Empty;
            return;
        }

        int minutes = seconds / 60;
        int remainingSeconds = seconds % 60;
        timerText.text = $"{minutes:00}:{remainingSeconds:00}";
    }

    private void Show()
    {
        canvas.enabled = true;
        canvasGroup.alpha = 1f;
    }

    private void HideImmediate()
    {
        if (canvas != null)
        {
            canvas.enabled = false;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }
}
