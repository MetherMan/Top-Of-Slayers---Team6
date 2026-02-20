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

public enum AttackType
{
    Melee,  //근거리
    Ranged, //원거리
    Corn    //부채꼴
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

    public float moveSpeed;
    public float attackRange;

    [Header("몬스터 루팅 목록")]
    public int[] NormalDropCoin; //ex. 14~17 coin
    //public itemSO[] NormalDropItemList; ^RandomRange로 루팅 아이탬 확률 조정
    public int[] HardDropCoin;
    //public itemSO[] HardDropItemList;

    [Header("공격 타입")]
    public AttackType attackType;

    [Header("원거리 전용")]
    public GameObject bulletPrefab;
    public float bulletSpeed;

    [Header("부채꼴 전용")]
    public float attackAngle;
}