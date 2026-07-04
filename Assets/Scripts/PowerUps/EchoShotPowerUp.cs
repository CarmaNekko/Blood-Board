using UnityEngine;

public class EchoShotPowerUp : PassivePowerUp
{
    [Header("Echo Shot Settings")]
    [SerializeField, Range(0f, 100f)] private float chancePercent = 10f;

    protected override bool ApplyTo(MagicShooter shooter)
    {
        if (!CanApplyPassive(shooter.HasEchoShot()))
        {
            return false;
        }

        return shooter.UnlockEchoShot(chancePercent);
    }
}
