using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private EnemyFactory enemyFactory;
    [SerializeField] private StageConfigSO stageSO;

    [Header("설정")]
    [SerializeField] private Transform player;
    [SerializeField] private float spawnDistance = 5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float spawnInterval = 0.1f;

    //몬스터 수 관련은 몬스터매니저 같은 곳에서 하는 것이 좋을듯함
    private int monsterCount;

    private int currentRound = 0;

    //스폰 방향
    private Vector3[] spawnDirections =
    {
        Vector3.forward, Vector3.back, Vector3.left, Vector3.right, //상하좌우
        Vector3.forward + Vector3.left, Vector3.forward + Vector3.right, //좌상, 우상
        Vector3.back + Vector3.left, Vector3.back + Vector3.right //좌하, 우하
    };

    private void Awake()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    IEnumerator Start()
    {
        monsterCount = 0;

        yield return new WaitForSeconds(1f);

        WaveStart();
    }

    private void WaveStart()
    {
        if (currentRound >= stageSO.roundDatas.Count) return;
        StartCoroutine(SpawnCoroutine());
    }
    
    private IEnumerator SpawnCoroutine()
    {
        //StageFlowManager.Instance.waveIndex = currentRound + 1;

        var roundData = stageSO.roundDatas[currentRound]; //현재라운드 SO
        int dirCount = spawnDirections.Length; //방향갯수
        int dirIndex = 0; //방향 인덱스

        //라운드데이터 리스트만큼 몬스터 생성
        foreach (var monster in roundData.monsterSpawnList)
        {
            Vector3 dir = spawnDirections[dirIndex];
            Vector3 spawnPos = player.position + dir * spawnDistance;
            Vector3 finalSpawnPos = OnGround(spawnPos); //땅이 굴곡져도 땅 위에 있게하기
            enemyFactory.Create(monster, finalSpawnPos, Quaternion.identity);

            monsterCount++;
            dirIndex++; //방향 인덱스 추가로 다음 스폰 방향 가져오기

            //만약 8마리 이상일 때 처음부터 다시
            if(dirIndex >= dirCount)
            {
                dirIndex = 0;
            }
            yield return new WaitForSeconds(spawnInterval); //스폰 간격
        }
    }

    private Vector3 OnGround(Vector3 groundPos)
    {
        Vector3 ray = groundPos + Vector3.up * 5f; //5f 위에서 레이쏘기

        //레이를 아래로 쏴서 맞은 벡터값 리턴하기
        if(Physics.Raycast(ray, Vector3.down, out RaycastHit hit, 7f, groundLayer))
        {
            return hit.point;
        }
        return groundPos;
    }

    private void ReSpawn()
    {
        //한라운드 다 죽이면 라운드 증가, 다시 스폰
        if(monsterCount <= 0)
        {
            currentRound++;
            WaveStart();
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
