using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RuleDataContainer
{
    public StageConfigSO stageData; //스테이지 남은 시간
    public int playTime; //스테이지 진행시간 -> 타임오버

    public int currentPlayerHp = -1; //플레이어 체력 -> RIP

    public int waveCount; //해당 라운드 웨이브 수
    public int waveIndex; //현재 진행 중인 웨이브
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
    private readonly RuleDataContainer ruleDataContainer = new RuleDataContainer();
    private bool isStageResultResolved;
    private bool isInitialized;
    private PlayerHP playerHp;
    private PlayerCombatResource playerCombatResource;
    private bool isPlayerDeathSubscribed;
    private bool isPlayerHpSubscribed;

    public event Action<int> OnWaveClear;
    public event Action OnRoundClear;
    #endregion

    protected override void Awake()
    {
        base.Awake();

        StageManager stageManager = StageManager.Instance;
        if (stageManager == null || stageManager.selectDB == null)
        {
            Debug.LogWarning("StageManager.selectDB가 없어 WaveDirectorSystem 초기화를 보류합니다.");
            return;
        }

        Init(stageManager);
    }

    private void Update()
    {
        ConnectData();
        EnsureInitialized();
        if (ruleType != null)
        {
            ruleType.OnUpdate(ruleDataContainer, this);
        }
    }

    #region method
    private void Init(StageManager stageManager)
    {
        if (stageManager == null || stageManager.selectDB == null)
        {
            isInitialized = false;
            return;
        }

        SetRule();

        ruleDataContainer.stageData = stageManager.selectDB;
        ruleDataContainer.waveCount = stageManager.selectDB.roundDatas != null
            ? stageManager.selectDB.roundDatas.Count
            : 0;
        ruleDataContainer.currentPlayerHp = -1;
        isStageResultResolved = false;
        UnsubscribePlayerHp();
        UnsubscribePlayerDeath();
        playerHp = null;
        playerCombatResource = null;
        isInitialized = true;
        ConnectData();
    }

    private void SetRule()
    {
        StageManager stageManager = StageManager.Instance;
        if (stageManager == null || stageManager.selectDB == null) return;

        ruleType = stageManager.selectDB.stageRule;
        if (ruleType == null) return;

        ruleType.OnStart(ruleDataContainer, this);
    }

    //실시간 데이터 연동
    private void ConnectData()
    {
        StageFlowManager stageFlow = StageFlowManager.Instance;
        if (stageFlow == null) return;

        ruleDataContainer.playTime = stageFlow.playTime;
        ruleDataContainer.waveIndex = stageFlow.waveIndex;
        ruleDataContainer.waveCount = stageFlow.waveLength;

        if (playerCombatResource == null)
        {
            playerCombatResource = FindStagePlayerCombatResource();
            SubscribePlayerDeath();
        }

        if (playerHp == null)
        {
            playerHp = FindStagePlayerHp();
            SubscribePlayerHp();
        }

        if (playerCombatResource != null)
        {
            ruleDataContainer.currentPlayerHp = playerCombatResource.CurrentHp;
            return;
        }

        if (playerHp != null)
        {
            ruleDataContainer.currentPlayerHp = playerHp.currentHP;
            return;
        }

        ruleDataContainer.currentPlayerHp = -1;
    }

    private void EnsureInitialized()
    {
        if (isInitialized && ruleType != null && ruleDataContainer.stageData != null)
        {
            return;
        }

        StageManager stageManager = StageManager.Instance;
        if (stageManager == null || stageManager.selectDB == null)
        {
            return;
        }

        Init(stageManager);
    }

    private void SubscribePlayerDeath()
    {
        if (playerCombatResource == null || isPlayerDeathSubscribed) return;

        playerCombatResource.OnDead += HandlePlayerDead;
        isPlayerDeathSubscribed = true;
    }

    private void SubscribePlayerHp()
    {
        if (playerHp == null || isPlayerHpSubscribed) return;

        playerHp.OnHPChanged += HandlePlayerHpChanged;
        isPlayerHpSubscribed = true;
    }

    private void UnsubscribePlayerDeath()
    {
        if (playerCombatResource == null || !isPlayerDeathSubscribed) return;

        playerCombatResource.OnDead -= HandlePlayerDead;
        isPlayerDeathSubscribed = false;
    }

    private void UnsubscribePlayerHp()
    {
        if (playerHp == null || !isPlayerHpSubscribed) return;

        playerHp.OnHPChanged -= HandlePlayerHpChanged;
        isPlayerHpSubscribed = false;
    }

    private void HandlePlayerDead()
    {
        if (playerCombatResource != null)
        {
            ruleDataContainer.currentPlayerHp = playerCombatResource.CurrentHp;
        }

        ResolveStageFailure();
    }

    private void HandlePlayerHpChanged(int nextCurrentHp, int nextMaxHp)
    {
        ruleDataContainer.currentPlayerHp = nextCurrentHp;
        if (nextCurrentHp > 0) return;

        ResolveStageFailure();
    }

    private PlayerCombatResource FindStagePlayerCombatResource()
    {
        PlayerCombatResource[] candidates = FindObjectsByType<PlayerCombatResource>(FindObjectsSortMode.None);
        return FindBestStagePlayer(candidates);
    }

    private PlayerHP FindStagePlayerHp()
    {
        PlayerHP[] candidates = FindObjectsByType<PlayerHP>(FindObjectsSortMode.None);
        return FindBestStagePlayer(candidates);
    }

    private T FindBestStagePlayer<T>(T[] candidates) where T : Component
    {
        if (candidates == null || candidates.Length == 0)
        {
            return null;
        }

        Scene ownerScene = gameObject.scene;

        for (int i = 0; i < candidates.Length; i++)
        {
            T candidate = candidates[i];
            if (!IsStagePlayerCandidate(candidate, ownerScene, true))
            {
                continue;
            }

            return candidate;
        }

        for (int i = 0; i < candidates.Length; i++)
        {
            T candidate = candidates[i];
            if (!IsStagePlayerCandidate(candidate, ownerScene, false))
            {
                continue;
            }

            return candidate;
        }

        return null;
    }

    private static bool IsStagePlayerCandidate(Component candidate, Scene ownerScene, bool requireSameScene)
    {
        if (candidate == null) return false;
        if (!candidate.gameObject.activeInHierarchy) return false;
        if (requireSameScene && candidate.gameObject.scene != ownerScene) return false;
        if (candidate.GetComponentInParent<Canvas>(true) != null) return false;

        PlayerMoveController moveController = candidate.GetComponent<PlayerMoveController>();
        if (moveController == null) moveController = candidate.GetComponentInParent<PlayerMoveController>();
        return moveController != null;
    }
    #endregion

    #region 룰
    public void TimeOver()
    {
        ResolveStageFailure();
    }

    public void HpZero()
    {
        ResolveStageFailure();
    }

    public void WaveClear()
    {
        EnsureInitialized();

        int currentWave = Mathf.Clamp(ruleDataContainer.waveIndex, 1, Mathf.Max(1, ruleDataContainer.waveCount));

        GameFlowManager gameFlow = GameFlowManager.Instance;
        if (gameFlow != null)
        {
            gameFlow.waveIndex = currentWave;
        }

        StageFlowManager stageFlow = StageFlowManager.Instance;
        if (stageFlow != null)
        {
            stageFlow.RecordWaveStart(currentWave);
        }

        Debug.Log($"웨이브 시작 {currentWave}");
        OnWaveClear?.Invoke(currentWave);
    }

    public void RoundClear()
    {
        if (isStageResultResolved) return;

        EnsureInitialized();

        StageManager stageManager = StageManager.Instance;
        if (stageManager == null || stageManager.selectDB == null)
        {
            Debug.LogWarning("RoundClear 호출 시 StageManager.selectDB가 null입니다.");
            return;
        }

        if (stageManager.selectDB.clearResult == ClearResult.None)
        {
            stageManager.selectDB.clearResult = ClearResult.Success;
        }

        if (stageManager.selectDB.clearResult != ClearResult.Success) return;

        isStageResultResolved = true;
        OnRoundClear?.Invoke();

        StageFlowManager stageFlow = StageFlowManager.Instance;
        if (stageFlow == null)
        {
            Debug.LogWarning("RoundClear 호출 시 StageFlowManager가 null입니다.");
            return;
        }

        stageFlow.RoundClear();
        ruleType?.OnExit(ruleDataContainer, this);
    }

    private void ResolveStageFailure()
    {
        if (isStageResultResolved) return;

        EnsureInitialized();

        StageFlowManager stageFlow = StageFlowManager.Instance;
        if (stageFlow == null || stageFlow.waveIndex <= 0)
        {
            return;
        }

        StageManager stageManager = StageManager.Instance;
        if (stageManager == null || stageManager.selectDB == null) return;

        if (stageManager.selectDB.clearResult == ClearResult.None)
        {
            stageManager.selectDB.clearResult = ClearResult.Faile;
        }

        if (stageManager.selectDB.clearResult != ClearResult.Faile) return;

        isStageResultResolved = true;

        if (stageFlow != null)
        {
            stageFlow.MarkStageFailed();
        }

        ruleType?.OnExit(ruleDataContainer, this);
    }
    #endregion

    private void OnDisable()
    {
        UnsubscribePlayerHp();
        UnsubscribePlayerDeath();
    }
}
