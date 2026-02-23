using UnityEngine;

public class GachaCalculator
{
    public static ItemSO Roll(GachaDataSO data)
    {
        float total = data.TotalChance();
        float rand = Random.Range(0, total);
        float current = 0;

        foreach(var item in data.items)
        {
            current += item.chance;

            if(rand <= current)
            {
                return item.item;
            }
        }
        return null;
    }
}

