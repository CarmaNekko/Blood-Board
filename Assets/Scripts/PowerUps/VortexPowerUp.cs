using UnityEngine;

public class VortexPowerUp : PermanentPowerUp
{
    [SerializeField, Range(0.05f, 1f)] private float manaCostPercent = 0.20f;

    protected override bool ApplyTo(MagicShooter shooter)
    {
        return shooter.UnlockVortexAttack(manaCostPercent);
    }
}