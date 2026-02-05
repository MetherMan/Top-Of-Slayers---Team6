using UnityEngine;

public class RuleDataContainer
{
    public StageConfigSO stageData; //스테이지 기본자료
    public int playTime; //스테이지 진행시간 -> 타임오버
    public int currentPlayerHp; //플레이어 체력 -> RIP
    public int killCount; //웨이브 진행 조건
}

public class WaveDirectorSystem : MonoBehaviour
{
    /*
        !전략패턴 룰 매니저

        스테이지 맵 Hierarchy에 생성
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

    [Header("스테이지 실시간 데이터 연동")]
    RuleDataContainer ruleDataContainer = new RuleDataContainer();
    #endregion

    void Awake()
    {
        //스테이지 값이 정해질 경우 실행
        if (StageManager.Instance.selectDB != null)
        {
           SetData();
        }
    }

    void Update()
    {
        if (ruleType != null) ruleType.OnUpdate(ruleDataContainer ,this);
        ConnectData();

        //ruleType.OnExit(ruleDataContainer, this);
    }

    #region method
    public void SetData()
    {
        SetRule();
        ruleDataContainer.stageData = StageManager.Instance.selectDB;

    }

    public void SetRule()
    {
        ruleType = StageManager.Instance.selectDB.stageRule;
        ruleType.OnStart(ruleDataContainer ,this);
    }


    //실시간 데이터 연동
    public void ConnectData()
    {
        ruleDataContainer.playTime = StageFlowManager.Instance.playTime;
        //ruleDataContainer.currentPlayerHp = 플레이어 스텟 연동
        //ruleDataContainer.killCount = 오브젝트 풀, 에너미 스포너 완료 후 작성
    }

    public void TimeOver()
    {
        StageManager.Instance.selectDB.clearResult = (ClearResult)2;
    }

    public void HpZero()
    {
        StageManager.Instance.selectDB.clearResult = (ClearResult)2;
    }

    public void WaveClear()
    {

    }

    public void RoundClear()
    {
        StageManager.Instance.selectDB.clearResult = (ClearResult)1;
    }
    #endregion
}