using UnityEngine;

public class GachaCalculator
{
    public static ItemSO Roll(GachaDataSO data)
    {
        float rand = Random.Range(0f, 100f);

        // 1️⃣ 등급 결정
        if (rand < data.legendChance)
        {
            return GetRandomItem(data.legendItems);
        }
        else if (rand < data.legendChance + data.epicChance)
        {
            return GetRandomItem(data.epicItems);
        }
        else
        {
            return GetRandomItem(data.normalItems);
        }
    }

    private static ItemSO GetRandomItem(System.Collections.Generic.List<ItemSO> list)
    {
        if (list == null || list.Count == 0)
        {
            Debug.LogError("❌ 해당 등급 아이템 없음");
            return null;
        }

        int index = Random.Range(0, list.Count);
        return list[index];
    }
}

