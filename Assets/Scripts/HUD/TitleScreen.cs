using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BloodBoard.UI;

public class TitleScreen : MonoBehaviour
{
    public static TitleScreen Instance { get; private set; }

    [Header("Initial Screen")]
    [SerializeField] private GameObject initialScreenPanel;
    [SerializeField] private TMP_Text gameTitleText;
    [SerializeField] private TMP_Text initialText;
    [SerializeField] private CheckerboardTransition checkerboardTransition;

    [Header("Selection Manager")]
    [SerializeField] private TitleScreenSelection selectionManager;

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

        currentMenuState = MenuState.Initial;
    }

    private void Start()
    {
        if (selectionManager != null)
        {
            selectionManager.HideMainMenu();
        }

        // Mostrar pantalla inicial
        if (initialScreenPanel != null)
        {
            initialScreenPanel.SetActive(true);
        }
        if (gameTitleText != null)
        {
            gameTitleText.text = "BLOOD BOARD";
        }
        if (initialText != null)
        {
            initialText.text = "PRESIONA ENTER O CLICK IZ. PARA INICIAR";
        }

        if (selectionManager != null)
        {
            selectionManager.SetupMainMenu();
        }
    }

    private void Update()
    {
        if (currentMenuState == MenuState.Initial)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
            {
                StartTransition();
            }
        }
    }

    private void StartTransition()
    {
        currentMenuState = MenuState.Transition;
        if (initialScreenPanel != null) initialScreenPanel.SetActive(false);
        if (gameTitleText != null) gameTitleText.gameObject.SetActive(false);
        if (initialText != null) initialText.gameObject.SetActive(false);

        if (checkerboardTransition != null)
        {
            checkerboardTransition.StartTransition(() => OnTransitionComplete());
        }
        else
        {
            OnTransitionComplete();
        }
    }

    private void OnTransitionComplete()
    {
        currentMenuState = MenuState.MainMenu;
        if (selectionManager != null)
        {
            selectionManager.ShowMainMenu();
        }
    }

    public void ShowModeSelector() { if (selectionManager != null) selectionManager.ShowModeSelector(); }
    public void ShowSlotsScreen(SlotAction action) { if (selectionManager != null) selectionManager.ShowSlotsScreen(action); }
    public void OnNormalButton() { if (selectionManager != null) selectionManager.OnNormalButton(); }
    public void OnEndlessButton() { if (selectionManager != null) selectionManager.OnEndlessButton(); }
    public void OnPlayButton() { if (selectionManager != null) selectionManager.OnPlayButton(); }
    public void OnOptionsButton() { if (selectionManager != null) selectionManager.OnOptionsButton(); }
    public void OnLeaderboardButton() { if (selectionManager != null) selectionManager.OnLeaderboardButton(); }
    public void OnBackButton() { if (selectionManager != null) selectionManager.OnBackButton(); }
    public void OnBackToMenu() { if (selectionManager != null) selectionManager.OnBackToMenu(); }
    public void OnCreditsButton() { if (selectionManager != null) selectionManager.OnCreditsButton(); }

    public SlotAction GetCurrentSlotAction() { return selectionManager != null ? selectionManager.GetCurrentSlotAction() : SlotAction.None; }

    private MenuState currentMenuState = MenuState.Initial;
}
