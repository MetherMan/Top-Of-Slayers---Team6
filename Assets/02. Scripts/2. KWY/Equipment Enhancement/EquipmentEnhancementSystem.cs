using UnityEngine;

public class EquipmentEnhancementSystem : MonoBehaviour
{
    [SerializeField] int baseCost = 1000;
    [SerializeField] float multiplier = 1.2f;
    [SerializeField] int maxLevel = 10;
    public bool TryEnhance(InventoryItem targetItem)
    {
        if (targetItem == null) return false;

        if (targetItem.enhancementLevel >= maxLevel)
        {

            Debug.Log("최대 강화 도달");
            return false;
        }

        int cost = GetCost(targetItem.enhancementLevel);

        if (!CurrencyManager.Instance.HasEnough(cost))
        {
            Debug.Log("골드 부족");
            return false;
        }

        CurrencyManager.Instance.Spend(cost);


        targetItem.enhancementLevel++;



        Debug.Log($"강화 성공! 현재 레벨: {targetItem.enhancementLevel}");

        InventoryManager.Instance.NotifyInventoryChanged();


        return true;
    }

    public int GetCost(int level)
    {
        if(level >= maxLevel)
        {
            return -1;
        }

        return Mathf.RoundToInt(baseCost * Mathf.Pow(multiplier, level));
    }

    public bool IsMaxLevel(int level) 
    {
        return level >= maxLevel;
    }
}
