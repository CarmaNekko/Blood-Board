using UnityEngine;

public abstract class TemporaryPowerUp : PowerUpBase
{
    [Header("Temporary Power Up")]
    [SerializeField] protected float duration = 30f;
}
