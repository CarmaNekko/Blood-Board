using UnityEngine;

public class LaserShotPowerUp : PermanentPowerUp
{
    protected override bool ApplyTo(MagicShooter shooter)
    {
        return shooter.UnlockLaserShot();
    }
}