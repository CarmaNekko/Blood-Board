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
        if (deleteAllButton != null) deleteAllButton.gameObject.SetActive(false);
        if (backToMenuButton != null) backToMenuButton.gameObject.SetActive(false);
    }

    public void UpdateSlots()
    {
        var action = TitleScreen.Instance != null ? TitleScreen.Instance.GetCurrentSlotAction() : SlotAction.None;

        for (int i = 0; i < 3; i++)
        {
            int slot = i + 1;
            bool isEmpty = SaveManager.IsSlotEmpty(slot);

            if (isEmpty)
            {
                slotTexts[i].text = "ARCHIVO VACIO";
            }
            else
            {
                var data = SaveManager.LoadFromSlot(slot);
                string floorText = data.floor == 0 ? "Tutorial" : $"Piso {data.floor}";
                slotTexts[i].text = $"{floorText}, Score {data.score}, Modo {data.mode}";
            }

            if (slotButtons[i] != null)
            {
                if (action == SlotAction.ContinueGame)
                {
                    slotButtons[i].interactable = !isEmpty;
                }
                else
                {
                    slotButtons[i].interactable = true;
                }
            }
        }
    }

    private void OnSlotClicked(int slot)
    {
        selectedSlot = slot;

        if (TitleScreen.Instance.GetCurrentSlotAction() == SlotAction.NewGame)
        {
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
                BloodBoard.GameManagement.ScoreManager.Instance?.SetCurrentScore(data.score);

                if (data.floor == 0)
                {
                    CheckerboardTransition.LoadScene("Level_Tuto");
                }
                else
                {
                    CheckerboardTransition.LoadScene("Level_1");
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
