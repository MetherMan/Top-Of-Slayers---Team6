using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private EnemyFactory enemyFactory;
    [SerializeField] private StageConfigSO stageSO;

    [Header("설정")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int spawnCount;

    //몬스터 수 관련은 몬스터매니저 같은 곳에서 하는 것이 좋을듯함
    private int monsterCount;

    private int currentRound = 0;

    void Start()
    {
        monsterCount = 0;

        Spawn();
    }

    private void Spawn()
    {
        var roundData = stageSO.roundDatas[currentRound];
        //라운드데이터 리스트만큼 몬스터 생성
        foreach (var monster in roundData.monsterSpawnList)
        {
            enemyFactory.Create(monster, spawnPoint.position, Quaternion.identity);

            monsterCount++;
        }
    }

    private void ReSpawn()
    {
        //한라운드 다 죽이면 라운드 증가, 다시 스폰
        if(monsterCount <= 0)
        {
            currentRound++;
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
