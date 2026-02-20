using System;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class EnemyBase : MonoBehaviour
{
    public static Action<int> OnEnemyKilled;
    [SerializeField] private int killScore = 100;

    private GameObject enemyPrefab;

    public Transform player;
    public Rigidbody rb;
    public EnemyAnim enemyAnim;

    public EnemyStateMachine enemyStateMachine;

    public EnemyIdleState IdleState { get; private set; }
    public EnemyFollow FollowState { get; private set; }
    public EnemyAttack AttackState { get; private set; }


    [SerializeField] private EnemyConfigSO enemySO;
    public float moveSpeed => enemySO.moveSpeed;
    public float attackRange => enemySO.attackRange;
    public AttackType attackType => enemySO.attackType;
    public GameObject bulletPrefab => enemySO.bulletPrefab;
    public float bulletSpeed => enemySO.bulletSpeed;
    public int attackDamage => enemySO.strength;  
    public float attackAngle => enemySO.attackAngle;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody>();
        enemyAnim = GetComponent<EnemyAnim>();
        enemyStateMachine = GetComponent<EnemyStateMachine>();

        IdleState = new EnemyIdleState(this, enemyStateMachine);
        FollowState = new EnemyFollow(this, enemyStateMachine);
        AttackState = new EnemyAttack(this, enemyStateMachine);
    }

    private void Start()
    {
        enemyStateMachine.ChangeState(IdleState);
    }

    public void Init(GameObject enemyPrefab)
    {
        this.enemyPrefab = enemyPrefab;
    }

    public void Die()
    {
        OnEnemyKilled?.Invoke(killScore);
        ObjectPoolManager.Instance.ReturnPool(enemyPrefab, gameObject);
    }

    private void OnDrawGizmos()
    {
        //근접공격 범위 시각화
        Gizmos.color = Color.red;
        if (enemySO.attackType == AttackType.Melee)
        {
            Gizmos.DrawWireSphere(transform.position, enemySO.attackRange);
        }

        if (enemySO.attackType == AttackType.Corn)
        {
            Gizmos.DrawWireSphere(transform.position, enemySO.attackRange);
            //부채꼴 방향선
            Vector3 leftDir = Quaternion.Euler(0, -enemySO.attackAngle / 2, 0) * transform.forward;
            Vector3 rightDir = Quaternion.Euler(0, enemySO.attackAngle / 2, 0) * transform.forward;
            Gizmos.DrawLine(transform.position, transform.position + leftDir * enemySO.attackRange);
            Gizmos.DrawLine(transform.position, transform.position + rightDir * enemySO.attackRange);
        }
    }
}
