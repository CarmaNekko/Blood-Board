using UnityEngine;

public class Chest : DestructiblePillar
{
    [Header("Loot")]
    public LootItem[] LootTable;
    public float lootHeightOffset = 1f;

    protected override void Shatter()
    {
        DropLoot();
        base.Shatter();
        Destroy(gameObject, 5f);
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
