using UnityEngine;

[CreateAssetMenu(fileName = "Rule_", menuName = "Rule/BossRule")]
public class BossRule : WaveRule
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
        //보스 몬스터 다운
        context.RoundClear();
    }
    #endregion
}
