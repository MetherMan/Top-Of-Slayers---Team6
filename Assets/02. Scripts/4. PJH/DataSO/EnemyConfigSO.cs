using UnityEngine;

public enum MonsterType
{
    Attack,
    Defence,
    Elite,
    Boss
}

public enum Level
{
    Nomal,
    Hard
}

[CreateAssetMenu(fileName = "Enemy_", menuName = "Config/Enemy")]
public class EnemyConfigSO : ScriptableObject
{
    [Header("몬스터 기본 설정")]
    public Level level; //난이도 별 구분 : 20% 스펙 값 증감
    public MonsterType monsterType;
    public GameObject monsterPrefab;

    [Header("몬스터 스펙")]
    public int maxHp;
    public int hp;

    public int strength; //Str 데미지
    public int defence; //df 방어력

    [Header("몬스터 루팅 목록")]
    public int[] NormalDropCoin; //ex. 14~17 coin
    //public itemSO[] NormalDropItemList; ^RandomRange로 루팅 아이탬 확률 조정
    public int[] HardDropCoin;
    //public itemSO[] HardDropItemList;
}