using System;
using BloodBoard.GameManagement;
using UnityEngine;

[Serializable]
public class PowerUpShopItem
{
    [SerializeField] private string displayNameOverride;
    [SerializeField] private GameObject powerUpPrefab;
    [Min(0)]
    [SerializeField] private int price = 100;
    [Min(1)]
    [SerializeField] private int availableFromFloor = 1;

    public GameObject PowerUpPrefab => powerUpPrefab;
    public int Price => price;
    public int AvailableFromFloor => availableFromFloor;

    public string GetDisplayName()
    {
        if (!string.IsNullOrWhiteSpace(displayNameOverride))
        {
            return displayNameOverride;
        }

        PowerUpBase powerUp = powerUpPrefab != null ? powerUpPrefab.GetComponent<PowerUpBase>() : null;
        if (powerUp != null && !string.IsNullOrWhiteSpace(powerUp.GetDisplayName()))
        {
            return powerUp.GetDisplayName();
        }

        return powerUpPrefab != null ? powerUpPrefab.name : "Power Up";
    }

    public bool IsConfigured()
    {
        return powerUpPrefab != null && powerUpPrefab.GetComponent<PowerUpBase>() != null;
    }

    public bool IsAvailableOnFloor(int floor)
    {
        return floor >= availableFromFloor;
    }
}

public class PowerUpShopInteractable : InteractableBase
{
    [Header("Shop")]
    [SerializeField] private string shopTitle = "Tienda de Power Ups";
    [SerializeField] private PowerUpShopItem[] items;
    [SerializeField] private PowerUpShopUI shopUI;
    [SerializeField] private bool closeAfterPurchase = true;

    protected override void OnInteract(PlayerInteractor interactor)
    {
        if (interactor == null || interactor.Shooter == null)
        {
            Debug.LogWarning("PowerUpShopInteractable: el jugador no tiene MagicShooter.");
            return;
        }

        PowerUpShopUI targetUI = shopUI != null ? shopUI : FindFirstObjectByType<PowerUpShopUI>();
        if (targetUI == null)
        {
            Debug.LogWarning("PowerUpShopInteractable: no hay PowerUpShopUI asignada en escena.");
            return;
        }

        targetUI.Open(this, interactor);
    }

    public string ShopTitle => shopTitle;
    public PowerUpShopItem[] Items => items;
    public bool CloseAfterPurchase => closeAfterPurchase;

    public int GetCurrentFloor()
    {
        return Mathf.Max(1, LevelManager.currentLevel);
    }

    public PowerUpShopPurchaseResult TryBuy(PowerUpShopItem item, PlayerInteractor buyer)
    {
        if (item == null || buyer == null || buyer.Shooter == null)
        {
            return PowerUpShopPurchaseResult.Invalid;
        }

        CoinManager coinManager = CoinManager.Instance;
        if (coinManager == null) return PowerUpShopPurchaseResult.Invalid;

        if (!item.IsConfigured())
        {
            return PowerUpShopPurchaseResult.Invalid;
        }

        if (!item.IsAvailableOnFloor(GetCurrentFloor()))
        {
            return PowerUpShopPurchaseResult.LockedByFloor;
        }

        PowerUpBase powerUpToBuy = item.PowerUpPrefab.GetComponent<PowerUpBase>();
        if (powerUpToBuy == null)
        {
            return PowerUpShopPurchaseResult.Invalid;
        }

        if (!coinManager.CanAffordCoins(item.Price))
        {
            return PowerUpShopPurchaseResult.NotEnoughCoins;
        }

        if (coinManager.SpendCoins(item.Price))
        {
            if (powerUpToBuy.TryGrantTo(buyer.Shooter))
            {
                return PowerUpShopPurchaseResult.Purchased;
            }
            else
            {
                coinManager.AddCoins(item.Price);
                return PowerUpShopPurchaseResult.AlreadyOwnedOrUnavailable;
            }
        }
        else
        {
            return PowerUpShopPurchaseResult.NotEnoughCoins;
        }
    }
    private bool IsChargedAttack(PowerUpShopItem item)
    {
        if (item?.PowerUpPrefab == null) return false;
        string name = item.PowerUpPrefab.name.ToLower();
        return name.Contains("slash") || name.Contains("vortex");
    }
    private bool IsChargedAttack(PowerUpBase powerUp)
    {
        if (powerUp == null) return false;
        string typeName = powerUp.GetType().Name.ToLower();
        return typeName.Contains("slash") || typeName.Contains("vortex");
    }
}

public enum PowerUpShopPurchaseResult
{
    Purchased,
    NotEnoughCoins,
    LockedByFloor,
    AlreadyOwnedOrUnavailable,
    Invalid
}
