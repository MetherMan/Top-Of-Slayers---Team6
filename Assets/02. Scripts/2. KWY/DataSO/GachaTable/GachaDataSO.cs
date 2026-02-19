using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/GachaDataBase")]

public class GachaDataSO : ScriptableObject
{
    [System.Serializable]
    public class GachaEntry
    {
        public ItemSO item;
        public float chance;
    }
    public List<GachaEntry> items = new List<GachaEntry>();

    public float TotalChance()
    {
        float total = 0f;
        foreach (var item in items)
        {
            total += item.chance;
        }
        return total;
    }
}
