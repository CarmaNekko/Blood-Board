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

#if UNITY_EDITOR
        // Añadido para diagnosticar referencias faltantes en el Inspector.
        // Si ves un warning en la consola al iniciar, hazle clic para ver qué referencia falta.
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
            sensitivitySlider.minValue = minSensitivity;
            sensitivitySlider.maxValue = maxSensitivity;
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(HideOptions);
        }

        if (fpsToggle != null)
        {
            fpsToggle.onValueChanged.AddListener(OnFPSToggleChanged);
        }

        if (fullscreenButtonOn != null)
        {
            if (SupportsScreenModeOptions)
            {
                fullscreenButtonOn.onClick.AddListener(SetFullscreen);
            }
        }

        if (fullscreenButtonWindowed != null)
        {
            if (SupportsScreenModeOptions)
            {
                fullscreenButtonWindowed.onClick.AddListener(SetWindowed);
            }
        }

        // Desactiva el panel por defecto. Todos sus hijos (botones, textos, etc.)
        // se desactivarán con él, manteniendo su estado para cuando se reactive.
        gameObject.SetActive(false);
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
            sensitivityTitleText.text = "SENSIBILIDAD DE LA CÁMARA";
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

    public void ShowOptions()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        gameObject.SetActive(true);

        transform.SetAsLastSibling();

        // Activar explícitamente todos los elementos y sus CanvasGroups si existen,
        // para asegurar que sean visibles incluso si estaban desactivados en el editor.
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

    private void ShowUIElement(GameObject go, bool active = true)
    {
        if (go == null) return;

        go.SetActive(active);

        if (active)
        {
            // Si el elemento tiene un CanvasGroup, asegurarse de que sea visible.
            CanvasGroup cg = go.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
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
