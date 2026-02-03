using UnityEngine;

[CreateAssetMenu(fileName = "Rule_", menuName = "Rule/BossRule")]
public class BossRule : WaveRule
{
    /*
        보스 스테이지 클리어 룰
    */
    #region field
    #endregion

    public void Awake()
    {
        
    }

    #region method
    //클리어 조건
    public override void ClearRule()
    {
        //보스 몬스터 다운
        //StageManager.Instance.selectDB.RoundData.boss
    }
    #endregion
}
