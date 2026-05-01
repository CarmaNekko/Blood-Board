using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using BloodBoard.GameManagement;

namespace BloodBoard.UI // Added namespace
{

public class LeaderboardUI : MonoBehaviour
{
    public static LeaderboardUI Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button backButton;

    [Header("Tabs")]
    [SerializeField] private Button normalTabButton;
    [SerializeField] private Button endlessTabButton;

    [Header("Normal Mode Leaderboard")]
    [SerializeField] private Transform normalScoresParent; // Parent for normal score entries
    [SerializeField] private GameObject normalScoreEntryPrefab; // Prefab for a single score entry (Text or custom UI)

    [Header("Endless Mode Leaderboard")]
    [SerializeField] private Transform endlessScoresParent; // Parent for endless score entries
    [SerializeField] private GameObject endlessScoreEntryPrefab; // Prefab for a single score entry (Text or custom UI)

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
        // Asegurarse de que todos los elementos de la UI estén ocultos al inicio
        if (backButton != null) backButton.gameObject.SetActive(false);
        if (normalTabButton != null) normalTabButton.gameObject.SetActive(false);
        if (endlessTabButton != null) endlessTabButton.gameObject.SetActive(false);
        if (normalScoresParent != null) normalScoresParent.gameObject.SetActive(false);
        if (endlessScoresParent != null) endlessScoresParent.gameObject.SetActive(false);

        // Limpiar cualquier entrada de puntuación residual que pudiera haber quedado en el editor
        if (normalScoresParent != null) foreach (Transform child in normalScoresParent) Destroy(child.gameObject);
        if (endlessScoresParent != null) foreach (Transform child in endlessScoresParent) Destroy(child.gameObject);
    }

    private void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBack);
        }
        if (normalTabButton != null)
        {
            normalTabButton.onClick.AddListener(() => ShowTab(true));
        }
        if (endlessTabButton != null)
        {
            endlessTabButton.onClick.AddListener(() => ShowTab(false));
        }
    }

    public void ShowLeaderboards()
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }
        // Asegurarse de que los botones de navegación también sean visibles
        if (backButton != null) backButton.gameObject.SetActive(true);
        if (normalTabButton != null) normalTabButton.gameObject.SetActive(true);
        if (endlessTabButton != null) endlessTabButton.gameObject.SetActive(true);

        Time.timeScale = 0f; // Pause game if called mid-game (though usually from title)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        DisplayNormalScores();
        DisplayEndlessScores();
        ShowTab(true); // Show normal tab by default
        Debug.Log("Showing Leaderboards.");
    }

    private void ShowTab(bool showNormal)
    {
        if (normalScoresParent != null)
        {
            normalScoresParent.gameObject.SetActive(showNormal);
        }
        if (endlessScoresParent != null)
        {
            endlessScoresParent.gameObject.SetActive(!showNormal);
        }
        if (normalTabButton != null) normalTabButton.interactable = !showNormal;
        if (endlessTabButton != null) endlessTabButton.interactable = showNormal;
    }

    private void Hide()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

        // Limpiar las entradas de puntuación al salir para que no queden en la escena
        if (normalScoresParent != null)
        {
            normalScoresParent.gameObject.SetActive(false);
            foreach (Transform child in normalScoresParent) Destroy(child.gameObject);
        }
        if (endlessScoresParent != null)
        {
            endlessScoresParent.gameObject.SetActive(false);
            foreach (Transform child in endlessScoresParent) Destroy(child.gameObject);
        }

        // Ocultar explícitamente los botones para asegurar que desaparezcan
        if (backButton != null) backButton.gameObject.SetActive(false);
        if (normalTabButton != null) normalTabButton.gameObject.SetActive(false);
        if (endlessTabButton != null) endlessTabButton.gameObject.SetActive(false);

        Time.timeScale = 1f; // Resume game
        TitleScreen.Instance?.OnBackToMenu(); // Use existing TitleScreen method to restore main menu
    }

    private void OnBack()
    {
        Hide();
    }

    private void DisplayNormalScores()
    {
        if (normalScoresParent != null) foreach (Transform child in normalScoresParent) Destroy(child.gameObject);

        List<ScoreEntry> normalScores = ScoreManager.Instance?.GetNormalModeScores();
        const int maxEntries = 6;

        for (int i = 0; i < maxEntries; i++)
        {
            if (normalScores != null && i < normalScores.Count)
            {
                ScoreEntry entry = normalScores[i];
                CreateScoreEntry(normalScoresParent, $"{i + 1}. {entry.playerName}", $"{entry.score} pts", "");
            }
            else
            {
                CreateScoreEntry(normalScoresParent, $"{i + 1}. ----", "----", "");
            }
        }
    }

    private void DisplayEndlessScores()
    {
        if (endlessScoresParent != null) foreach (Transform child in endlessScoresParent) Destroy(child.gameObject);

        List<ScoreEntry> endlessScores = ScoreManager.Instance?.GetEndlessModeScores();
        const int maxEntries = 6;

        for (int i = 0; i < maxEntries; i++)
        {
            if (endlessScores != null && i < endlessScores.Count)
            {
                ScoreEntry entry = endlessScores[i];
                CreateScoreEntry(endlessScoresParent, $"{i + 1}. {entry.playerName}", $"{entry.score} pts", $"Piso {entry.floor}");
            }
            else
            {
                CreateScoreEntry(endlessScoresParent, $"{i + 1}. ----", "----", "----");
            }
        }
    }
    private void CreateScoreEntry(Transform parent, string rankName, string scoreText, string floorText)
    {
        // Usamos el prefab correspondiente al padre
        GameObject prefab = (parent == normalScoresParent) ? normalScoreEntryPrefab : endlessScoreEntryPrefab;
        if (prefab == null) { Debug.LogError("Score Entry Prefab no está asignado en LeaderboardUI."); return; }

        GameObject entryObj = Instantiate(prefab, parent);
        ScoreEntryUI entryUI = entryObj.GetComponent<ScoreEntryUI>();

        if (entryUI == null)
        {
            Debug.LogError("El prefab de la entrada de puntuación no tiene el script 'ScoreEntryUI'.");
            return;
        }

        if (entryUI.rankNameText != null) entryUI.rankNameText.SetText(rankName);
        if (entryUI.scoreText != null) entryUI.scoreText.SetText(scoreText);
        if (entryUI.floorText != null) entryUI.floorText.SetText(floorText);
    }
}
}