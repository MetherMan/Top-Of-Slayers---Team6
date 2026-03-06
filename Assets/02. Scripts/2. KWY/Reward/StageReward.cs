using System.Collections.Generic;
using UnityEngine;

public class StageReward : MonoBehaviour
{
    [SerializeField] public DropTable dropTable;
    [SerializeField] public int firstCleargold = 100;
    [SerializeField] public int repeatGold = 20;

    [SerializeField] RewardSystem rewardSystem;
    [SerializeField] RewardUI rewardUI;

    //stageflowManager구독
    private void OnEnable()
    {
        StageFlowManager.OnStageClear += StageClear;
    }

    private void OnDisable()
    {
        StageFlowManager.OnStageClear -= StageClear;
    }

    public void StageClear(bool isFirstClear)
    {
        List<RewardData> rewards = new List<RewardData>();

        //스테이지 클리어 보상 : false 미클리어 :: true 클리어
        if (!isFirstClear)
        {
            rewards = DropSystem.Calculate(dropTable);

            rewards.Add(new RewardData{gold = firstCleargold});
        }
        else
        {
            rewards.Add(new RewardData{gold = repeatGold});
        }
        rewardSystem.GiveRewards(rewards);

        rewardUI.ShowReward(rewards);
    }
    public void TestFirstClear()
    {
        StageClear(true);
    }
    public void TestRepeatClear()
    {
        StageClear(false);
    }

}
