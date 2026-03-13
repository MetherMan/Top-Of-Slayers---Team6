using System;
using UnityEngine;

/*
    스테이지 씬에 생성
    웨이브 시작, 종료 조건 변수 값을 관리
    DontDestroyOnLoad : false

    *변수 데이터 -> WaveDirectorySystem.cs 보내고 -> GameFlowManager에서 결과에 따라 메서드 실행
    *상태전환 <- GameStateMachine(상태머신) <- GameFlowManager 메서드 실행
*/

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

    //(우영)
    //스테이지 클리어 여부 확인
    public static Action<bool> OnStageClear;
    #endregion

    protected override void Awake()
    {
        base.Awake();

        if (StageManager.Instance.selectDB != null)
        {
            remainingTime = StageManager.Instance.selectDB.stageTime;
            waveLength = StageManager.Instance.selectDB.roundDatas != null
                ? StageManager.Instance.selectDB.roundDatas.Count
                : 0;
        }
    }

    void Update()
    {
        //플레이 시간
        if (StageManager.Instance.selectDB != null)
        {
            timer += Time.unscaledDeltaTime;

            if (timer >= 1f)
            {
                timer -= 1f;

                if (playTime < StageManager.Instance.selectDB.stageTime)
                {
                    playTime++;
                }

                if (remainingTime > 0)
                {
                    remainingTime--;
                }
            }
        }
    }

    #region method
    public void MonsterCleared(int monsterIndex)
    {
        
    }

    public void WaveClear()
    {
        //상태전환 : UI 등등
    }

    public void RoundClear()
    {
        //우영
        //스테이지 클리어 보상 : false 미클리어 :: true 클리어
        bool check = StageManager.Instance.selectDB.isCleared;

        if (!check)
        {
            //(첫클리어)
            OnStageClear?.Invoke(check);
        }
        else
        {
            //(반복)
            OnStageClear?.Invoke(check);
        }

        GameFlowManager.Instance.RoundClear();

        //상태전환 : 씬 이동
    }
    #endregion
}
