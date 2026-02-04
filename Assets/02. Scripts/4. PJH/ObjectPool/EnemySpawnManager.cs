using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private EnemyFactory enemyFactory;

    [Header("설정")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int spawnCount;

    //몬스터 수 관련은 몬스터매니저 같은 곳에서 하는 것이 좋을듯함
    private int monsterCount;

    void Start()
    {
        monsterCount = 0;

        Spawn();
    }

    private void Spawn()
    {
        //스폰카운트만큼 생성하고 카운트 증가
        for (int i = 0; i < spawnCount; i++)
        {
            GameObject monster = enemyFactory.Create(spawnPoint.position, Quaternion.identity);

            monsterCount++;
        }
    }

    private void ReSpawn()
    {
        //한라운드 다 죽이면 다시 스폰
        if(monsterCount <= 0)
        {
            Spawn();
        }
    }

    //몬스터 스크립트에서 죽었을 때 호출
    public void MonsterDead()
    {
        monsterCount--;
        ReSpawn();
        //풀반환은 몬스터 스크립트에서 처리?
    }
}
