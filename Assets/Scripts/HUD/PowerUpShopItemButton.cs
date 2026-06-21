using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpShopItemButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text labelText;

    public Button Button
    {
        get
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            return button;
        }
    }

    public void SetLabel(string label)
    {
        if (labelText == null)
        {
            labelText = GetComponentInChildren<TMP_Text>();
        }

        if (labelText != null)
        {
            labelText.text = label;
        }
    }

    public void SetAvailable(bool isAvailable)
    {
        if (Button != null)
        {
            Button.interactable = isAvailable;
        }

        Image image = GetComponent<Image>();
        if (image != null)
        {
            Color color = image.color;
            color.a = isAvailable ? 0.09019608f : 0.04f;
            image.color = color;
        }
    }
}
