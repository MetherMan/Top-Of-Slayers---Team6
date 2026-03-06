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

public enum SpawnPattern
{
    Diagonal,
    Cross,
    Around,
    Up,
    Down,
    Left,
    Right
}

[CreateAssetMenu(fileName = "Stage_", menuName = "Config/Stage")]
public class StageConfigSO : ScriptableObject
{
    [Header("SceneInstance 주소")]
    //public AssetReferenceT<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance> sceneReference;
    public AssetReference sceneReference;

    [Header("스테이지 타입")]
    public StageType stageType;

    [Header("스테이지 룰")]
    public WaveRule stageRule;

    [Header("스테이지 시간")]
    public int stageTime;

    [Header("스테이지 초회차 여부")]
    public bool isCleared = false;

    //데이터 중심 설계 : 각 스테이지 데이터에서 웨이브 값 설정
    //웨이브 : 각 웨이브 별 스폰될 몬스터 수, 타입
    //라운드 : 사용되어질 몬스터 타입, 엘리트, 보스 유무
    //중첩클래스 : https://artiper.tistory.com/125
    [Header("스테이지 웨이브(라운드) 세팅")]
    public List<RoundData> roundDatas = new List<RoundData>();

    [System.Serializable] public class RoundData
    {
        public SpawnPattern spawnPattern;
        public EnemyConfigSO[] monsterSpawnList;
        public EnemyConfigSO elite;
        public EnemyConfigSO boss;
    }

    [Header("클리어 결과")]
    public ClearResult clearResult; //기본 값 none
}
