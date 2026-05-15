using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BloodBoard.GameManagement;
using BloodBoard.UI;

public class TitleScreenSelection : MonoBehaviour
{
    public static TitleScreenSelection Instance { get; private set; }

    [Header("Referencias")]
    [SerializeField] private GameObject titleBackground;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private GameObject modeSelectorPanel;
    [SerializeField] private GameObject modeSelectorContent;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button endlessButton;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button backButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private SlotsUI slotsUI;
    [SerializeField] private GameObject slotsPanel;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private Button creditsButton;

    private SlotAction currentSlotAction = SlotAction.None;

    private void Awake()
    {
        Debug.Log("TitleScreenSelection Awake");
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    public void SetupMainMenu()
    {
        if (slotsPanel != null)
        {
            slotsPanel.SetActive(false);
        }

        if (modeSelectorContent != null)
        {
            modeSelectorContent.SetActive(false);
        }
        else
        {
            Debug.LogWarning("ModeSelectorContent no asignado, ocultando elementos individualmente");
            if (normalButton != null) normalButton.gameObject.SetActive(false);
            if (endlessButton != null) endlessButton.gameObject.SetActive(false);
            if (descriptionText != null) descriptionText.gameObject.SetActive(false);
            if (backButton != null) backButton.gameObject.SetActive(false);
        }

        if (newGameButton != null)
        {
            newGameButton.onClick.AddListener(() => { Debug.Log("NewGame button pressed"); ShowSlotsScreen(SlotAction.NewGame); });
            AddHoverListeners(newGameButton);
        }

        if (normalButton != null)
        {
            AddHoverListeners(normalButton, "El modo normal es una experiencia curada con un final definido. Supera los pisos para enfrentarte a los jefes finales.");
            normalButton.onClick.AddListener(OnNormalButton);
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(() => ShowSlotsScreen(SlotAction.ContinueGame));
            AddHoverListeners(continueButton);
        }

        if (endlessButton != null)
        {
            AddHoverListeners(endlessButton, "El modo infinito es un desafío sin fin. Sobrevive el mayor tiempo posible, subiendo de piso y acumulando puntos.");
            endlessButton.onClick.AddListener(OnEndlessButton);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButton);
            AddHoverListeners(backButton);
        }

        if (optionsButton != null)
        {
            optionsButton.onClick.AddListener(OnOptionsButton);
            AddHoverListeners(optionsButton);
        }

        if (leaderboardButton != null)
        {
            leaderboardButton.onClick.AddListener(OnLeaderboardButton);
            AddHoverListeners(leaderboardButton);
        }

        if (creditsButton != null)
        {
            creditsButton.onClick.AddListener(OnCreditsButton);
            AddHoverListeners(creditsButton);
        }

        ReorganizeUI();
    }

    public void HideMainMenu()
    {
        if (titleBackground != null) titleBackground.SetActive(false);
        if (newGameButton != null) newGameButton.gameObject.SetActive(false);
        if (continueButton != null) continueButton.gameObject.SetActive(false);
        if (optionsButton != null) optionsButton.gameObject.SetActive(false);
        if (leaderboardButton != null) leaderboardButton.gameObject.SetActive(false);
        if (creditsButton != null) creditsButton.gameObject.SetActive(false);
    }

    public void ShowMainMenu()
    {
        if (titleBackground != null) titleBackground.SetActive(true);
        var saves = SaveManager.GetAvailableSaves();
        if (newGameButton != null)
        {
            newGameButton.gameObject.SetActive(true);
            SetButtonDimmed(newGameButton);
        }
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(saves.Count > 0);
            if (continueButton.gameObject.activeSelf) SetButtonDimmed(continueButton);
        }
        if (optionsButton != null)
        {
            optionsButton.gameObject.SetActive(true);
            SetButtonDimmed(optionsButton);
        }
        if (leaderboardButton != null)
        {
            leaderboardButton.gameObject.SetActive(true);
            SetButtonDimmed(leaderboardButton);
        }
        if (creditsButton != null)
        {
            creditsButton.gameObject.SetActive(true);
            SetButtonDimmed(creditsButton);
        }
        ReorganizeUI();
    }

    public void ShowModeSelector()
    {
        if (modeSelectorContent != null)
        {
            modeSelectorContent.SetActive(true);
            if (normalButton != null) SetButtonDimmed(normalButton);
            if (endlessButton != null) SetButtonDimmed(endlessButton);
            if (backButton != null) SetButtonDimmed(backButton);
        }
        if (slotsPanel != null)
        {
            slotsPanel.SetActive(false);
        }
    }

    public void OnBackButton()
    {
        if (modeSelectorContent != null)
        {
            modeSelectorContent.SetActive(false);
        }
        if (slotsPanel != null)
        {
            slotsPanel.SetActive(true);
        }
        HideDescription();
    }

    public void OnBackToMenu()
    {
        Debug.Log("Volver al menú: ocultando slotsPanel");
        if (slotsPanel != null)
        {
            slotsPanel.SetActive(false);
            Debug.Log("slotsPanel oculto: " + !slotsPanel.activeSelf);
        }
        if (modeSelectorContent != null)
        {
            modeSelectorContent.SetActive(false);
        }
        ShowMainMenu();
    }

    public void OnCreditsButton()
    {
        CheckerboardTransition.LoadScene("Credits");
    }

    private void ReorganizeUI()
    {
        float currentY = 0f;
        float spacing = 60f;

        if (newGameButton != null)
        {
            SetButtonPosition(newGameButton, currentY);
            currentY -= spacing;
        }

        if (continueButton != null && continueButton.gameObject.activeSelf)
        {
            SetButtonPosition(continueButton, currentY);
            currentY -= spacing;
        }

        if (optionsButton != null)
        {
            SetButtonPosition(optionsButton, currentY);
            currentY -= spacing;
        }

        if (leaderboardButton != null)
        {
            SetButtonPosition(leaderboardButton, currentY);
            currentY -= spacing;
        }

        if (creditsButton != null)
        {
            SetButtonPosition(creditsButton, currentY);
        }
    }

    private void SetButtonPosition(Button button, float yPosition)
    {
        RectTransform rect = button.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, yPosition);
        }
    }

    private void AddHoverListeners(Button button, string description = null)
    {
        button.transition = Selectable.Transition.None;

        var trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = button.gameObject.AddComponent<EventTrigger>();
        trigger.triggers.Clear();

        var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((data) => OnHoverEnter(button, description));
        trigger.triggers.Add(enterEntry);

        var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((data) => OnHoverExit(button));
        trigger.triggers.Add(exitEntry);
    }

    private void OnHoverEnter(Button button, string description = null)
    {
        SetButtonBright(button);

        if (descriptionText != null && !string.IsNullOrEmpty(description))
        {
            descriptionText.text = description;
            descriptionText.gameObject.SetActive(true);
        }
    }

    private void OnHoverExit(Button button)
    {
        SetButtonDimmed(button);

        if (descriptionText != null)
        {
            descriptionText.gameObject.SetActive(false);
        }
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

    private void HideDescription()
    {
        if (descriptionText != null)
        {
            descriptionText.gameObject.SetActive(false);
        }
    }

    public void ShowSlotsScreen(SlotAction action)
    {
        currentSlotAction = action;

        HideDescription();

        if (slotsPanel != null)
        {
            slotsPanel.SetActive(true);
        }

        if (newGameButton != null) newGameButton.gameObject.SetActive(false);
        if (continueButton != null) continueButton.gameObject.SetActive(false);
        if (optionsButton != null) optionsButton.gameObject.SetActive(false);
        if (leaderboardButton != null) leaderboardButton.gameObject.SetActive(false);
        if (creditsButton != null) creditsButton.gameObject.SetActive(false);
    }

    public void OnNormalButton()
    {
        int selectedSlot = slotsUI.GetSelectedSlot();
        if (selectedSlot == -1) return;

        GameModeManager.SetSlot(selectedSlot);
        ScoreManager.Instance?.ResetCurrentScore();
        GameModeManager.SetMode(GameModeManager.CreateNormalMode());
        SaveManager.SaveToSlot(selectedSlot, 0, 0, 100f, GameModeManager.CurrentMode.GetModeName());
        Debug.Log($"Nueva partida Normal iniciada en slot {selectedSlot}. Guardado inicial en tutorial (piso 0).");
        CheckerboardTransition.LoadScene("Level_Tuto");
    }

    public void OnEndlessButton()
    {
        int selectedSlot = slotsUI.GetSelectedSlot();
        if (selectedSlot == -1) return;

        GameModeManager.SetSlot(selectedSlot);
        ScoreManager.Instance?.ResetCurrentScore();
        GameModeManager.SetMode(GameModeManager.CreateEndlessMode());
        LevelManager.currentLevel = 1;
        SaveManager.SaveToSlot(selectedSlot, 1, 0, 100f, GameModeManager.CurrentMode.GetModeName());
        Debug.Log($"Nueva partida Infinita iniciada en slot {selectedSlot}. Guardado inicial en piso 1.");
        CheckerboardTransition.LoadScene("Level_1");
    }

    public void OnPlayButton()
    {
        CheckerboardTransition.LoadScene("Level_Tuto");
    }

    public void OnOptionsButton()
    {
        if (Options.Instance != null)
        {
            Options.Instance.ShowOptions();
        }
        else
        {
            Debug.Log("Options not available");
        }
    }

    public void OnLeaderboardButton()
    {
        LeaderboardUI.Instance?.ShowLeaderboards();
        
        if (newGameButton != null) newGameButton.gameObject.SetActive(false);
        if (continueButton != null) continueButton.gameObject.SetActive(false);
        if (optionsButton != null) optionsButton.gameObject.SetActive(false);
        if (leaderboardButton != null) leaderboardButton.gameObject.SetActive(false);
        if (creditsButton != null) creditsButton.gameObject.SetActive(false);
    }

    public SlotAction GetCurrentSlotAction() { return currentSlotAction; }
}