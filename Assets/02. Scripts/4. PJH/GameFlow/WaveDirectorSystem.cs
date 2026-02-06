using UnityEngine;

public class RuleDataContainer
{
    public StageConfigSO stageData; //스테이지 남은 시간
    public int playTime; //스테이지 진행시간 -> 타임오버

    public int currentPlayerHp; //플레이어 체력 -> RIP

    public int waveCount; //해당 라운드 웨이브 수
    public int waveIndex; //웨이브 클리어 확인
}

public class WaveDirectorSystem : Singleton<WaveDirectorSystem>
{
    /*
        스테이지 한정 싱글톤
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

   protected override void Awake()
    {
        base.Awake();
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
    private void SetData()
    {
        SetRule();
        ruleDataContainer.stageData = StageManager.Instance.selectDB;
    }

    private void SetRule()
    {
        ruleType = StageManager.Instance.selectDB.stageRule;
        ruleType.OnStart(ruleDataContainer ,this);
    }


    //실시간 데이터 연동
    private void ConnectData()
    {
        ruleDataContainer.playTime = StageFlowManager.Instance.playTime;
        ruleDataContainer.waveIndex = StageFlowManager.Instance.waveIndex;
        //ruleDataContainer.currentPlayerHp = 플레이어 스텟 연동
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
        //상태 전환 메니저에 보내는 값 : 아직 상태머신 작업되지 않음 구현만 되게 나중에 수정
        GameFlowManager.Instance.waveIndex = ruleDataContainer.waveIndex;
    }

    public void RoundClear()
    {
        StageManager.Instance.selectDB.clearResult = (ClearResult)1;
        GameFlowManager.Instance.RoundClear();
    }
    #endregion
}