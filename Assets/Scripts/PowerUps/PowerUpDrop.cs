public class PowerUpDrop : TemporaryPowerUp
{
    protected override bool ApplyTo(MagicShooter shooter)
    {
        shooter.ActivateHarmonicPowerUp(duration);
        return true;
    }
}
