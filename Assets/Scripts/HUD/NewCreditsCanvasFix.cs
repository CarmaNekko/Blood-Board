using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class NewCreditsCanvasFix : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button exitPCButton;

    [Header("Credits")]
    [SerializeField] private TMP_Text creditsTitleText;
    [SerializeField] private TMP_Text[] creditsNameTexts;

    [Header("Dynamic Creation")]
    [SerializeField] private bool createBackButtonIfNotAssigned = true;
    [SerializeField] private bool createExitButtonIfNotAssigned = true;

    private Canvas creditsCanvas;
    private RectTransform canvasRect;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        InitializeCanvas();
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "NewCredits")
        {
            SetupButtons();
        }
    }

    private void Start()
    {
        SetupButtons();
        PopulateCredits();
    }

    private void OnEnable()
    {
        PopulateCredits();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackButton();
        }
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

        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    private void SetupButtons()
    {
        CreditsManager credits = Object.FindFirstObjectByType<CreditsManager>();
        bool inSameScene = credits != null;

        bool isPCBuild = !IsWebGLBuild();

        if (createBackButtonIfNotAssigned && backButton == null)
        {
            backButton = CreateButton("BackButton", new Vector2(200, 60), new Vector2(20, -20), new Vector2(0, 0), new Vector2(0, 0));
        }

        if (createExitButtonIfNotAssigned && exitPCButton == null && isPCBuild)
        {
            exitPCButton = CreateButton("ExitPCButton", new Vector2(200, 60), new Vector2(-20, 20), new Vector2(1, 0), new Vector2(1, 0));
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(() => OnBackButton());
        }

        if (exitPCButton != null)
        {
            exitPCButton.gameObject.SetActive(isPCBuild);
            if (isPCBuild && exitPCButton.onClick.GetPersistentEventCount() == 0)
            {
                exitPCButton.onClick.AddListener(OnExitPCButton);
            }
        }
    }

    private Button CreateButton(string buttonName, Vector2 size, Vector2 position, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject buttonGO = new GameObject(buttonName);
        buttonGO.transform.SetParent(transform, false);

        var buttonRect = buttonGO.AddComponent<RectTransform>();
        buttonRect.anchorMin = anchorMin;
        buttonRect.anchorMax = anchorMax;
        buttonRect.pivot = anchorMin;
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = size;

        var buttonImage = buttonGO.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        return buttonGO.AddComponent<Button>();
    }

    private void OnBackButton()
    {
        CreditsManager credits = Object.FindFirstObjectByType<CreditsManager>();
        if (credits != null)
        {
            credits.HideCredits();
        }
        else
        {
            CheckerboardTransition.directToMenu = true;
            SceneManager.LoadScene("NewTitleScreen");
        }
    }

    private void OnExitPCButton()
    {
        CreditsManager credits = Object.FindFirstObjectByType<CreditsManager>();
        if (credits != null)
        {
            credits.HideCredits();
        }
        else
        {
            Application.Quit();
        }
    }

    private void PopulateCredits()
    {
        if (creditsTitleText != null)
        {
            creditsTitleText.text = "CRÉDITOS";
        }
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