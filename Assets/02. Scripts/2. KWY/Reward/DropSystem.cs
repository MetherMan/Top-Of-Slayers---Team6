using System.Collections.Generic;

public static class DropSystem
{
    public static List<RewardData> Calculate(DropTable table)
    {
        List<RewardData> rewards = new List<RewardData>();

        foreach(var drop in table.items)
        {
            if (DropCalculator.Roll(drop.dropChance))
            {
                rewards.Add(new RewardData
                {
                    item = drop.item,
                    amount = drop.amount
                });
            }
        }
        return rewards;
    }
}
