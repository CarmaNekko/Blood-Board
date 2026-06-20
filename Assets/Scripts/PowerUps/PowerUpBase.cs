using UnityEngine;

public abstract class PowerUpBase : MonoBehaviour
{
    [Header("Power Up Catalog")]
    [SerializeField] private string powerUpId;
    [SerializeField] private string displayName;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        MagicShooter shooter = other.GetComponent<MagicShooter>();
        if (shooter == null)
        {
            shooter = other.GetComponentInChildren<MagicShooter>();
        }

        if (shooter != null && TryGrantTo(shooter))
        {
            Destroy(gameObject);
        }
    }

    public bool TryGrantTo(MagicShooter shooter)
    {
        return shooter != null && ApplyTo(shooter);
    }

    public string GetPowerUpId()
    {
        return powerUpId;
    }

    public string GetDisplayName()
    {
        return displayName;
    }

    protected abstract bool ApplyTo(MagicShooter shooter);
}
