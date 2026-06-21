using UnityEngine;

public class AnomalousSoulPowerUp : PermanentPowerUp
{
    protected override bool ApplyTo(MagicShooter shooter)
    {
        if (CanApplyPermanent(shooter.HasAnomalousSoul()))
        {
            return shooter.UnlockAnomalousSoul();
        }
        return false;
    }
}