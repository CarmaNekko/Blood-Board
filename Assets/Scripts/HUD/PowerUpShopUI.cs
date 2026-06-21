using BloodBoard.GameManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpShopUI : MonoBehaviour
{
    public static bool IsOpen { get; private set; }

    [Header("Root")]
    [SerializeField] private GameObject panelRoot;

    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text pointsText;
    [SerializeField] private TMP_Text statusText;

    [Header("Grid Slots")]
    [SerializeField] private PowerUpShopItemButton[] itemButtons;

    [Header("Actions")]
    [SerializeField] private Button cancelButton;

    private PowerUpShopInteractable currentShop;
    private PlayerInteractor currentBuyer;
    private float previousTimeScale = 1f;
    private CursorLockMode previousCursorLockState;
    private bool previousCursorVisible;

    private void Awake()
    {
        if (panelRoot == null)
        {
            panelRoot = gameObject;
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(Close);
        }

        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null && parentCanvas.GetComponent<GraphicRaycaster>() == null)
        {
            Debug.LogWarning("PowerUpShopUI: el Canvas no tiene GraphicRaycaster. Los botones no recibiran clicks.");
        }

        Close();
    }

    public void Open(PowerUpShopInteractable shop, PlayerInteractor buyer)
    {
        currentShop = shop;
        currentBuyer = buyer;
        IsOpen = true;
        IsOpen = true;
        PauseScreen.IsShopOpen = true;
        previousTimeScale = Time.timeScale;
        previousCursorLockState = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        transform.SetAsLastSibling();

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
            panelRoot.transform.SetAsLastSibling();
        }

        if (statusText != null)
        {
            statusText.text = string.Empty;
        }

        Rebuild();
    }

    public void Close()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        currentShop = null;
        currentBuyer = null;

        if (IsOpen)
        {
            IsOpen = false;
            PauseScreen.IsShopOpen = false;

            if (!PauseScreen.IsPaused && !TutorialMessage.IsTutorialActive)
            {
                Time.timeScale = previousTimeScale;
                Cursor.lockState = previousCursorLockState;
                Cursor.visible = previousCursorVisible;
            }
        }
    }

    private void Rebuild()
    {
        if (currentShop == null)
        {
            Close();
            return;
        }

        int currentScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetCurrentScore() : 0;
        int currentFloor = currentShop.GetCurrentFloor();

        if (titleText != null)
        {
            titleText.text = currentShop.ShopTitle;
        }

        if (pointsText != null)
        {
            pointsText.text = $"{currentScore} pts | Piso {currentFloor}";
        }

        PowerUpShopItem[] items = currentShop.Items;
        int itemCount = items != null ? items.Length : 0;

        for (int i = 0; i < itemButtons.Length; i++)
        {
            PowerUpShopItemButton itemButton = itemButtons[i];
            if (itemButton == null)
            {
                continue;
            }

            bool hasItem = i < itemCount && items[i] != null && items[i].IsConfigured();
            itemButton.gameObject.SetActive(hasItem);

            if (!hasItem)
            {
                continue;
            }

            PowerUpShopItem item = items[i];
            itemButton.SetLabel(BuildItemLabel(item, currentFloor));

            bool isAvailable = item.IsAvailableOnFloor(currentFloor) && currentScore >= item.Price;
            itemButton.SetAvailable(isAvailable);

            Button button = itemButton.Button;
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                PowerUpShopItem capturedItem = item;
                button.onClick.AddListener(() => Buy(capturedItem));
            }
        }

        if (statusText != null && itemCount > itemButtons.Length)
        {
            statusText.text = "Hay mas items configurados que botones en el grid.";
        }
    }

    private string BuildItemLabel(PowerUpShopItem item, int currentFloor)
    {
        if (!item.IsAvailableOnFloor(currentFloor))
        {
            return $"{item.GetDisplayName()}\n{item.Price} pts\nPiso {item.AvailableFromFloor}";
        }

        return $"{item.GetDisplayName()}\n{item.Price} pts";
    }

    private void Buy(PowerUpShopItem item)
    {
        if (currentShop == null || currentBuyer == null)
        {
            Close();
            return;
        }

        PowerUpShopPurchaseResult result = currentShop.TryBuy(item, currentBuyer);
        switch (result)
        {
            case PowerUpShopPurchaseResult.Purchased:
                SetStatus($"{item.GetDisplayName()} comprado.");
                if (currentShop.CloseAfterPurchase)
                {
                    Close();
                    return;
                }
                break;
            case PowerUpShopPurchaseResult.NotEnoughPoints:
                SetStatus("No tienes suficientes puntos.");
                break;
            case PowerUpShopPurchaseResult.LockedByFloor:
                SetStatus($"Disponible desde el piso {item.AvailableFromFloor}.");
                break;
            case PowerUpShopPurchaseResult.AlreadyOwnedOrUnavailable:
                SetStatus("Ese power up ya esta activo o no se puede aplicar.");
                break;
            default:
                SetStatus("No se pudo comprar ese power up.");
                break;
        }

        Rebuild();
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}
