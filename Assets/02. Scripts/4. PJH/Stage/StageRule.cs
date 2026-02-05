using UnityEngine;

[CreateAssetMenu(fileName = "Rule_", menuName = "Rule/StageRule")]
public class StageRule : WaveRule
{
    #region method
    public override void OnStart(RuleDataContainer data, WaveDirectorSystem context)
    {
        
    }

    public override void OnUpdate(RuleDataContainer data, WaveDirectorSystem context)
    {
        TimeOver(data, context);
        HpZero(data, context);
        WaveClear(data, context);
        RoundClear(data, context);
    }

    public override void OnExit(RuleDataContainer data, WaveDirectorSystem context)
    {
        
    }

    //클리어 조건
    public override void RoundClear(RuleDataContainer data, WaveDirectorSystem context)
    {
        //StageConfigSO -> StageDatabase -> StageManager -> ObjectPool -> EnemySpawner
        //EnemyDown() x++ 변수 증감 -> StageDatabase.Instance.stageLound 변수와 if문 비교
        context.RoundClear();
    }
    #endregion
}