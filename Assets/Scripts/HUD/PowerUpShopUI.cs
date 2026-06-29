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
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text statusText;

    [Header("Grid Slots")]
    [SerializeField] private PowerUpShopItemButton itemButtonPrefab;
    [SerializeField] private Transform itemButtonsContainer;

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

        int currentCoins = CoinManager.Instance != null ? CoinManager.Instance.Coins : 0;
        int currentFloor = currentShop.GetCurrentFloor();

        if (titleText != null)
        {
            titleText.text = currentShop.ShopTitle;
        }

        if (coinsText != null)
        {
            coinsText.text = $"{currentCoins} monedas | Piso {currentFloor}";
        }
        foreach (Transform child in itemButtonsContainer)
        {
            Destroy(child.gameObject);
        }

        PowerUpShopItem[] items = currentShop.Items;
        if (items == null) return;

        foreach (PowerUpShopItem item in items)
        {
            if (item == null || !item.IsConfigured() || currentFloor < item.AvailableFromFloor)
            {
                continue;
            }
            PowerUpShopItemButton itemButton = Instantiate(itemButtonPrefab, itemButtonsContainer);
            itemButton.SetLabel(BuildItemLabel(item, currentFloor));
            bool canAfford = currentCoins >= item.Price;
            itemButton.SetAvailable(canAfford);

            Button button = itemButton.Button;
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                PowerUpShopItem capturedItem = item;
                button.onClick.AddListener(() => Buy(capturedItem));
            }
        }
    }

    private string BuildItemLabel(PowerUpShopItem item, int currentFloor)
    {
        return $"{item.GetDisplayName()}\n{item.Price} monedas";
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
                SaveManager.SavePowerUpPurchase(GetPowerUpTypeFromPrefab(item.PowerUpPrefab));
                if (currentShop.CloseAfterPurchase)
                {
                    Close();
                    return;
                }
                break;
            case PowerUpShopPurchaseResult.NotEnoughCoins:
                SetStatus("No tienes suficientes monedas.");
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

    private string GetPowerUpTypeFromPrefab(GameObject prefab)
    {
        if (prefab == null) return "";

        string name = prefab.name.ToLower();

        if (name.Contains("anomalous") || name.Contains("soul")) return "anomalousSoul";
        if (name.Contains("slash")) return "slashAttack";
        if (name.Contains("vortex")) return "vortexAttack";
        if (name.Contains("vampirism")) return "vampirism";
        if (name.Contains("bullet") && name.Contains("rain")) return "bulletRain";
        if (name.Contains("health")) return "health";
        if (name.Contains("damage")) return "damage";
        if (name.Contains("speed")) return "speed";

        return "";
    }
}