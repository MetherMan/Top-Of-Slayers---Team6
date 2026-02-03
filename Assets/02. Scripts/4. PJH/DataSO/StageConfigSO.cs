using System.Collections.Generic;
using UnityEngine;

public enum StageType
{
    Normal,
    Hard,
    BossNm,
    BossHd,
    Chellenge
}

public enum ClearResult
{
    None,
    Success,
    Faile
}

[CreateAssetMenu(fileName = "Stage_", menuName = "Config/Stage")]
public class StageConfigSO : ScriptableObject
{
    /*
        1번 스테이지 노말맵 10
        1번 스테이지 하드맵 11
        10번 스테이지 노말 보스맵 103
        10번 스테이지 하드 보스맵 104
    */ //넘버링 설정 값

    [Header("스테이지 넘버링")]
    public int stageNum;

    [Header("스테이지 타입")]
    public StageType stageType;

    [Header("스테이지 시간")]
    public int stageTime;

    //데이터 중심 설계 : 각 스테이지 데이터에서 라운드 값 설정
    //보스 스테이지 : 배열의 끝에 다다를 경우 다시 0으로 초기화해서 반복한다
    [Header("스테이지 웨이브(라운드) 세팅")]
    public List<RoundData> loundDatas = new List<RoundData>();

    [System.Serializable] public class RoundData
    {
        public EnemyConfigSO[] monsterSpawnList;
        public EnemyConfigSO elite;
        public EnemyConfigSO boss;
    }

    [Header("클리어 결과")]
    public ClearResult clearResult; //기본 값 none

    [Header("스테이지 클리어 보상")]
    //public itemDataSO[] FirstClearReward;
    public int[] FirstClearCoin;

    [Header("스테이지 반복 클리어 보상")]
    //public itemDataSO[] ClearReward;
    public int[] ClearCoin;
}