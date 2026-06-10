using UnityEngine;

public class PowerUpDrop : MonoBehaviour
{
    [SerializeField] private float duration = 60f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MagicShooter shooter = other.GetComponent<MagicShooter>();
            if (shooter == null)
            {
                shooter = other.GetComponentInChildren<MagicShooter>();
            }

            if (shooter != null)
            {
                shooter.ActivateHarmonicPowerUp(duration);
                Destroy(gameObject);
            }
        }
    }
}