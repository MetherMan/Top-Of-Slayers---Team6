using System;
using UnityEngine;

/*
    스테이지 씬에 생성
    웨이브 시작, 종료 조건 변수 값을 관리
    DontDestroyOnLoad : false

    *변수 데이터 -> WaveDirectorySystem.cs 보내고 -> GameFlowManager에서 결과에 따라 메서드 실행
    *상태전환 <- GameStateMachine(상태머신) <- GameFlowManager 메서드 실행
*/

public struct StageResultData
{
    public StageResultData(bool isClear, bool isFirstClear, int lastClearedWave, int totalWaveCount)
    {
        IsClear = isClear;
        IsFirstClear = isFirstClear;
        LastClearedWave = lastClearedWave;
        TotalWaveCount = totalWaveCount;
    }

    public bool IsClear { get; }
    public bool IsFirstClear { get; }
    public int LastClearedWave { get; }
    public int TotalWaveCount { get; }
}

public class StageFlowManager : Singleton<StageFlowManager>
{
    #region field
    //플레이 시간
    public int remainingTime;
    public int playTime;

    private float timer;

    //웨이브
    public int waveLength;
    public int monsterCount;
    public int waveIndex; //EnemySpawnManager에서 값 변경
    public int LastClearedWave { get; private set; }

    //(우영)
    //스테이지 클리어 여부 확인
    public static Action<bool> OnStageClear;
    public static Action<StageResultData> OnStageFinished;

    private bool hasStageResult;
    private bool isStageStateInitialized;

    public bool stageIn = false;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        ResetStageState();
    }

    private void Start()
    {
        ResetStageState();
    }

    private void ResetStageState()
    {
        StageConfigSO stageConfig = StageManager.Instance != null ? StageManager.Instance.selectDB : null;
        if (stageConfig == null)
        {
            isStageStateInitialized = false;
            return;
        }

        stageConfig.clearResult = ClearResult.None;
        remainingTime = stageConfig.stageTime;
        playTime = 0;
        timer = 0f;
        waveLength = stageConfig.roundDatas != null ? stageConfig.roundDatas.Count : 0;
        monsterCount = 0;
        waveIndex = 0;
        LastClearedWave = 0;
        hasStageResult = false;
        isStageStateInitialized = true;
    }

    private void Update()
    {
        if (Time.timeScale <= 0f) return;

        StageConfigSO stageConfig = StageManager.Instance != null ? StageManager.Instance.selectDB : null;
        if (stageConfig == null) return;

        if (!isStageStateInitialized)
        {
            ResetStageState();
            if (!isStageStateInitialized) return;
        }

        timer += Time.unscaledDeltaTime;
        if (timer < 1f) return;

        timer -= 1f;

        if (playTime < stageConfig.stageTime)
        {
            playTime++;
        }

        if (remainingTime > 0)
        {
            remainingTime--;
        }
    }

    #region method
    public void MonsterCleared(int monsterIndex)
    {
    }

    public void WaveClear()
    {
        RecordWaveStart(waveIndex);
    }

    public void RecordWaveStart(int currentWave)
    {
        if (hasStageResult) return;

        waveIndex = Mathf.Clamp(currentWave, 0, waveLength);
        LastClearedWave = Mathf.Clamp(waveIndex - 1, 0, waveLength);
    }

    public void MarkStageFailed()
    {
        CompleteStage(false);
    }

    public void RoundClear()
    {
        CompleteStage(true);
        GameFlowManager.Instance?.RoundClear();
    }

    private void CompleteStage(bool isClear)
    {
        if (hasStageResult) return;

        if (!isStageStateInitialized)
        {
            ResetStageState();
        }

        StageConfigSO stageConfig = StageManager.Instance != null ? StageManager.Instance.selectDB : null;
        if (stageConfig == null) return;

        bool wasAlreadyCleared = stageConfig.isCleared;
        if (isClear)
        {
            LastClearedWave = waveLength;
            OnStageClear?.Invoke(wasAlreadyCleared);
            stageConfig.isCleared = true;
        }

        hasStageResult = true;
        OnStageFinished?.Invoke(new StageResultData(
            isClear,
            !wasAlreadyCleared,
            LastClearedWave,
            waveLength));
    }
    #endregion
}
