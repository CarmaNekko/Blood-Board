using UnityEngine;

public static class PowerUpRewardRoller
{
    public static bool TryGrantRandom(MagicShooter shooter, GameObject[] powerUpPrefabs)
    {
        if (shooter == null || powerUpPrefabs == null || powerUpPrefabs.Length == 0)
        {
            return false;
        }

        GameObject prefab = GetRandomValidPowerUp(powerUpPrefabs);
        if (prefab == null)
        {
            return false;
        }

        PowerUpBase powerUp = prefab.GetComponent<PowerUpBase>();
        return powerUp != null && powerUp.TryGrantTo(shooter);
    }

    public static GameObject PickChestPowerUp(GameObject[] temporaryPowerUps, GameObject[] permanentAndPassivePowerUps, float temporaryChance, float permanentOrPassiveChance)
    {
        float roll = Random.Range(0f, 100f);

        if (roll < permanentOrPassiveChance)
        {
            return GetRandomValidPowerUp(permanentAndPassivePowerUps);
        }

        if (roll < permanentOrPassiveChance + temporaryChance)
        {
            return GetRandomValidPowerUp(temporaryPowerUps);
        }

        return null;
    }

    private static GameObject GetRandomValidPowerUp(GameObject[] powerUpPrefabs)
    {
        if (powerUpPrefabs == null || powerUpPrefabs.Length == 0)
        {
            return null;
        }

        int startIndex = Random.Range(0, powerUpPrefabs.Length);
        for (int i = 0; i < powerUpPrefabs.Length; i++)
        {
            GameObject prefab = powerUpPrefabs[(startIndex + i) % powerUpPrefabs.Length];
            if (prefab != null && prefab.GetComponent<PowerUpBase>() != null)
            {
                return prefab;
            }
        }

        return null;
    }
}
