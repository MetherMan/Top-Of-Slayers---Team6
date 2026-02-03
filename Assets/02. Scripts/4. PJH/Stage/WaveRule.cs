using UnityEngine;

public abstract class WaveRule : ScriptableObject
{
    /*
        공통적으로 적용되는 룰
    */
    #region field
    //조건 체크 변수
    

    #endregion

    #region method
    //실패 조건
    public void TimeOut()
    {
        if (StageFlowManager.Instance.remainingTime <= StageFlowManager.Instance.playTime)
        {
            //게임오버

            //
            StageManager.Instance.selectDB.clearResult = (ClearResult)2;
        }
    }

    public void HpZero(int hp)
    {
        //플레이어 체력 '0' 이하 일 경우 게임 오버
        if (hp <= 0)
        {
            StageManager.Instance.selectDB.clearResult = (ClearResult)2;
        }
    }

    //웨이브 진행 조건
    public void EnemyDown()
    {
        //StageConfigSO 각 웨이브 몬스터가 전부 다운 될 경우 조건변수 증감
        StageManager.Instance.cleardWaveCount++;
    }

    //클리어 조건
    public abstract void ClearRule();
    #endregion
}