using UnityEngine;

public abstract class WaveRule : ScriptableObject
{
    /*
        공통적으로 적용되는 룰
    */
    #region field

    #endregion

    #region method
    public abstract void OnStart(RuleDataContainer data, WaveDirectorSystem context);

    public abstract void OnUpdate(RuleDataContainer data, WaveDirectorSystem context);

    //클리어 시 수집해야할 데이터 전송
    public abstract void OnExit(RuleDataContainer data, WaveDirectorSystem context);

    //실패 조건
    public void TimeOver(RuleDataContainer data, WaveDirectorSystem context)
    {
        if (data.playTime >= data.stageData.stageTime)
        {
            context.TimeOver();
        }
    }

    public void HpZero(RuleDataContainer data, WaveDirectorSystem context)
    {
        //플레이어 체력 '0' 이하 일 경우 게임 오버
        //context.HpZero();
    }

    //웨이브 진행 조건
    public void WaveClear(RuleDataContainer data, WaveDirectorSystem context)
    {
        context.WaveClear();
    }

    //클리어 조건
    public abstract void RoundClear(RuleDataContainer data, WaveDirectorSystem context);
    #endregion
}