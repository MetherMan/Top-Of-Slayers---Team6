using UnityEngine;

public class WaveDirectorSystem : MonoBehaviour
{
    /*
        !전략패턴 - 인터페이스

        웨이브 룰 : 라운드 시간을 공유 / 
        스폰 트리거 실행 :
    */

    #region field
    [Header("활성화 된 스테이지 룰")]
    [SerializeField] private WaveRule ruleType;
    public WaveRule RuleType
    {
        get { return ruleType; }
        private set
        {
            ruleType = value;
        }
    }
    #endregion

    void Awake()
    {
        //스테이지 값이 정해질 경우 실행
        //스테이지가 어떤 룰이 필요할 지 어디서 분별하고 가져올 것인가?
        if (StageManager.Instance.selectDB != null)
        {
            //SetRule();
        }
    }

    void Update()
    {
        if (ruleType != null)
        {
            //실패 조건
            ruleType.TimeOut();
            ruleType.HpZero(3); //플레이어 상태창 HP 연동필요

            //웨이브 진행 조건
            ruleType.EnemyDown();

            //클리어 조건
            ruleType.ClearRule();
        }
    }

    #region method
    public void SetRule(WaveRule newRule)
    {
        ruleType = newRule;
    }
    #endregion
}