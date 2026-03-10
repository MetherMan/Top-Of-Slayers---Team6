using System;
using System.Collections;
using UnityEngine;

public class RuleDataContainer
{
    public StageConfigSO stageData; //스테이지 남은 시간
    public int playTime; //스테이지 진행시간 -> 타임오버

    public int currentPlayerHp = -1; //플레이어 체력 -> RIP

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
    private PlayerHP playerHp;

    public event Action<int> OnWaveClear;
    public event Action OnRoundClear;
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

        Init(stageManager);
    }

    void Update()
    {
        ConnectData();
        if (ruleType != null) ruleType.OnUpdate(ruleDataContainer, this);

    }

    #region method
    private void Init(StageManager stageManager)
    {
        SetRule();

        ruleDataContainer.stageData = stageManager.selectDB;
        ruleDataContainer.waveCount = stageManager.selectDB.roundDatas != null
            ? stageManager.selectDB.roundDatas.Count
            : 0;
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
        ruleDataContainer.waveCount = stageFlow.waveLength;

        if (playerHp == null)
        {
            playerHp = FindFirstObjectByType<PlayerHP>();
        }

        if (playerHp != null)
        {
            ruleDataContainer.currentPlayerHp = playerHp.currentHP;
        }
    }
    #endregion

    #region 룰
    public void TimeOver()
    {
        var stageManager = StageManager.Instance;
        if (stageManager == null || stageManager.selectDB == null) return;
        if (stageManager.selectDB.clearResult != ClearResult.None) return;

        stageManager.selectDB.clearResult = (ClearResult)2;
    }

    public void HpZero()
    {
        var stageManager = StageManager.Instance;
        if (stageManager == null || stageManager.selectDB == null) return;
        if (stageManager.selectDB.clearResult != ClearResult.None) return;

        stageManager.selectDB.clearResult = (ClearResult)2;
    }

    public void WaveClear()
    {
        //웨이브 전환 시점에만 호출
        var gameFlow = GameFlowManager.Instance;
        if (gameFlow == null) return;

        gameFlow.waveIndex = ruleDataContainer.waveIndex;

        Debug.Log($"라운드 클리어{ruleDataContainer.waveIndex}");
        OnWaveClear?.Invoke(ruleDataContainer.waveIndex +1);
    }

    public void RoundClear()
    {
        Debug.Log("웨이브시스템 스테이지 클리어");
        if (isRoundClearResolved) return;

        StageManager stageManager = StageManager.Instance;
        if (stageManager == null || stageManager.selectDB == null)
        {
            Debug.LogWarning("RoundClear 호출 시 StageManager.selectDB가 null입니다.");
            return;
        }

        if (stageManager.selectDB.clearResult != ClearResult.None) return;

        isRoundClearResolved = true;
        stageManager.selectDB.clearResult = (ClearResult)1;

        StageFlowManager stageFlow = StageFlowManager.Instance;
        if (stageFlow == null)
        {
            Debug.LogWarning("RoundClear 호출 시 StageFlowManager가 null입니다.");
            return;
        }

        stageFlow.RoundClear();

        ruleType.OnExit(ruleDataContainer, this);

        OnRoundClear?.Invoke();
    }
    #endregion
}
