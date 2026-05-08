using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour
{
    [SerializeField] private Button[] slotButtons = new Button[3];
    [SerializeField] private TMP_Text[] slotTexts = new TMP_Text[3];
    [SerializeField] private Button deleteAllButton;

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
    }

    public void UpdateSlots()
    {
        for (int i = 0; i < 3; i++)
        {
            int slot = i + 1;
            if (SaveManager.IsSlotEmpty(slot))
            {
                slotTexts[i].text = "Vacío";
            }
            else
            {
                var data = SaveManager.LoadFromSlot(slot);
                slotTexts[i].text = $"Piso {data.floor}, Score {data.score}, Modo {data.mode}";
            }
        }
    }

    private void OnSlotClicked(int slot)
    {
        if (SaveManager.IsSlotEmpty(slot))
        {
            selectedSlot = slot;
            TitleScreen.Instance.ShowModeSelector();
        }
        else
        {
            var data = SaveManager.LoadFromSlot(slot);
            GameModeManager.SetSlot(slot);
            GameModeManager.SetMode(data.mode == "Normal" ? GameModeManager.CreateNormalMode() : GameModeManager.CreateEndlessMode());
            LevelManager.currentLevel = data.floor;
            UnityEngine.SceneManagement.SceneManager.LoadScene(data.mode == "Normal" ? "Level_Tuto" : "Level_1");
        }
    }

    private void OnDeleteAll()
    {
        SaveManager.DeleteAllSlots();
        UpdateSlots();
    }

    public int GetSelectedSlot()
    {
        return selectedSlot;
    }
}
