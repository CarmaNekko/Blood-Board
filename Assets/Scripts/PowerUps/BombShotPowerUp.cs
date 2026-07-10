using UnityEngine;

public class BombShotPowerUp : PermanentPowerUp
{
    [Header("Bomb Mana Cost")]
    [SerializeField] private float extraManaCost = 10f;

    protected override bool ApplyTo(MagicShooter shooter)
    {
        return shooter.UnlockBombShot(extraManaCost);
    }
}