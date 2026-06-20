using UnityEngine;

public class Chest : DestructiblePillar
{
    [Header("Loot")]
    public LootItem[] LootTable;
    public float lootHeightOffset = 1f;

    [Header("Power Up Loot")]
    [SerializeField] private GameObject[] temporaryPowerUps;
    [SerializeField] private GameObject[] permanentAndPassivePowerUps;
    [SerializeField, Range(0f, 100f)] private float temporaryPowerUpChance = 20f;
    [SerializeField, Range(0f, 100f)] private float permanentOrPassivePowerUpChance = 5f;

    protected override void Shatter()
    {
        if (!TryDropPowerUp())
        {
            DropLoot();
        }

        base.Shatter();
        Destroy(gameObject, 5f);
    }
    private bool TryDropPowerUp()
    {
        GameObject powerUp = PowerUpRewardRoller.PickChestPowerUp(
            temporaryPowerUps,
            permanentAndPassivePowerUps,
            temporaryPowerUpChance,
            permanentOrPassivePowerUpChance);

        if (powerUp == null)
        {
            return false;
        }

        Vector3 spawnPosition = transform.position + Vector3.up * lootHeightOffset;
        Instantiate(powerUp, spawnPosition, Quaternion.identity);
        return true;
    }

    private void DropLoot()
    {
        float randomValue = Random.Range(0f, 100f);
        float currentChance = 0f;

        foreach (LootItem loot in LootTable)
        {
            currentChance += loot.DropChance;

            if (randomValue <= currentChance)
            {
                Vector3 spawnPosition = transform.position + Vector3.up * lootHeightOffset;
                Instantiate(loot.Prefab, spawnPosition, Quaternion.identity);

                break;
            }
        }
    }
}
