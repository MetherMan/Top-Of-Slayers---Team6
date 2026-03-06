using System.Collections.Generic;
using UnityEngine;

public class RewardSystem : MonoBehaviour
{
    public void GiveRewards(List<RewardData> rewards)
    {
        foreach (var reward in rewards)
        {
            if(reward.item != null)
            {
                InventoryManager.Instance.AddItem(reward.item, reward.amount);

            }
            if (reward.gold > 0)
            {
                CurrencyManager.Instance.Add(reward.gold);
            }
        }
    }
    public void GiveGold(int gold)
    {
        CurrencyManager.Instance.Add(gold);
    }
}
