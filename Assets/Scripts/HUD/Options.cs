using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Options : MonoBehaviour
{
    public static Options Instance { get; private set; }
    public bool IsOpen { get; private set; }

    [Header("Referencias UI")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button backButton;

    [Header("Video & Pantalla")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown screenModeDropdown;
    [SerializeField] private TMP_Text fpsTitleText;
    [SerializeField] private Toggle fpsToggle;
    [SerializeField] private GameObject fpsDisplay;
    [SerializeField] private VolumeProfile globalVolumeProfile;
    [SerializeField] private Toggle motionBlurToggle;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private TMP_Text brightnessLabel;

    [Header("Audio")]
    [SerializeField] private AudioMixer mainAudioMixer;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;

    [Header("Controles & Sensibilidad")]
    [SerializeField] private TMP_Text sensitivityTitleText;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TMP_Text sensitivityLabel;
    [SerializeField] private Toggle disableSprintFovToggle;

    [SerializeField] private float minSensitivity = 50f;
    [SerializeField] private float maxSensitivity = 400f;
    [SerializeField] private float defaultSensitivity = 200f;

    private List<Resolution> filteredResolutions;
    private static float brightnessOffset = 0f;

    public static float BrightnessOffset => brightnessOffset;

    public static void SetBrightnessOffset(float offset)
    {
        brightnessOffset = Mathf.Clamp(offset, -1f, 1f);
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (resolutionDropdown != null) resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        if (screenModeDropdown != null) screenModeDropdown.onValueChanged.AddListener(OnScreenModeChanged);
        if (fpsToggle != null) fpsToggle.onValueChanged.AddListener(OnFPSToggleChanged);
        if (motionBlurToggle != null) motionBlurToggle.onValueChanged.AddListener(OnMotionBlurChanged);

        if (sensitivitySlider != null)
        {
            sensitivitySlider.minValue = minSensitivity;
            sensitivitySlider.maxValue = maxSensitivity;
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        }
        if (disableSprintFovToggle != null) disableSprintFovToggle.onValueChanged.AddListener(OnDisableFovChanged);

        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        if (bgmVolumeSlider != null) bgmVolumeSlider.onValueChanged.AddListener(SetBGMVolume);

        if (brightnessSlider != null)
        {
            brightnessSlider.minValue = -1f;
            brightnessSlider.maxValue = 1f;
            brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
        }

        if (backButton != null) backButton.onClick.AddListener(HideOptions);

        gameObject.SetActive(false);
    }

    private void Start()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        InitializeResolutions();

        int screenModeIndex = PlayerPrefs.GetInt("ScreenMode", 0);
        if (screenModeDropdown != null) screenModeDropdown.SetValueWithoutNotify(screenModeIndex);
        ApplyScreenMode(screenModeIndex);

        float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", defaultSensitivity);
        PlayerMovement.SetGlobalMouseSensitivity(savedSensitivity);
        if (sensitivitySlider != null) sensitivitySlider.value = savedSensitivity;
        UpdateSensitivityLabel(savedSensitivity);

        bool disableFov = PlayerPrefs.GetInt("DisableSprintFOV", 0) == 1;
        if (disableSprintFovToggle != null) disableSprintFovToggle.SetIsOnWithoutNotify(disableFov);

        bool showFPS = PlayerPrefs.GetInt("ShowFPS", 1) == 1;
        SetFPSVisibility(showFPS);
        if (fpsToggle != null) fpsToggle.SetIsOnWithoutNotify(showFPS);

        bool motionBlurOn = PlayerPrefs.GetInt("MotionBlur", 1) == 1;
        if (motionBlurToggle != null) motionBlurToggle.SetIsOnWithoutNotify(motionBlurOn);
        ApplyMotionBlur(motionBlurOn);

        float brightness = PlayerPrefs.GetFloat("Brightness", 0f);
        brightnessOffset = brightness;
        if (brightnessSlider != null) brightnessSlider.SetValueWithoutNotify(brightness);
        if (brightnessLabel != null) UpdateBrightnessLabel(brightness);
        
        var lightingManager = FindAnyObjectByType<DungeonLightingManager>();
        if (lightingManager != null)
        {
            lightingManager.UpdateBrightness();
        }

        float masterVol = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);
        float bgmVol = PlayerPrefs.GetFloat("BGMVolume", 1f);

        if (masterVolumeSlider != null) masterVolumeSlider.SetValueWithoutNotify(masterVol);
        if (sfxVolumeSlider != null) sfxVolumeSlider.SetValueWithoutNotify(sfxVol);
        if (bgmVolumeSlider != null) bgmVolumeSlider.SetValueWithoutNotify(bgmVol);

        SetMasterVolume(masterVol);
        SetSFXVolume(sfxVol);
        SetBGMVolume(bgmVol);
    }

    public static void SetBrightness(float value)
    {
        brightnessOffset = value;
        PlayerPrefs.SetFloat("Brightness", value);
        PlayerPrefs.Save();
    }

    private void InitializeResolutions()
    {
        if (resolutionDropdown == null) return;

        Resolution[] allResolutions = Screen.resolutions;
        filteredResolutions = new List<Resolution>();
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < allResolutions.Length; i++)
        {
            if (i == 0 || allResolutions[i].width != allResolutions[i - 1].width || allResolutions[i].height != allResolutions[i - 1].height)
            {
                filteredResolutions.Add(allResolutions[i]);
                options.Add(allResolutions[i].width + " x " + allResolutions[i].height);

                if (allResolutions[i].width == Screen.currentResolution.width &&
                    allResolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = filteredResolutions.Count - 1;
                }
            }
        }

        resolutionDropdown.AddOptions(options);

        int savedResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);
        if (savedResolutionIndex >= 0 && savedResolutionIndex < filteredResolutions.Count)
        {
            resolutionDropdown.SetValueWithoutNotify(savedResolutionIndex);
            ApplyResolution(savedResolutionIndex);
        }
        else
        {
            resolutionDropdown.SetValueWithoutNotify(currentResolutionIndex);
        }

        resolutionDropdown.RefreshShownValue();
    }

    public void ShowOptions()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        IsOpen = true;
    }

    public void HideOptions()
    {
        gameObject.SetActive(false);
        IsOpen = false;
    }

    public void OnResolutionChanged(int index)
    {
        PlayerPrefs.SetInt("ResolutionIndex", index);
        PlayerPrefs.Save();
        ApplyResolution(index);
    }

    private void ApplyResolution(int index)
    {
        if (filteredResolutions == null || index < 0 || index >= filteredResolutions.Count) return;
        Resolution resolution = filteredResolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
    }

    public void OnScreenModeChanged(int index)
    {
        PlayerPrefs.SetInt("ScreenMode", index);
        PlayerPrefs.Save();
        ApplyScreenMode(index);
    }

    private void ApplyScreenMode(int index)
    {
        switch (index)
        {
            case 0: Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen; break;
            case 1: Screen.fullScreenMode = FullScreenMode.FullScreenWindow; break;
            case 2: Screen.fullScreenMode = FullScreenMode.Windowed; break;
        }
    }

    public void OnSensitivityChanged(float value)
    {
        PlayerMovement.SetGlobalMouseSensitivity(value);
        PlayerPrefs.SetFloat("MouseSensitivity", value);
        PlayerPrefs.Save();
        UpdateSensitivityLabel(value);
    }

    private void UpdateSensitivityLabel(float value)
    {
        if (sensitivityLabel != null) sensitivityLabel.text = $"{value:0}";
    }

    public void OnDisableFovChanged(bool isOn)
    {
        PlayerPrefs.SetInt("DisableSprintFOV", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void OnMotionBlurChanged(bool isOn)
    {
        PlayerPrefs.SetInt("MotionBlur", isOn ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMotionBlur(isOn);
    }

    public void OnBrightnessChanged(float value)
    {
        brightnessOffset = value;
        PlayerPrefs.SetFloat("Brightness", value);
        PlayerPrefs.Save();
        UpdateBrightnessLabel(value);
        var lightingManager = FindAnyObjectByType<DungeonLightingManager>();
        if (lightingManager != null)
        {
            lightingManager.UpdateBrightness();
        }
    }

    private void UpdateBrightnessLabel(float value)
    {
        if (brightnessLabel != null) brightnessLabel.text = $"{value:0.00}";
    }

    private void ApplyMotionBlur(bool isOn)
    {
        if (globalVolumeProfile != null && globalVolumeProfile.TryGet(out MotionBlur motionBlur))
        {
            motionBlur.active = isOn;
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
        if (fpsDisplay != null) fpsDisplay.SetActive(isVisible);
    }

    public void SetMasterVolume(float sliderValue)
    {
        PlayerPrefs.SetFloat("MasterVolume", sliderValue);
        if (mainAudioMixer != null) mainAudioMixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20);
    }

    public void SetSFXVolume(float sliderValue)
    {
        PlayerPrefs.SetFloat("SFXVolume", sliderValue);
        if (mainAudioMixer != null) mainAudioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20);
    }

    public void SetBGMVolume(float sliderValue)
    {
        PlayerPrefs.SetFloat("BGMVolume", sliderValue);
        if (mainAudioMixer != null) mainAudioMixer.SetFloat("BGMVolume", Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20);
    }
}