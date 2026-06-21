using UnityEngine;

public class Chest : Destruction
{
    [Header("Loot")]
    public LootItem[] LootTable;
    public float lootHeightOffset = 1f;

    [Header("Power Up Loot")]
    [SerializeField] private GameObject[] temporaryPowerUps;
    [SerializeField] private GameObject[] permanentAndPassivePowerUps;
    [SerializeField, Range(0f, 100f)] private float temporaryPowerUpChance = 20f;
    [SerializeField, Range(0f, 100f)] private float permanentOrPassivePowerUpChance = 5f;

    private bool isOpened = false;

    public override void DamageAtPoint(Vector3 impactPoint, float radius, float force)
    {
        if (isOpened) return;
        isOpened = true;

        DropLoot();
        ShatterFull(impactPoint, force);
    }

    private void DropLoot()
    {
        float randomValue = Random.Range(0f, 100f);
        float currentChance = 0f;

        if (LootTable != null)
        {
            foreach (LootItem loot in LootTable)
            {
                if (loot == null || loot.Prefab == null)
                {
                    continue;
                }

                currentChance += loot.DropChance;

                if (randomValue <= currentChance)
                {
                    SpawnLoot(loot.Prefab);
                    return;
                }
            }
        }

        currentChance += temporaryPowerUpChance;
        if (randomValue <= currentChance && TryDropPowerUp(temporaryPowerUps))
        {
            return;
        }

        currentChance += permanentOrPassivePowerUpChance;
        if (randomValue <= currentChance)
        {
            TryDropPowerUp(permanentAndPassivePowerUps);
        }
    }

    private bool TryDropPowerUp(GameObject[] powerUps)
    {
        GameObject powerUp = PowerUpRewardRoller.PickRandomValidPowerUp(powerUps);
        if (powerUp == null)
        {
            return false;
        }

        SpawnLoot(powerUp);
        return true;
    }

    private void SpawnLoot(GameObject prefab)
    {
        Vector3 spawnPosition = transform.position + Vector3.up * lootHeightOffset;
        Instantiate(prefab, spawnPosition, Quaternion.identity);
    }
}