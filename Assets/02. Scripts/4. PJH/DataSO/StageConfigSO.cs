using UnityEngine;

public enum StageType
{
    Normal,
    Hard,
    MeddleBossNm,
    MeddleBossHd,
    BossNm,
    BossHd
}

public enum ClearResult
{
    None,
    OneStar,
    TwoStars,
    ThreeStars,
    Falie
}

[CreateAssetMenu(fileName = "Stage_", menuName = "Config/Stage")]
public class StageConfigSO : ScriptableObject
{
    /*
        튜토리얼 맵 01 ~ 0?
        1번 스테이지 노말맵 10
        1번 스테이지 하드맵 11
        10번 스테이지 노말맵 100
        10번 스테이지 하드맵 101
    */ //넘버링 설정 값
    [Header("스테이지 넘버링")]
    public int stageNum;

    [Header("스테이지 타입")]
    public StageType stageType;

    [Header("스테이지 몬스터 세팅")]
    public GameObject[] monsterSpawnList;
    public GameObject meddleBoss;
    public GameObject boss;

    [Header("클리어 결과")]
    public ClearResult clearResult; //기본 값 none

    /*
        clearResult 값 기준으로
        OneStar = ClearReward[0]
        TwoStars = ClearReward[0], ClearReward[1]
        ThreeStars = ClearReward[0], ClearReward[1], ClearReward[2]
    */
    [Header("스테이지 클리어 보상")]
    //public itemDataSO[] FirstClearReward;
    public int[] FirstClearCoin;

    [Header("스테이지 반복 클리어 보상")]
    //public itemDataSO[] ClearReward;
    public int[] ClearCoin;
}