using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using BloodBoard.GameManagement;

namespace BloodBoard.UI
{

public class EndlessScoreInputUI : MonoBehaviour
{
    public static EndlessScoreInputUI Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private TMP_Text scoreDisplay;
    [SerializeField] private TMP_Text floorDisplay;

    private int _currentFloor;
    private int _currentScore;
    private float _playerHealthAtDeath;
    private bool _isNormalModeCompletion = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        if (panel != null)
        {
            panel.SetActive(false);
        }
        if (nameInputField != null) nameInputField.gameObject.SetActive(false);
        if (submitButton != null) submitButton.gameObject.SetActive(false);
        if (skipButton != null) skipButton.gameObject.SetActive(false);
        if (scoreDisplay != null) scoreDisplay.gameObject.SetActive(false);
        if (titleText != null) titleText.gameObject.SetActive(false);
        if (floorDisplay != null) floorDisplay.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (nameInputField != null)
        {
            nameInputField.characterLimit = 9;
        }
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmitScore);
        }
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(OnSkipScore);
        }
    }

    public void Show(int floor, int score, float healthAtDeath)
    {
        _isNormalModeCompletion = false;
        _currentFloor = floor;
        _currentScore = score;
        _playerHealthAtDeath = healthAtDeath;

        if (titleText != null)
        {
            titleText.text = "HAS CAÍDO";
            titleText.gameObject.SetActive(true);
        }
        if (floorDisplay != null)
        {
            floorDisplay.text = $"Piso: {_currentFloor}";
            floorDisplay.gameObject.SetActive(true);
        }

        ShowCommonUI();
    }

    public void ShowForNormalMode(int score)
    {
        _isNormalModeCompletion = true;
        _currentScore = score;

        if (titleText != null)
        {
            titleText.text = "REGISTRA TU HAZAÑA";
            titleText.gameObject.SetActive(true);
        }
        if (floorDisplay != null) floorDisplay.gameObject.SetActive(false);

        ShowCommonUI();
    }

    private void ShowCommonUI()
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }
        if (nameInputField != null) nameInputField.gameObject.SetActive(true);
        if (submitButton != null) submitButton.gameObject.SetActive(true);
        if (skipButton != null) skipButton.gameObject.SetActive(true);
        if (scoreDisplay != null) scoreDisplay.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;

        if (scoreDisplay != null)
        {
            scoreDisplay.text = $"{_currentScore}";
        }
        if (nameInputField != null)
        {
            nameInputField.text = "";
            nameInputField.Select();
            nameInputField.ActivateInputField();
        }
        Debug.Log("Mostrando UI para registrar puntuación.");
    }

    private void Hide()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
        if (nameInputField != null) nameInputField.gameObject.SetActive(false);
        if (submitButton != null) submitButton.gameObject.SetActive(false);
        if (skipButton != null) skipButton.gameObject.SetActive(false);
        if (scoreDisplay != null) scoreDisplay.gameObject.SetActive(false);
        if (titleText != null) titleText.gameObject.SetActive(false);
        if (floorDisplay != null) floorDisplay.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    private void OnSubmitScore()
    {
        string playerName = nameInputField.text.Trim();
        if (string.IsNullOrEmpty(playerName)) { playerName = "Anon"; }

        if (_isNormalModeCompletion)
        {
            ScoreManager.Instance?.AddNormalModeScore(_currentScore, playerName);
            SaveManager.DeleteSlot(GameModeManager.CurrentSlot);
            Time.timeScale = 1f;
            SceneManager.LoadScene("Credits");
        }
        else
        {
            ScoreManager.Instance?.AddEndlessModeScore(_currentFloor, _currentScore, playerName);
            SaveManager.DeleteSlot(GameModeManager.CurrentSlot);
            Hide();
            Object.FindFirstObjectByType<GameOver>()?.ShowGameOver();
        }
    }
 
    private void OnSkipScore()
    {
        if (_isNormalModeCompletion)
        {
            SaveManager.DeleteSlot(GameModeManager.CurrentSlot);
            Time.timeScale = 1f;
            SceneManager.LoadScene("Credits");
        }
        else
        {
            SaveManager.DeleteSlot(GameModeManager.CurrentSlot);
            Hide();
            Object.FindFirstObjectByType<GameOver>()?.ShowGameOver();
        }
    }
}
}
