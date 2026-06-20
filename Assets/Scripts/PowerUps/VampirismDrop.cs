public class VampirismDrop : PassivePowerUp
{
    protected override bool ApplyTo(MagicShooter shooter)
    {
        if (!CanApplyPassive(shooter.HasVampirism()))
        {
            return false;
        }

        return shooter.UnlockVampirism();
    }
}
