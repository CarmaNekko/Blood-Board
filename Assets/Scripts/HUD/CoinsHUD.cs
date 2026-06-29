using TMPro;
using UnityEngine;

public class CoinsHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text coinsText;

    private void Awake()
    {
        if (coinsText == null)
        {
            coinsText = GetComponentInChildren<TMP_Text>();
            if (coinsText == null)
            {
                Debug.LogError("CoinsHUD: No TMP_Text component found. Please assign one in the Inspector or ensure one exists as a child.", this);
                enabled = false;
            }
        }
    }

    private void Update()
    {
        UpdateCoinsDisplay();
    }

    private void UpdateCoinsDisplay()
    {
        if (CoinManager.Instance != null && coinsText != null)
        {
            int coinCount = CoinManager.Instance.Coins;
            string suffix = (coinCount == 1) ? "moneda" : "monedas";
            coinsText.text = $"{coinCount} {suffix}";
        }
        else if (coinsText != null)
        {
            coinsText.text = "0 monedas";
        }
    }
}
