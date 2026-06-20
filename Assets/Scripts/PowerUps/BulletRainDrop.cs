public class BulletRainDrop : TemporaryPowerUp
{
    protected override bool ApplyTo(MagicShooter shooter)
    {
        shooter.ActivateBulletRainAttack(duration);
        return true;
    }
}
