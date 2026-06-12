using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class NewCreditsCanvasFix : MonoBehaviour
{
    [Header("Exit Button (PC only)")]
    [SerializeField] private Button exitPCButton;

    [Header("Manual UI Elements")]
    [SerializeField] private List<GameObject> uiElementsToReposition = new List<GameObject>();

    [Header("Dynamic Creation")]
    [SerializeField] private bool createExitButtonIfNotAssigned = true;

    private Canvas creditsCanvas;
    private RectTransform canvasRect;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        InitializeCanvas();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "NewCredits")
        {
            ForceRefreshAllCanvases();
            RepositionUIElements();
        }
    }

    private void Start()
    {
        ForceRefreshAllCanvases();
        if (createExitButtonIfNotAssigned && exitPCButton == null && !IsWebGLBuild())
        {
            CreateExitButton();
        }
        SetupExitButton();
        RepositionUIElements();
    }

    private void OnEnable()
    {
        ForceRefreshAllCanvases();
        SetupExitButton();
        RepositionUIElements();
    }

    private void OnRectTransformDimensionsChange()
    {
        ForceRefreshAllCanvases();
        RepositionUIElements();
    }

    private void InitializeCanvas()
    {
        canvasRect = GetComponent<RectTransform>();
        if (canvasRect == null)
        {
            canvasRect = gameObject.AddComponent<RectTransform>();
        }
        canvasRect.anchorMin = Vector2.zero;
        canvasRect.anchorMax = Vector2.one;
        canvasRect.offsetMin = Vector2.zero;
        canvasRect.offsetMax = Vector2.zero;

        creditsCanvas = GetComponent<Canvas>();
        if (creditsCanvas == null)
        {
            creditsCanvas = gameObject.AddComponent<Canvas>();
        }
        creditsCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        creditsCanvas.pixelPerfect = false;
        creditsCanvas.overrideSorting = true;
        creditsCanvas.sortingOrder = 10;

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = gameObject.AddComponent<CanvasScaler>();
        }
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GraphicRaycaster raycaster = GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    private void RepositionUIElements()
    {
        if (canvasRect == null) return;
        
        Canvas.ForceUpdateCanvases();
        
        foreach (var element in uiElementsToReposition)
        {
            if (element != null)
            {
                var rect = element.GetComponent<RectTransform>();
                if (rect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
                }
            }
        }
    }

    private void SetupExitButton()
    {
        bool isPCBuild = !IsWebGLBuild();
        if (exitPCButton != null)
        {
            exitPCButton.gameObject.SetActive(isPCBuild);
            if (isPCBuild && exitPCButton.onClick.GetPersistentEventCount() == 0)
            {
                exitPCButton.onClick.AddListener(OnExitPCButton);
            }
        }
    }

    private void CreateExitButton()
    {
        GameObject exitGO = new GameObject("ExitPCButton");
        exitGO.transform.SetParent(transform, false);
        exitGO.tag = "ExitButton";
        
        var exitRect = exitGO.AddComponent<RectTransform>();
        exitRect.anchorMin = new Vector2(1, 0);
        exitRect.anchorMax = new Vector2(1, 0);
        exitRect.pivot = new Vector2(1, 0);
        exitRect.anchoredPosition = new Vector2(-20, 20);
        exitRect.sizeDelta = new Vector2(200, 60);

        var exitImage = exitGO.AddComponent<Image>();
        exitImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        var exitBtn = exitGO.AddComponent<Button>();
        exitPCButton = exitBtn;

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(exitGO.transform, false);
        var textRect = textGO.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var tmpText = textGO.AddComponent<TextMeshProUGUI>();
        tmpText.text = "SALIR";
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.color = Color.white;
        tmpText.fontSize = 24;
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(exitRect);
    }

    private static void ForceRefreshAllCanvases()
    {
        Canvas[] canvases = Object.FindObjectsOfType<Canvas>();
        foreach (var canvas in canvases)
        {
            canvas.enabled = false;
            canvas.enabled = true;
        }
        Canvas.ForceUpdateCanvases();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToSelector();
        }
    }

    private void OnExitPCButton()
    {
        Application.Quit();
    }

    private void ReturnToSelector()
    {
        Time.timeScale = 1f;
        CheckerboardTransition.directToMenu = true;
        SceneManager.LoadScene("NewTitleScreen");
    }

    private static bool IsWebGLBuild()
    {
#if UNITY_WEBGL
        return true;
#else
        return false;
#endif
    }
}