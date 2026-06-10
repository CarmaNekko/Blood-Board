using UnityEngine;

public class VampirismDrop : MonoBehaviour
{
    [SerializeField] private float duration = 30f;

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
                shooter.ActivateVampirism(duration);
                Destroy(gameObject);
            }
        }
    }
}