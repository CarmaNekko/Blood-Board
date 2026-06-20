using UnityEngine;

public abstract class PermanentPowerUp : PowerUpBase
{
    [Header("Permanent Power Up")]
    [SerializeField] protected bool applyOnlyOnce = true;

    protected bool CanApplyPermanent(bool playerAlreadyHasIt)
    {
        if (!applyOnlyOnce)
        {
            return true;
        }

        return !playerAlreadyHasIt;
    }
}
