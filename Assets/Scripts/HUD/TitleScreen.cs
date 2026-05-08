using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BloodBoard.GameManagement;
using BloodBoard.UI;

public enum SlotAction
{
    None,
    NewGame,
    ContinueGame
}

public class TitleScreen : MonoBehaviour
{
    public static TitleScreen Instance { get; private set; }

    [Header("Referencias")]
    [SerializeField] private GameObject titleBackground;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private GameObject modeSelectorPanel;
    [SerializeField] private GameObject modeSelectorContent;
    [SerializeField] private Button normalButton;
    [SerializeField] private Button endlessButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button backButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private SlotsUI slotsUI;
    [SerializeField] private GameObject slotsPanel;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private Button creditsButton;

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

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        currentSlotAction = SlotAction.None;
    }

    private void Start()
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

        var saves = SaveManager.GetAvailableSaves();
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(saves.Count > 0);
            if (saves.Count > 0)
            {
                continueButton.onClick.AddListener(() => ShowSlotsScreen(SlotAction.ContinueGame));
            }
            else
            {
                continueButton.gameObject.SetActive(false);
            }
        }

        if (normalButton != null)
        {
            normalButton.onClick.AddListener(OnNormalButton);
            AddHoverListeners(normalButton, "Modo Normal: La experiencia base de Blood Board de principio a fin. El terreno ideal para forzar tu técnica y dominar las reglas.");
        }

        if (endlessButton != null)
        {
            endlessButton.onClick.AddListener(OnEndlessButton);
            AddHoverListeners(endlessButton, "Modo Infinito: Un desafío absoluto y sin límites. Supera tus marcas y demuestra que tu rendimiento está por encima del resto.");
        }

        if (newGameButton != null)
        {
            newGameButton.onClick.AddListener(() => ShowSlotsScreen(SlotAction.NewGame));
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButton);
        }

        if (optionsButton != null)
        {
            optionsButton.onClick.AddListener(OnOptionsButton);
        }

        if (leaderboardButton != null)
        {
            leaderboardButton.onClick.AddListener(OnLeaderboardButton);
        }

        if (creditsButton != null)
        {
            creditsButton.onClick.AddListener(OnCreditsButton);
        }
    }

    public void ShowModeSelector()
    {
        if (modeSelectorContent != null)
        {
            modeSelectorContent.SetActive(true);
        }
        if (titleBackground != null)
        {
            titleBackground.SetActive(false);
        }
        if (slotsPanel != null)
        {
            slotsPanel.SetActive(false);
        }
    }

    private SlotAction currentSlotAction = SlotAction.None;

    public void ShowSlotsScreen(SlotAction action)
    {
        currentSlotAction = action;

        if (slotsPanel != null)
        {
            slotsPanel.SetActive(true);
        }
        if (titleBackground != null)
        {
            titleBackground.SetActive(false);
        }
        if (newGameButton != null)
        {
            newGameButton.gameObject.SetActive(false);
        }
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }
        if (optionsButton != null)
        {
            optionsButton.gameObject.SetActive(false);
        }
        if (creditsButton != null)
        {
            creditsButton.gameObject.SetActive(false);
        }
        if (leaderboardButton != null) {
            leaderboardButton.gameObject.SetActive(false);
        }
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
        SceneManager.LoadScene("Level_Tuto");
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
        SceneManager.LoadScene("Level_1");
    }

    public void OnPlayButton()
    {
        SceneManager.LoadScene("Level_Tuto");
    }

    public void OnOptionsButton()
    {
        if (Options.Instance != null)
        {
            Options.Instance.ShowOptions();
        }
        else
        {
            Debug.Log("Opciones no disponibles");
        }
    }

    public void OnLeaderboardButton()
    {
        LeaderboardUI.Instance?.ShowLeaderboards();

        if (titleBackground != null)
        {
            titleBackground.SetActive(false);
        }

        if (newGameButton != null) newGameButton.gameObject.SetActive(false);
        if (continueButton != null) continueButton.gameObject.SetActive(false);
        if (optionsButton != null) optionsButton.gameObject.SetActive(false);
        if (leaderboardButton != null) leaderboardButton.gameObject.SetActive(false);
        if (creditsButton != null) creditsButton.gameObject.SetActive(false);
    }
    private void AddHoverListeners(Button button, string description)
    {
        var trigger = button.gameObject.AddComponent<EventTrigger>();
        var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((data) => OnHoverEnter(description));
        trigger.triggers.Add(enterEntry);

        var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((data) => OnHoverExit());
        trigger.triggers.Add(exitEntry);
    }

    private void OnHoverEnter(string description)
    {
        if (descriptionText != null)
        {
            descriptionText.text = description;
            descriptionText.gameObject.SetActive(true);
        }
    }

    private void OnHoverExit()
    {
        if (descriptionText != null)
        {
            descriptionText.gameObject.SetActive(false);
        }
    }

    public void OnBackButton()
    {
        if (modeSelectorContent != null)
        {
            modeSelectorContent.SetActive(false);
        }
        if (titleBackground != null)
        {
            titleBackground.SetActive(true);
        }
        if (newGameButton != null)
        {
            newGameButton.gameObject.SetActive(true);
        }
        if (continueButton != null)
        {
            var saves = SaveManager.GetAvailableSaves();
            continueButton.gameObject.SetActive(saves.Count > 0);
        }
        if (optionsButton != null)
        {
            optionsButton.gameObject.SetActive(true);
        }
        if (creditsButton != null)
        {
            creditsButton.gameObject.SetActive(true);
        }
        if (leaderboardButton != null)
        {
            leaderboardButton.gameObject.SetActive(true);
        }
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
        if (titleBackground != null)
        {
            titleBackground.SetActive(true);
        }
        if (newGameButton != null)
        {
            newGameButton.gameObject.SetActive(true);
        }
        if (continueButton != null)
        {
            var saves = SaveManager.GetAvailableSaves();
            continueButton.gameObject.SetActive(saves.Count > 0);
        }
        if (optionsButton != null)
        {
            optionsButton.gameObject.SetActive(true);
        }
        if (creditsButton != null)
        {
            creditsButton.gameObject.SetActive(true);
        }
        if (leaderboardButton != null)
        {
            leaderboardButton.gameObject.SetActive(true);
        }
    }

    public SlotAction GetCurrentSlotAction() { return currentSlotAction; }

    public void OnCreditsButton()
    {
        SceneManager.LoadScene("Credits");
    }
}
