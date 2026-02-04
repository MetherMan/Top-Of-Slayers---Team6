using UnityEngine;

[CreateAssetMenu(fileName = "Rule_", menuName = "Rule/ChellengeStage")]
public class ChellengeStage : WaveRule
{
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

    public override void RoundClear(RuleDataContainer data, WaveDirectorSystem context)
    {
        //
        context.RoundClear();
    }
}
