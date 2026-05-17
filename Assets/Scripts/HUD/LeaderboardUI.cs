using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using BloodBoard.GameManagement;

namespace BloodBoard.UI
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
    [SerializeField] private Transform normalScoresParent;
    [SerializeField] private GameObject normalScoreEntryPrefab;

    [Header("Endless Mode Leaderboard")]
    [SerializeField] private Transform endlessScoresParent;
    [SerializeField] private GameObject endlessScoreEntryPrefab;

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
        if (backButton != null) backButton.gameObject.SetActive(false);
        if (normalTabButton != null) normalTabButton.gameObject.SetActive(false);
        if (endlessTabButton != null) endlessTabButton.gameObject.SetActive(false);
        if (normalScoresParent != null) normalScoresParent.gameObject.SetActive(false);
        if (endlessScoresParent != null) endlessScoresParent.gameObject.SetActive(false);

        if (normalScoresParent != null) foreach (Transform child in normalScoresParent) Destroy(child.gameObject);
        if (endlessScoresParent != null) foreach (Transform child in endlessScoresParent) Destroy(child.gameObject);
    }

    private void Start()
    {
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBack);
            AddHoverListeners(backButton);
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
        if (backButton != null)
        {
            backButton.gameObject.SetActive(true);
            SetButtonDimmed(backButton);
        }
        if (normalTabButton != null) normalTabButton.gameObject.SetActive(true);
        if (endlessTabButton != null) endlessTabButton.gameObject.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        DisplayNormalScores();
        DisplayEndlessScores();
        ShowTab(true);
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

        if (normalTabButton != null) UpdateTabAppearance(normalTabButton, showNormal);
        if (endlessTabButton != null) UpdateTabAppearance(endlessTabButton, !showNormal);
    }

    private void UpdateTabAppearance(Button button, bool isActive)
    {
        button.transition = Selectable.Transition.None;

        Image image = button.GetComponent<Image>();
        TMP_Text text = button.GetComponentInChildren<TMP_Text>();

        if (image == null || text == null) return;

        Color imageColor = image.color;
        Color textColor = text.color;

        if (isActive)
        {
            imageColor.a = 0.09019608f;
            textColor.a = 1.0f;
        }
        else
        {
            imageColor.a = 0.04313726f;
            textColor.a = 0.2901961f;
        }

        image.color = imageColor;
        text.color = textColor;
        button.interactable = !isActive;
    }
    private void Hide()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

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

        if (backButton != null) backButton.gameObject.SetActive(false);
        if (normalTabButton != null) normalTabButton.gameObject.SetActive(false);
        if (endlessTabButton != null) endlessTabButton.gameObject.SetActive(false);

        Time.timeScale = 1f;
        TitleScreen.Instance?.OnBackToMenu();
    }

    private void OnBack()
    {
        Hide();
    }

    private void AddHoverListeners(Button button)
    {
        button.transition = Selectable.Transition.None;

        var trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = button.gameObject.AddComponent<EventTrigger>();
        trigger.triggers.Clear();

        var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((data) => OnHoverEnter(button));
        trigger.triggers.Add(enterEntry);

        var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((data) => OnHoverExit(button));
        trigger.triggers.Add(exitEntry);
    }

    private void OnHoverEnter(Button button)
    {
        SetButtonBright(button);
    }

    private void OnHoverExit(Button button)
    {
        SetButtonDimmed(button);
    }

    private void SetButtonBright(Button button)
    {
        var image = button.GetComponent<Image>();
        if (image != null)
        {
            Color c = image.color;
            c.a = 1f;
            image.color = c;
        }

        var text = button.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            Color tc = text.color;
            tc.a = 1f;
            text.color = tc;
        }
    }

    private void SetButtonDimmed(Button button)
    {
        var image = button.GetComponent<Image>();
        if (image != null)
        {
            Color c = image.color;
            c.a = 0.09019608f;
            image.color = c;
        }

        var text = button.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            Color tc = text.color;
            tc.a = 1f;
            text.color = tc;
        }
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
