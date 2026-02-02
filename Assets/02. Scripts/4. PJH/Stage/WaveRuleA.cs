using UnityEngine;

public class WaveRuleA : WaveRule
{
    /*
        A type rule : 일반적인 모든 적 다운
    */
    #region field
    #endregion

    public void Awake()
    {
        
    }

    #region method
    //웨이브 진행 조건
    public void EnemyDown()
    {
        //StageConfigSO 각 웨이브 몬스터가 전부 다운 될 경우 조건변수 증감
        cleardWaveNum++;
    }
    #endregion
}
