using UnityEngine;

public class Chest : MonoBehaviour
{
    public LootItem[] LootTable;
    public int Health = 1;

    public void TakeDamage(int damage)
    {
        Health -= damage;

        if (Health <= 0)
        {
            DropLoot();
            Destroy(gameObject);
        }
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
                Instantiate(loot.Prefab, transform.position, Quaternion.identity);

                break;
            }
        }
    }
}
