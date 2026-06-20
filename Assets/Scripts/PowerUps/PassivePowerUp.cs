using UnityEngine;

public abstract class PassivePowerUp : PowerUpBase
{
    [Header("Passive Power Up")]
    [SerializeField] protected bool applyOnlyOnce = true;
    [SerializeField] protected bool activateWhenCollected = true;

    protected bool CanApplyPassive(bool playerAlreadyHasIt)
    {
        if (!activateWhenCollected)
        {
            return false;
        }

        if (!applyOnlyOnce)
        {
            return true;
        }

        return !playerAlreadyHasIt;
    }
}
