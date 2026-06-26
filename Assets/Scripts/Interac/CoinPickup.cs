using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    public int Value = 1;

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            CoinManager.Instance.AddCoins(Value);
            Destroy(transform.parent.gameObject);
        }
    }
}