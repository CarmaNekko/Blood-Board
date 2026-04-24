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
}
