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
    private bool isRoundClearResolved;
    #endregion

    protected override void Awake()
    {
        base.Awake();

        var stageManager = StageManager.Instance;
        if (stageManager == null || stageManager.selectDB == null)
        {
            Debug.LogWarning("StageManager.selectDB가 없어 WaveDirectorSystem 초기화를 보류합니다.");
            return;
        }

        SetData();
    }

    void Update()
    {
        ConnectData();
        if (ruleType != null) ruleType.OnUpdate(ruleDataContainer, this);

        //ruleType.OnExit(ruleDataContainer, this);
    }

    #region method
    private void SetData()
    {
        SetRule();

        var stageManager = StageManager.Instance;
        if (stageManager == null) return;

        ruleDataContainer.stageData = stageManager.selectDB;
        isRoundClearResolved = false;
    }

    private void SetRule()
    {
        var stageManager = StageManager.Instance;
        if (stageManager == null || stageManager.selectDB == null) return;

        ruleType = stageManager.selectDB.stageRule;
        if (ruleType == null) return;

        ruleType.OnStart(ruleDataContainer, this);
    }


    //실시간 데이터 연동
    private void ConnectData()
    {
        var stageFlow = StageFlowManager.Instance;
        if (stageFlow == null) return;

        ruleDataContainer.playTime = stageFlow.playTime;
        ruleDataContainer.waveIndex = stageFlow.waveIndex;
        //ruleDataContainer.currentPlayerHp = 플레이어 스텟 연동
    }

    public void TimeOver()
    {
        var stageManager = StageManager.Instance;
        if (stageManager == null || stageManager.selectDB == null) return;

        stageManager.selectDB.clearResult = (ClearResult)2;
    }

    public void HpZero()
    {
        var stageManager = StageManager.Instance;
        if (stageManager == null || stageManager.selectDB == null) return;

        stageManager.selectDB.clearResult = (ClearResult)2;
    }

    public void WaveClear()
    {
        //웨이브 전환 시점에만 호출
        var gameFlow = GameFlowManager.Instance;
        if (gameFlow == null) return;

        gameFlow.waveIndex = ruleDataContainer.waveIndex;
    }

    public void RoundClear()
    {
        if (isRoundClearResolved) return;

        isRoundClearResolved = true;

        var stageManager = StageManager.Instance;
        if (stageManager != null && stageManager.selectDB != null)
        {
            stageManager.selectDB.clearResult = (ClearResult)1;
        }
        else
        {
            Debug.LogWarning("RoundClear 호출 시 StageManager.selectDB가 null입니다.");
        }

        var gameFlow = GameFlowManager.Instance;
        if (gameFlow == null)
        {
            Debug.LogWarning("RoundClear 호출 시 GameFlowManager가 null입니다.");
            return;
        }

        gameFlow.RoundClear();
    }
    #endregion
}
