using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    public Transform player;
    public Rigidbody rb;
    public EnemyAnim enemyAnim;

    public EnemyStateMachine enemyStateMachine;

    public EnemyFollow FollowState { get; private set; }
    public EnemyAttack AttackState { get; private set; }


    [SerializeField] private EnemyConfigSO enemySO;
    public float moveSpeed => enemySO.moveSpeed;
    public float attackRange => enemySO.attackRange;
    public AttackType attackType => enemySO.attackType;
    public GameObject bulletPrefab => enemySO.bulletPrefab;
    public float bulletSpeed => enemySO.bulletSpeed;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody>();
        enemyAnim = GetComponent<EnemyAnim>();
        enemyStateMachine = GetComponent<EnemyStateMachine>();

        FollowState = new EnemyFollow(this, enemyStateMachine);
        AttackState = new EnemyAttack(this, enemyStateMachine);
    }

    private void Start()
    {
        enemyStateMachine.ChangeState(FollowState);
    }
}
