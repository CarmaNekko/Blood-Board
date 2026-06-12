using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    public static Options Instance { get; private set; }
    public bool IsOpen { get; private set; }

    private const int WindowedWidth = 1280;
    private const int WindowedHeight = 720;
    private const string WindowedWidthPrefKey = "WindowedResolutionWidth";
    private const string WindowedHeightPrefKey = "WindowedResolutionHeight";

    private static bool SupportsScreenModeOptions
    {
        get
        {
#if UNITY_WEBGL
            return false;
#else
            return true;
#endif
        }
    }

    [Header("Referencias")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text sensitivityTitleText;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TMP_Text sensitivityLabel;
    [SerializeField] private Button backButton;

    [Header("FPS")]
    [SerializeField] private TMP_Text fpsTitleText;
    [SerializeField] private Toggle fpsToggle;
    [SerializeField] private GameObject fpsDisplay;

    [Header("Pantalla Completa")]
    [SerializeField] private TMP_Text fullscreenTitleText;
    [SerializeField] private Button fullscreenButtonOn;
    [SerializeField] private Button fullscreenButtonWindowed;

    [Header("Sensibilidad")]
    [SerializeField] private float minSensitivity = 0.1f;
    [SerializeField] private float maxSensitivity = 10f;
    [SerializeField] private float defaultSensitivity = 1f;
    [SerializeField] private float sensitivityExponent = 2f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

#if UNITY_EDITOR
        if (titleText == null) Debug.LogWarning("Options: 'titleText' no está asignado en el Inspector.", this);
        if (sensitivityTitleText == null) Debug.LogWarning("Options: 'sensitivityTitleText' no está asignado en el Inspector.", this);
        if (sensitivitySlider == null) Debug.LogWarning("Options: 'sensitivitySlider' no está asignado en el Inspector.", this);
        if (sensitivityLabel == null) Debug.LogWarning("Options: 'sensitivityLabel' no está asignado en el Inspector.", this);
        if (backButton == null) Debug.LogWarning("Options: 'backButton' no está asignado en el Inspector.", this);
        if (fpsTitleText == null) Debug.LogWarning("Options: 'fpsTitleText' no está asignado en el Inspector.", this);
        if (fpsToggle == null) Debug.LogWarning("Options: 'fpsToggle' no está asignado en el Inspector.", this);
        if (fullscreenTitleText == null) Debug.LogWarning("Options: 'fullscreenTitleText' no está asignado en el Inspector.", this);
        if (fullscreenButtonOn == null) Debug.LogWarning("Options: 'fullscreenButtonOn' no está asignado en el Inspector.", this);
        if (fullscreenButtonWindowed == null) Debug.LogWarning("Options: 'fullscreenButtonWindowed' no está asignado en el Inspector.", this);
#endif

        if (sensitivitySlider != null)
        {
            sensitivitySlider.minValue = 0f;
            sensitivitySlider.maxValue = 1f;
            sensitivitySlider.onValueChanged.AddListener(OnSensitivitySliderChanged);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(HideOptions);
        }

        if (fpsToggle != null)
        {
            fpsToggle.onValueChanged.AddListener(OnFPSToggleChanged);
        }

        if (fullscreenButtonOn != null && SupportsScreenModeOptions)
        {
            fullscreenButtonOn.onClick.AddListener(SetFullscreen);
        }

        if (fullscreenButtonWindowed != null && SupportsScreenModeOptions)
        {
            fullscreenButtonWindowed.onClick.AddListener(SetWindowed);
        }

        gameObject.SetActive(false);
    }

    private void Start()
    {
        float savedSliderValue = PlayerPrefs.GetFloat("MouseSensitivity", defaultSensitivity);
        float actualSensitivity = CalculateActualSensitivity(savedSliderValue);
        PlayerMovement.SetGlobalMouseSensitivity(actualSensitivity);

        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = savedSliderValue;
        }

        if (sensitivityTitleText != null)
        {
            sensitivityTitleText.text = "SENSIBILIDAD DE LA CÁMARA";
        }

        UpdateSensitivityLabel(savedSliderValue);

        bool showFPS = PlayerPrefs.GetInt("ShowFPS", 1) == 1;
        SetFPSVisibility(showFPS);

        if (fpsToggle != null)
        {
            fpsToggle.SetIsOnWithoutNotify(showFPS);
        }

        if (fpsTitleText != null)
        {
            fpsTitleText.text = "MOSTRAR FPS";
        }

        if (SupportsScreenModeOptions)
        {
            bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            ApplyScreenMode(isFullscreen);
        }

        if (fullscreenTitleText != null)
        {
            fullscreenTitleText.text = "MODO DE PANTALLA";
        }
    }

    private float CalculateActualSensitivity(float sliderValue)
    {
        sliderValue = Mathf.Clamp01(sliderValue);
        float normalized = Mathf.Pow(sliderValue, sensitivityExponent);
        float actual = Mathf.Lerp(minSensitivity, maxSensitivity, normalized);
        return actual;
    }

    public void OnSensitivitySliderChanged(float sliderValue)
    {
        float actualSensitivity = CalculateActualSensitivity(sliderValue);
        PlayerMovement.SetGlobalMouseSensitivity(actualSensitivity);
        PlayerPrefs.SetFloat("MouseSensitivity", sliderValue);
        PlayerPrefs.Save();
        UpdateSensitivityLabel(sliderValue);
    }

    private void UpdateSensitivityLabel(float sliderValue)
    {
        if (sensitivityLabel != null)
        {
            float actual = CalculateActualSensitivity(sliderValue);
            sensitivityLabel.text = $"{actual:F0}";
        }
    }

    private void ShowUIElement(GameObject go, bool active = true)
    {
        if (go == null)
        {
            return;
        }

        go.SetActive(active);

        if (active)
        {
            CanvasGroup cg = go.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }
    }

    public void ShowOptions()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        ShowUIElement(titleText?.gameObject);
        ShowUIElement(sensitivityTitleText?.gameObject);
        ShowUIElement(sensitivitySlider?.gameObject);
        ShowUIElement(sensitivityLabel?.gameObject);
        ShowUIElement(backButton?.gameObject);
        ShowUIElement(fpsTitleText?.gameObject);
        ShowUIElement(fpsToggle?.gameObject);
        ShowUIElement(fullscreenTitleText?.gameObject, SupportsScreenModeOptions);
        ShowUIElement(fullscreenButtonOn?.gameObject, SupportsScreenModeOptions);
        ShowUIElement(fullscreenButtonWindowed?.gameObject, SupportsScreenModeOptions);

        IsOpen = true;
    }

    public void HideOptions()
    {
        gameObject.SetActive(false);
        IsOpen = false;
    }

    public void OnFPSToggleChanged(bool isOn)
    {
        PlayerPrefs.SetInt("ShowFPS", isOn ? 1 : 0);
        PlayerPrefs.Save();
        SetFPSVisibility(isOn);
    }

    private void SetFPSVisibility(bool isVisible)
    {
        GameObject target = GetFPSDisplayObject();
        if (target == null)
        {
            return;
        }

        target.SetActive(true);

        FPSDisplay display = target.GetComponent<FPSDisplay>();
        if (display == null)
        {
            display = target.AddComponent<FPSDisplay>();
        }

        display.SetVisible(isVisible);
    }

    private GameObject GetFPSDisplayObject()
    {
        if (fpsDisplay != null)
        {
            return fpsDisplay;
        }

        if (FPSDisplay.Instance != null)
        {
            return FPSDisplay.Instance.gameObject;
        }

        GameObject foundFPS = GameObject.Find("FPS");
        if (foundFPS != null)
        {
            fpsDisplay = foundFPS;
            return fpsDisplay;
        }

        return null;
    }

    public void SetFullscreen()
    {
        if (!SupportsScreenModeOptions)
        {
            return;
        }

        CacheWindowedResolution();
        PlayerPrefs.SetInt("Fullscreen", 1);
        PlayerPrefs.Save();
        ApplyScreenMode(true);
    }

    public void SetWindowed()
    {
        if (!SupportsScreenModeOptions)
        {
            return;
        }

        PlayerPrefs.SetInt("Fullscreen", 0);
        PlayerPrefs.Save();
        ApplyScreenMode(false);
    }

    private void ApplyScreenMode(bool fullscreen)
    {
        if (fullscreen)
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
            StartCoroutine(RefreshCanvasesNextFrame());
            return;
        }

        int windowedWidth = PlayerPrefs.GetInt(WindowedWidthPrefKey, WindowedWidth);
        int windowedHeight = PlayerPrefs.GetInt(WindowedHeightPrefKey, WindowedHeight);

        Screen.fullScreenMode = FullScreenMode.Windowed;
        if (Screen.width != windowedWidth || Screen.height != windowedHeight)
        {
            Screen.SetResolution(windowedWidth, windowedHeight, FullScreenMode.Windowed);
        }
        StartCoroutine(RefreshCanvasesNextFrame());
    }

    private IEnumerator RefreshCanvasesNextFrame()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        yield return null;
        Canvas.ForceUpdateCanvases();
    }

    private void CacheWindowedResolution()
    {
        if (Screen.fullScreenMode == FullScreenMode.Windowed)
        {
            PlayerPrefs.SetInt(WindowedWidthPrefKey, Screen.width);
            PlayerPrefs.SetInt(WindowedHeightPrefKey, Screen.height);
        }
        else if (!PlayerPrefs.HasKey(WindowedWidthPrefKey) || !PlayerPrefs.HasKey(WindowedHeightPrefKey))
        {
            PlayerPrefs.SetInt(WindowedWidthPrefKey, WindowedWidth);
            PlayerPrefs.SetInt(WindowedHeightPrefKey, WindowedHeight);
        }
    }
}