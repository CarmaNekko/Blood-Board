using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    public static Options Instance { get; private set; }
    public bool IsOpen { get; private set; }
    private const int WindowedWidth = 1280;
    private const int WindowedHeight = 720;
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
    [SerializeField] private GameObject optionsBackground;
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
    [SerializeField] private float minSensitivity = 50f;
    [SerializeField] private float maxSensitivity = 400f;
    [SerializeField] private float defaultSensitivity = 200f;

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

        if (optionsBackground != null)
        {
            optionsBackground.SetActive(false);
        }

        if (titleText != null)
        {
            titleText.gameObject.SetActive(false);
        }

        if (sensitivityTitleText != null)
        {
            sensitivityTitleText.gameObject.SetActive(false);
        }

        if (sensitivitySlider != null)
        {
            sensitivitySlider.minValue = minSensitivity;
            sensitivitySlider.maxValue = maxSensitivity;
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
            sensitivitySlider.gameObject.SetActive(false);
        }

        if (sensitivityLabel != null)
        {
            sensitivityLabel.gameObject.SetActive(false);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(HideOptions);
            backButton.gameObject.SetActive(false);
        }

        if (fpsTitleText != null)
        {
            fpsTitleText.gameObject.SetActive(false);
        }

        if (fpsToggle != null)
        {
            fpsToggle.onValueChanged.AddListener(OnFPSToggleChanged);
            fpsToggle.gameObject.SetActive(false);
        }

        if (fullscreenTitleText != null)
        {
            fullscreenTitleText.gameObject.SetActive(false);
        }

        if (fullscreenButtonOn != null)
        {
            if (SupportsScreenModeOptions)
            {
                fullscreenButtonOn.onClick.AddListener(SetFullscreen);
            }
            fullscreenButtonOn.gameObject.SetActive(false);
        }

        if (fullscreenButtonWindowed != null)
        {
            if (SupportsScreenModeOptions)
            {
                fullscreenButtonWindowed.onClick.AddListener(SetWindowed);
            }
            fullscreenButtonWindowed.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", defaultSensitivity);
        PlayerMovement.SetGlobalMouseSensitivity(savedSensitivity);

        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = savedSensitivity;
        }

        if (sensitivityTitleText != null)
        {
            sensitivityTitleText.text = "Sensibilidad del Mouse";
        }

        UpdateSensitivityLabel(savedSensitivity);

        bool showFPS = PlayerPrefs.GetInt("ShowFPS", 1) == 1;
        SetFPSVisibility(showFPS);
        if (fpsToggle != null)
        {
            fpsToggle.SetIsOnWithoutNotify(showFPS);
        }
        if (fpsTitleText != null)
        {
            fpsTitleText.text = "Mostrar FPS";
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

    public void ShowOptions()
    {
        if (optionsBackground == null)
        {
            Debug.LogError("Options: no se ha asignado optionsBackground.");
            return;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        optionsBackground.SetActive(true);
        optionsBackground.transform.SetAsFirstSibling();

        if (titleText != null)
        {
            titleText.gameObject.SetActive(true);
        }

        if (sensitivityTitleText != null)
        {
            sensitivityTitleText.gameObject.SetActive(true);
        }

        if (sensitivitySlider != null)
        {
            sensitivitySlider.gameObject.SetActive(true);
        }

        if (sensitivityLabel != null)
        {
            sensitivityLabel.gameObject.SetActive(true);
        }

        if (backButton != null)
        {
            backButton.gameObject.SetActive(true);
        }

        if (fpsTitleText != null)
        {
            fpsTitleText.gameObject.SetActive(true);
        }

        if (fpsToggle != null)
        {
            ShowToggle(fpsToggle);
        }

        if (fullscreenTitleText != null)
        {
            fullscreenTitleText.gameObject.SetActive(SupportsScreenModeOptions);
        }

        if (fullscreenButtonOn != null)
        {
            fullscreenButtonOn.gameObject.SetActive(SupportsScreenModeOptions);
        }

        if (fullscreenButtonWindowed != null)
        {
            fullscreenButtonWindowed.gameObject.SetActive(SupportsScreenModeOptions);
        }

        IsOpen = true;
    }

    public void HideOptions()
    {
        if (optionsBackground != null)
        {
            optionsBackground.SetActive(false);
        }

        if (titleText != null)
        {
            titleText.gameObject.SetActive(false);
        }

        if (sensitivityTitleText != null)
        {
            sensitivityTitleText.gameObject.SetActive(false);
        }

        if (sensitivitySlider != null)
        {
            sensitivitySlider.gameObject.SetActive(false);
        }

        if (sensitivityLabel != null)
        {
            sensitivityLabel.gameObject.SetActive(false);
        }

        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
        }

        if (fpsTitleText != null)
        {
            fpsTitleText.gameObject.SetActive(false);
        }

        if (fpsToggle != null)
        {
            fpsToggle.gameObject.SetActive(false);
        }

        if (fullscreenTitleText != null)
        {
            fullscreenTitleText.gameObject.SetActive(false);
        }

        if (fullscreenButtonOn != null)
        {
            fullscreenButtonOn.gameObject.SetActive(false);
        }

        if (fullscreenButtonWindowed != null)
        {
            fullscreenButtonWindowed.gameObject.SetActive(false);
        }

        IsOpen = false;
    }

    public void OnSensitivityChanged(float value)
    {
        PlayerMovement.SetGlobalMouseSensitivity(value);
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        UpdateSensitivityLabel(value);
    }

    private void UpdateSensitivityLabel(float value)
    {
        if (sensitivityLabel != null)
        {
            sensitivityLabel.text = $"{value:0}";
        }
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

    private void ShowToggle(Toggle toggle)
    {
        toggle.gameObject.SetActive(true);
        toggle.interactable = true;
        toggle.transform.SetAsLastSibling();

        CanvasGroup canvasGroup = toggle.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (toggle.targetGraphic != null)
        {
            toggle.targetGraphic.raycastTarget = true;
        }

        if (toggle.graphic != null)
        {
            toggle.graphic.raycastTarget = true;
        }
    }

    public void SetFullscreen()
    {
        if (!SupportsScreenModeOptions)
        {
            return;
        }

        PlayerPrefs.SetInt("Fullscreen", 1);
        ApplyScreenMode(true);
    }

    public void SetWindowed()
    {
        if (!SupportsScreenModeOptions)
        {
            return;
        }

        PlayerPrefs.SetInt("Fullscreen", 0);
        ApplyScreenMode(false);
    }

    private void ApplyScreenMode(bool fullscreen)
    {
        if (fullscreen)
        {
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.FullScreenWindow);
            return;
        }

        Screen.SetResolution(WindowedWidth, WindowedHeight, FullScreenMode.Windowed);
    }
}
