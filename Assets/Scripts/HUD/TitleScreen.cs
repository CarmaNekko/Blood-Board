using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
    [SerializeField] private GameObject modeSelectorContent; // Grupo para contenido del selector
    [SerializeField] private Button normalButton;
    [SerializeField] private Button endlessButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private TMP_Text continueText;
    [SerializeField] private TMP_Text descriptionText; // Para descripciones de modos
    [SerializeField] private Button backButton; // Para volver del selector
    [SerializeField] private Button newGameButton; // Para mostrar selector de modos
    [SerializeField] private SlotsUI slotsUI; // Referencia al script de slots
    [SerializeField] private GameObject slotsPanel; // Panel padre de slots
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton; // Added for completeness, if it exists
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

        currentSlotAction = SlotAction.None; // Initialize
    }

    private void Start()
    {
        if (slotsPanel != null)
        {
            slotsPanel.SetActive(false);
        }

        if (modeSelectorContent != null)
        {
            modeSelectorContent.SetActive(false); // Oculto hasta New Game
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
                // Continue button now just opens the slot selection screen
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
        { // Changed to pass SlotAction
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

        if (creditsButton != null)
        {
            creditsButton.onClick.AddListener(OnCreditsButton);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitButton);
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
            slotsPanel.SetActive(false); // Hide slots when selecting mode
        }
    }

    private SlotAction currentSlotAction = SlotAction.None; // New field

    public void ShowSlotsScreen(SlotAction action) // Modified to accept action
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
        if (quitButton != null) { // Hide quit button too
            quitButton.gameObject.SetActive(false);
        }
    }

    public void OnNormalButton()
    {
        int selectedSlot = slotsUI.GetSelectedSlot();
        if (selectedSlot == -1) return; // Should not happen

        GameModeManager.SetSlot(selectedSlot);
        GameModeManager.SetMode(GameModeManager.CreateNormalMode());
        // Perform initial save for a new game starting in tutorial
        SaveManager.SaveToSlot(selectedSlot, 0, 0, 100f, GameModeManager.CurrentMode.GetModeName()); // Floor 0 for tutorial
        Debug.Log($"Nueva partida Normal iniciada en slot {selectedSlot}. Guardado inicial en tutorial (piso 0).");
        SceneManager.LoadScene("Level_Tuto");
    }

    public void OnEndlessButton()
    {
        int selectedSlot = slotsUI.GetSelectedSlot();
        if (selectedSlot == -1) return; // Should not happen

        GameModeManager.SetSlot(selectedSlot);
        GameModeManager.SetMode(GameModeManager.CreateEndlessMode());
        LevelManager.currentLevel = 1;
        SaveManager.SaveToSlot(selectedSlot, 1, 0, 100f, GameModeManager.CurrentMode.GetModeName());
        Debug.Log($"Nueva partida Infinita iniciada en slot {selectedSlot}. Guardado inicial en piso 1.");
        SceneManager.LoadScene("Level_1");
    }

    public void OnPlayButton()
    {
        // Sin modo, tutorial
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
        if (quitButton != null) { // Show quit button too
            quitButton.gameObject.SetActive(true);
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
        if (quitButton != null) { // Show quit button too
            quitButton.gameObject.SetActive(true);
        }
    }

    public void OnQuitButton()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public SlotAction GetCurrentSlotAction() { return currentSlotAction; } // New public getter

    public void OnCreditsButton()
    {
        SceneManager.LoadScene("Credits");
    }
}