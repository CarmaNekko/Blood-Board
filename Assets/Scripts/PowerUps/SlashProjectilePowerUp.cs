using UnityEngine;

public class SlashProjectilePowerUp : PermanentPowerUp
{
    [Header("Slash Mana Cost")]
    [Range(0.05f, 0.5f)]
    [SerializeField] private float manaCostPercentage = 0.20f;

    protected override bool ApplyTo(MagicShooter shooter)
    {
        if (!CanApplyPermanent(shooter.HasSlashAttack()))
        {
            return false;
        }

        return shooter.UnlockSlashAttack(manaCostPercentage);
    }
}
