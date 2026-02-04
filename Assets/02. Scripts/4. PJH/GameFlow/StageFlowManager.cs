using UnityEngine;

/*
    스테이지 씬에 생성
    웨이브 시작, 종료 조건 변수 값을 관리
    DontDestroyOnLoad : false

    *변수 데이터 -> WaveDirectorySystem.cs 보내고 -> GameFlowManager에서 결과에 따라 메서드 실행
    *상태전환 <- GameStateMachine(상태머신)
*/

public class StageFlowManager : Singleton<StageFlowManager>
{
    #region field
    //플레이 시간
    public int remainingTime;
    public int playTime;

    //웨이브
    public int loundLength;
    public int monsterCount;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        //선택된 스테이지의 StageConfigSO에서 stageTime 변수 값을 가져와서 할당
        remainingTime = StageManager.Instance.selectDB.stageTime;

        //웨이브
        loundLength = StageManager.Instance.stageDB.roundDatas.Count;
    }

    void Update()
    {
        //플레이 시간
        if (remainingTime == 0) remainingTime = StageManager.Instance.selectDB.stageTime;
        playTime += (int)Time.deltaTime;
    }

    #region method

    #endregion
}