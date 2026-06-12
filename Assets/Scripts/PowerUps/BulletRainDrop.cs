using UnityEngine;

public class BulletRainDrop : MonoBehaviour
{
    [SerializeField] private float duration = 20f;

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
                shooter.ActivateBulletRainAttack(duration);
                Destroy(gameObject);
            }
        }
    }
}