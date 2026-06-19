using UnityEngine;

[System.Serializable]
public class LootItem
{
    public GameObject Prefab;

    [Range(0, 100)]
    public float DropChance;
}