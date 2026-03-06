using UnityEngine;

[System.Serializable]
public class DropItem
{
    public ItemSO item;
    public int amount;

    [Range(0f, 1f)]
    public float dropChance;
}
