using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    public static Options Instance { get; private set; }
    public bool IsOpen { get; private set; }

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

        // FPS
        if (fpsTitleText != null)
        {
            fpsTitleText.gameObject.SetActive(false);
        }

        if (fpsToggle != null)
        {
            fpsToggle.onValueChanged.AddListener(OnFPSToggleChanged);
            fpsToggle.gameObject.SetActive(false);
        }

        // Fullscreen
        if (fullscreenTitleText != null)
        {
            fullscreenTitleText.gameObject.SetActive(false);
        }

        if (fullscreenButtonOn != null)
        {
            fullscreenButtonOn.onClick.AddListener(SetFullscreen);
            fullscreenButtonOn.gameObject.SetActive(false);
        }

        if (fullscreenButtonWindowed != null)
        {
            fullscreenButtonWindowed.onClick.AddListener(SetWindowed);
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

        // FPS - Keep object active so FPSDisplay script initializes, but toggle visibility
        bool showFPS = PlayerPrefs.GetInt("ShowFPS", 0) == 1;
        if (fpsDisplay != null)
        {
            // Always keep active for script initialization
            fpsDisplay.SetActive(true);
            // Control visibility through CanvasGroup
            CanvasGroup canvasGroup = fpsDisplay.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = fpsDisplay.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = showFPS ? 1f : 0f;
            canvasGroup.blocksRaycasts = showFPS;
        }
        if (fpsToggle != null)
        {
            fpsToggle.isOn = showFPS;
        }
        if (fpsTitleText != null)
        {
            fpsTitleText.text = "Mostrar FPS";
        }

        // Fullscreen
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1; // Default to fullscreen
        Screen.fullScreen = isFullscreen;
        if (fullscreenTitleText != null)
        {
            fullscreenTitleText.text = "Pantalla Completa";
        }
    }

    public void ShowOptions()
    {
        if (optionsBackground == null)
        {
            Debug.LogError("Options: no se ha asignado optionsBackground.");
            return;
        }

        optionsBackground.SetActive(true);

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

        // FPS
        if (fpsTitleText != null)
        {
            fpsTitleText.gameObject.SetActive(true);
        }

        if (fpsToggle != null)
        {
            fpsToggle.gameObject.SetActive(true);
        }

        // Fullscreen
        if (fullscreenTitleText != null)
        {
            fullscreenTitleText.gameObject.SetActive(true);
        }

        if (fullscreenButtonOn != null)
        {
            fullscreenButtonOn.gameObject.SetActive(true);
        }

        if (fullscreenButtonWindowed != null)
        {
            fullscreenButtonWindowed.gameObject.SetActive(true);
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

        // FPS
        if (fpsTitleText != null)
        {
            fpsTitleText.gameObject.SetActive(false);
        }

        if (fpsToggle != null)
        {
            fpsToggle.gameObject.SetActive(false);
        }

        // Fullscreen
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
        if (fpsDisplay != null)
        {
            // Keep object active but control visibility
            fpsDisplay.SetActive(true);
            CanvasGroup canvasGroup = fpsDisplay.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = fpsDisplay.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = isOn ? 1f : 0f;
            canvasGroup.blocksRaycasts = isOn;
        }
    }

    public void SetFullscreen()
    {
        PlayerPrefs.SetInt("Fullscreen", 1);
        Screen.fullScreen = true;
    }

    public void SetWindowed()
    {
        PlayerPrefs.SetInt("Fullscreen", 0);
        Screen.fullScreen = false;
    }
}
