using UnityEngine;

[CreateAssetMenu(fileName = "Rule_", menuName = "Rule/StageRule")]
public class StageRule : WaveRule
{
    #region field
    /*
        일반 스테이지 적용 룰
    */
    #endregion

    #region method
    //클리어 조건
    public override void ClearRule()
    {
        //웨이브 진행상황을 어떻게 확인할 것인가?
        //StageConfigSO -> StageDatabase -> StageManager -> ObjectPool -> EnemySpawner
        //EnemyDown() x++ 변수 증감 -> StageDatabase.Instance.stageLound 변수와 if문 비교
        if (StageDatabase.Instance.stageLound == StageManager.Instance.cleardWaveCount)
        {
            //스테이지 클리어
            StageManager.Instance.selectDB.clearResult = (ClearResult)1;
        }
    }
    #endregion
}