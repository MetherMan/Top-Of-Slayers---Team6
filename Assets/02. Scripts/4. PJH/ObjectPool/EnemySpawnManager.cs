using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemySpawnManager : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private EnemyFactory enemyFactory;
    [SerializeField] private StageConfigSO stageSO;
    [SerializeField] private DamageSystem damageSystem;

    [Header("설정")]
    [FormerlySerializedAs("player")]
    [SerializeField] private Transform mapCenter;
    [SerializeField] private float spawnDistance = 5f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float spawnInterval = 0.1f;
    [SerializeField] private float waveDelay = 1f;
    [SerializeField] private GameObject VFXPrefab;
    [SerializeField] private float VFXDelay = 0.5f;

    // 몬스터 수 관련은 몬스터매니저 같은 곳에서 하는 것이 좋을듯함
    private int monsterCount;
    private int currentRound = 0;
    private readonly HashSet<int> aliveEnemyIds = new HashSet<int>();

    // 스폰 방향
    private Vector3[] SpawnDirections(SpawnPattern spawnPattern)
    {
        switch (spawnPattern)
        {
            case SpawnPattern.Diagonal:
                return new Vector3[]
                {
                    new Vector3(2, 0, 1),
                    new Vector3(-2, 0, 1),
                    new Vector3(2, 0, -1),
                    new Vector3(-2, 0, -1),
                    new Vector3(4, 0, 2),
                    new Vector3(-4, 0, 2),
                    new Vector3(4, 0, -2),
                    new Vector3(-4, 0, -2)
                };

            case SpawnPattern.Cross:
                return new Vector3[]
                {
                    Vector3.forward,
                    Vector3.back,
                    Vector3.left * 2,
                    Vector3.right * 2,
                    Vector3.forward*2,
                    Vector3.back*2,
                    Vector3.left*4,
                    Vector3.right*4
                };

            case SpawnPattern.Around:
                return new Vector3[]
                {
                    Vector3.forward,
                    Vector3.back,
                    Vector3.left * 2,
                    Vector3.right * 2,
                    new Vector3(2, 0, 1),
                    new Vector3(-2, 0, 1),
                    new Vector3(2, 0, -1),
                    new Vector3(-2, 0, -1)
                };

            case SpawnPattern.Up:
                return new Vector3[]
                {
                    Vector3.forward,
                    new Vector3(2, 0, 1),
                    new Vector3(-2, 0, 1),
                    Vector3.forward*2,
                    new Vector3(2, 0, 2),
                    new Vector3(-2, 0, 2)
                };

            case SpawnPattern.Down:
                return new Vector3[]
                {
                    Vector3.back,
                    new Vector3(2, 0, -1),
                    new Vector3(-2, 0, -1),
                    Vector3.back*2,
                    new Vector3(2, 0, -2),
                    new Vector3(-2, 0, -2)
                };

            case SpawnPattern.Left:
                return new Vector3[]
                {
                    Vector3.left * 2,
                    new Vector3(-2, 0, 1),
                    new Vector3(-2, 0, -1),
                    Vector3.left*4,
                    new Vector3(-4, 0, 1),
                    new Vector3(-4, 0, -1)
                };

            case SpawnPattern.Right:
                return new Vector3[]
                {
                    Vector3.right *2,
                    new Vector3(2, 0, 1),
                    new Vector3(2, 0, -1),
                    Vector3.right*4,
                    new Vector3(4, 0, 1),
                    new Vector3(4, 0, -1)
                };
        }

        return null;
    }

    private void Awake()
    {
        if (mapCenter == null)
        {
            mapCenter = ResolveSpawnCenter();
        }

        if (damageSystem == null)
        {
            damageSystem = FindObjectOfType<DamageSystem>();
        }
    }

    private void OnEnable()
    {
        if (damageSystem != null)
        {
            damageSystem.OnDamageApplied += HandleDamageApplied;
        }
    }

    private void OnDisable()
    {
        if (damageSystem != null)
        {
            damageSystem.OnDamageApplied -= HandleDamageApplied;
        }

        aliveEnemyIds.Clear();
    }

    private void Start()
    {
        monsterCount = 0;
        StartCoroutine(WaveDelay());
    }

    private void WaveStart()
    {
        if (stageSO == null || stageSO.roundDatas == null)
        {
            return;
        }

        if (currentRound >= stageSO.roundDatas.Count) return;
        StartCoroutine(SpawnCoroutine());
    }

    private IEnumerator SpawnCoroutine()
    {
        if (enemyFactory == null || stageSO == null || stageSO.roundDatas == null)
        {
            yield break;
        }

        if (mapCenter == null)
        {
            mapCenter = ResolveSpawnCenter();
            if (mapCenter == null)
            {
                yield break;
            }
        }

        StageFlowManager.Instance.waveIndex = currentRound + 1;
        WaveDirectorSystem.Instance.WaveClear();

        var roundData = stageSO.roundDatas[currentRound]; // 현재라운드 SO
        Vector3[] spawnDirections = SpawnDirections(roundData.spawnPattern);
        if (spawnDirections == null || spawnDirections.Length == 0)
        {
            yield break;
        }

        int dirCount = spawnDirections.Length;
        int dirIndex = 0; // 방향 인덱스

        // 라운드데이터 리스트만큼 몬스터 생성
        foreach (var monster in roundData.monsterSpawnList)
        {
            if (monster == null) continue;

            Vector3 dir = spawnDirections[dirIndex];
            Vector3 spawnPos = mapCenter.position + dir * spawnDistance;
            Vector3 finalSpawnPos = OnGround(spawnPos); // 최종 스폰위치는 땅에

            GameObject vfx = ObjectPoolManager.Instance.SpawnPool(VFXPrefab, finalSpawnPos, Quaternion.Euler(90f, 0, 0));
            yield return new WaitForSecondsRealtime(VFXDelay);

            var enemy = enemyFactory.Create(monster, finalSpawnPos, Quaternion.identity);
            if (enemy != null)
            {
                aliveEnemyIds.Add(enemy.GetInstanceID());

                Collider collider = enemy.GetComponent<Collider>();
                if(collider != null) collider.enabled = false;

                Vector3 startPos = finalSpawnPos - Vector3.up * 1.5f;
                enemy.transform.position = startPos;

                enemy.transform.DOMoveY(finalSpawnPos.y, 0.4f).SetEase(Ease.OutBack).OnComplete(()=>
                {
                    if (collider != null) collider.enabled = true;
                }) ;
            }

            monsterCount++;
            StageFlowManager.Instance.monsterCount = monsterCount;
            dirIndex++; // 방향 인덱스 추가로 다음 스폰 방향 가져오기

            // 만약 8마리 이상일 때 처음부터 다시
            if (dirIndex >= dirCount)
            {
                dirIndex = 0;
            }

            yield return new WaitForSecondsRealtime(spawnInterval); // 스폰 간격
        }
    }

    private Transform ResolveSpawnCenter()
    {
        try
        {
            GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
            if (taggedPlayer != null)
            {
                return taggedPlayer.transform;
            }
        }
        catch (UnityException)
        {
        }

        GameObject mainChar = GameObject.Find("1.Main Char");
        if (mainChar != null)
        {
            return mainChar.transform;
        }

        return transform;
    }

    // 땅 위에 스폰 위치 반환
    private Vector3 OnGround(Vector3 groundPos)
    {
        Vector3 ray = groundPos + Vector3.up * 10f; // 10f 위에서 레이쏘기

        // 레이를 아래로 쏴서 맞은 그라운드 레이어 벡터값 리턴하기
        if (Physics.Raycast(ray, Vector3.down, out RaycastHit hit, 15f, groundLayer))
        {
            return hit.point;
        }

        return groundPos;
    }

    private void ReSpawn()
    {
        // 한라운드 다 죽이면 라운드 증가, 다시 스폰
        if (monsterCount > 0) return;

        currentRound++;
        StageFlowManager.Instance.waveIndex = currentRound;

        if (currentRound >= stageSO.roundDatas.Count)
        {
            WaveDirectorSystem.Instance.RoundClear();
            return;
        }

        StartCoroutine(WaveDelay());
    }

    private IEnumerator WaveDelay()
    {
        yield return new WaitForSeconds(waveDelay);
        WaveStart();
    }

    // 몬스터 스크립트에서 죽었을 때 호출
    public void MonsterDead()
    {
        if (monsterCount <= 0) return;

        monsterCount--;
        StageFlowManager.Instance.monsterCount = monsterCount;
        ReSpawn();
        // 풀반환은 몬스터 스크립트에서 처리?
    }

    private void HandleDamageApplied(DamageSystem.DamageResult result)
    {
        if (!result.IsDead) return;
        if (result.Target == null) return;

        int id = result.Target.gameObject.GetInstanceID();
        if (!aliveEnemyIds.Remove(id)) return;

        MonsterDead();
    }
}

