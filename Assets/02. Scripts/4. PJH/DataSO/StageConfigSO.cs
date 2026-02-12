using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
    [Header("SceneInstance 주소")]
    //public AssetReferenceT<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance> sceneReference;
    public AssetReference sceneReference;
    /*
        1번 스테이지 노말맵 10
        1번 스테이지 하드맵 11
        10번 스테이지 노말 보스맵 103
        10번 스테이지 하드 보스맵 104
        첼린지 스테이지 05
    */ //넘버링 설정 값

    [Header("스테이지 넘버링")]
    public int stageKey;

    [Header("스테이지 타입")]
    public StageType stageType;

    [Header("스테이지 룰")]
    public WaveRule stageRule;

    [Header("스테이지 시간")]
    public int stageTime;

    //데이터 중심 설계 : 각 스테이지 데이터에서 웨이브 값 설정
    //웨이브 : 각 웨이브 별 스폰될 몬스터 수, 타입
    //라운드 : 사용되어질 몬스터 타입, 엘리트, 보스 유무
    //중첩클래스 : https://artiper.tistory.com/125
    [Header("스테이지 웨이브(라운드) 세팅")]
    public List<RoundData> roundDatas = new List<RoundData>();

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