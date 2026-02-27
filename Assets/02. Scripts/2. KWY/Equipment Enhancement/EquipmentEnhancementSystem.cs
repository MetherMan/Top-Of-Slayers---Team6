using TMPro;
using UnityEngine;

public class EquipmentEnhancementSystem : MonoBehaviour
{
    [SerializeField] int baseCost = 1000;
    [SerializeField] float multiplier = 1.2f;
    [SerializeField] int maxLevel = 10;

    [SerializeField] GameObject failPanel;
    [SerializeField] TextMeshProUGUI failText;

    public bool TryEnhance(InventoryItem targetItem)
    {
        if (targetItem == null) return false;

        if (targetItem.enhancementLevel >= maxLevel)
        {
            failPanel.SetActive(true);
            failText.text = "최대레벨은 강화를 할 수 없습니다.";
            return false;
        }

        int cost = GetCost(targetItem.enhancementLevel);

        if (!CurrencyManager.Instance.HasEnough(cost))
        {
            return false;
        }

        CurrencyManager.Instance.Spend(cost);


        targetItem.enhancementLevel++;

        InventoryManager.Instance.NotifyInventoryChanged();


        return true;
    }

    public int GetCost(int level)
    {
        if(level >= maxLevel)
        {
            return 0;
        }

        return Mathf.RoundToInt(baseCost * Mathf.Pow(multiplier, level));
    }

    public bool IsMaxLevel(int level)
    {
        return level >= maxLevel;
    }
}
