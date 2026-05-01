using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotsUI : MonoBehaviour
{
    [SerializeField] private Button[] slotButtons = new Button[3];
    [SerializeField] private TMP_Text[] slotTexts = new TMP_Text[3];
    [SerializeField] private Button deleteAllButton;
    [SerializeField] private Button backToMenuButton;

    private int selectedSlot = -1;

    private void Start()
    {
        UpdateSlots();
        for (int i = 0; i < 3; i++)
        {
            int slot = i + 1;
            slotButtons[i].onClick.AddListener(() => OnSlotClicked(slot));
        }
        if (deleteAllButton != null)
        {
            deleteAllButton.onClick.AddListener(OnDeleteAll);
        }

        if (backToMenuButton != null)
        {
            backToMenuButton.onClick.AddListener(OnBackToMenu);
        }
    }

    private void OnEnable()
    {
        UpdateSlots();
        if (deleteAllButton != null) deleteAllButton.gameObject.SetActive(true);
        if (backToMenuButton != null) backToMenuButton.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        // Hide these buttons when the slots panel is inactive
        if (deleteAllButton != null) deleteAllButton.gameObject.SetActive(false);
        if (backToMenuButton != null) backToMenuButton.gameObject.SetActive(false);
    }

    public void UpdateSlots()
    {
        for (int i = 0; i < 3; i++)
        {
            int slot = i + 1;
            if (SaveManager.IsSlotEmpty(slot))
            {
                slotTexts[i].text = "ARCHIVO VACIO";
            }
            else
            {
                var data = SaveManager.LoadFromSlot(slot);
                if (data.floor == 0)
                {
                    slotTexts[i].text = $"Tutorial, Score {data.score}, Modo {data.mode}";
                }
                else
                {
                    slotTexts[i].text = $"Piso {data.floor}, Score {data.score}, Modo {data.mode}";
                }
            }
        }
    }

    private void OnSlotClicked(int slot)
    {
        selectedSlot = slot; // Always set selected slot

        if (TitleScreen.Instance.GetCurrentSlotAction() == SlotAction.NewGame)
        {
            // If user clicked "New Game" and then a slot, proceed to mode selection.
            // If the slot is not empty, it implies the user wants to overwrite it.
            // A proper game would show a confirmation dialog here.
            TitleScreen.Instance.ShowModeSelector();
        }
        else if (TitleScreen.Instance.GetCurrentSlotAction() == SlotAction.ContinueGame)
        {
            if (!SaveManager.IsSlotEmpty(slot))
            {
                var data = SaveManager.LoadFromSlot(slot);
                GameModeManager.SetSlot(slot);
                GameModeManager.SetMode(data.mode == "Normal" ? GameModeManager.CreateNormalMode() : GameModeManager.CreateEndlessMode());
                LevelManager.currentLevel = data.floor;
                // Restaura la puntuación guardada para que no se acumule al salir y continuar.
                BloodBoard.GameManagement.ScoreManager.Instance?.SetCurrentScore(data.score);

                if (data.floor == 0) // Floor 0 is the tutorial
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("Level_Tuto");
                }
                else
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("Level_1");
                }
            }
            else
            {
                Debug.LogWarning($"No se puede continuar una partida en un slot vacío ({slot}).");
            }
        }
    }

    private void OnDeleteAll()
    {
        SaveManager.DeleteAllSlots();
        UpdateSlots();
    }

    private void OnBackToMenu()
    {
        TitleScreen.Instance.OnBackToMenu();
    }

    public int GetSelectedSlot()
    {
        return selectedSlot;
    }
}