using System;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    public static Action<int> OnEnemyKilled;
    [SerializeField] private int killScore = 100;

    private GameObject enemyPrefab;

    public Transform player;
    public Rigidbody rb;
    public EnemyAnim enemyAnim;

    public EnemyStateMachine enemyStateMachine;
    private PlayerMoveController cachedPlayerMoveController;
    private ChainCombatController cachedChainCombatController;

    [Header("체인 추적 감속")]
    [SerializeField] private bool useChainChaseSlow = true;
    [SerializeField, Min(0f)] private float chainChaseSlowRadius = 7f;
    [SerializeField, Range(0f, 1f)] private float chainChaseSlowMultiplier = 0.1f;

    [Header("체인 반응 연출")]
    [SerializeField] private bool useChainReactionAnimSlow = true;
    [SerializeField, Range(0.1f, 1f)] private float chainReactionAnimSpeed = 0.72f;
    [SerializeField, Min(0f)] private float chainReactionBlendSpeed = 8f;

    [Header("회전")]
    [SerializeField, Min(0f)] private float turnSpeedDegrees = 720f;

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
    public float attackCooldown => enemySO.attackCooldown;
    public float dashTime => enemySO.dashTime;
    public float dashSpeed => enemySO.dashSpeed;
    public float TurnSpeedDegrees => Mathf.Max(0f, turnSpeedDegrees);

    private float chainReactionWeight;
    private bool dashHitConsumed;

    public bool IsDash { get; set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        enemyAnim = GetComponent<EnemyAnim>();
        enemyStateMachine = GetComponent<EnemyStateMachine>();
        ResolvePlayer();

        IdleState = new EnemyIdleState(this, enemyStateMachine);
        FollowState = new EnemyFollow(this, enemyStateMachine);
        AttackState = new EnemyAttack(this, enemyStateMachine);
    }

    private void OnEnable()
    {
        EnsureRuntimeState();
        ResolvePlayer();
        EndDashAttack();
    }

    private void Start()
    {
        enemyStateMachine.ChangeState(IdleState);
    }

    private void Update()
    {
        UpdateChainReactionVisual();
    }

    private void OnDisable()
    {
        EndDashAttack();
        chainReactionWeight = 0f;
        if (enemyAnim != null)
        {
            enemyAnim.SetPlaybackSpeed(1f);
        }
    }

    public void Init(GameObject enemyPrefab)
    {
        this.enemyPrefab = enemyPrefab;
    }

    public Transform ResolvePlayer()
    {
        if (cachedPlayerMoveController == null || !cachedPlayerMoveController.gameObject.activeInHierarchy)
        {
            cachedPlayerMoveController = FindObjectOfType<PlayerMoveController>();
        }

        if (cachedPlayerMoveController != null)
        {
            player = cachedPlayerMoveController.transform;
            return player;
        }

        if (player != null && player.gameObject.activeInHierarchy)
        {
            return player;
        }

        try
        {
            var taggedPlayer = GameObject.FindGameObjectWithTag("Player");
            if (taggedPlayer != null)
            {
                player = taggedPlayer.transform;
            }
        }
        catch (UnityException)
        {
        }

        return player;
    }

    public float GetChaseSpeedMultiplier(Vector3 enemyPosition)
    {
        if (!useChainChaseSlow) return 1f;
        if (!IsChainSlowActive()) return 1f;

        var playerTransform = ResolvePlayer();
        if (playerTransform == null) return 1f;

        if (chainChaseSlowRadius > 0f)
        {
            var playerPosition = playerTransform.position;
            playerPosition.y = enemyPosition.y;
            if ((playerPosition - enemyPosition).sqrMagnitude > chainChaseSlowRadius * chainChaseSlowRadius)
            {
                return 1f;
            }
        }

        return Mathf.Clamp01(chainChaseSlowMultiplier);
    }

    public void Die()
    {
        EndDashAttack();
        OnEnemyKilled?.Invoke(killScore);
        ObjectPoolManager.Instance.ReturnPool(enemyPrefab, gameObject);
    }

    public void BeginDashAttack()
    {
        dashHitConsumed = false;
        IsDash = true;
    }

    public void EndDashAttack()
    {
        dashHitConsumed = false;
        IsDash = false;

        if (rb == null) return;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public static bool TryApplyPlayerDamage(GameObject targetObject, int damage)
    {
        if (targetObject == null) return false;
        if (damage <= 0) return false;

        var chainCombat = targetObject.GetComponent<ChainCombatController>();
        if (chainCombat == null) chainCombat = targetObject.GetComponentInParent<ChainCombatController>();
        if (chainCombat == null) chainCombat = FindObjectOfType<ChainCombatController>();
        if (chainCombat != null && chainCombat.IsSlowActive)
        {
            return false;
        }

        var combatResource = targetObject.GetComponent<PlayerCombatResource>();
        if (combatResource == null) combatResource = targetObject.GetComponentInParent<PlayerCombatResource>();
        if (combatResource != null)
        {
            return combatResource.TakeEnemyHit(damage);
        }

        var playerHP = targetObject.GetComponent<PlayerHP>();
        if (playerHP == null) playerHP = targetObject.GetComponentInParent<PlayerHP>();
        if (playerHP == null) return false;

        playerHP.TakeDamage(damage);
        return true;
    }

    private void EnsureRuntimeState()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        if (rb == null) return;

        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    private bool IsChainSlowActive()
    {
        if (cachedChainCombatController == null || !cachedChainCombatController.gameObject.activeInHierarchy)
        {
            cachedChainCombatController = FindObjectOfType<ChainCombatController>();
        }

        return cachedChainCombatController != null && cachedChainCombatController.IsSlowActive;
    }

    private void UpdateChainReactionVisual()
    {
        if (!useChainReactionAnimSlow) return;
        if (enemyAnim == null) return;

        var targetWeight = ShouldApplyChainReaction() ? 1f : 0f;
        var delta = Time.unscaledDeltaTime > 0f ? Time.unscaledDeltaTime : Time.deltaTime;
        var blendSpeed = Mathf.Max(0f, chainReactionBlendSpeed);
        chainReactionWeight = Mathf.MoveTowards(chainReactionWeight, targetWeight, Mathf.Max(0f, delta) * blendSpeed);

        var slowedSpeed = Mathf.Clamp(chainReactionAnimSpeed, 0.1f, 1f);
        var speed = Mathf.Lerp(1f, slowedSpeed, chainReactionWeight);
        enemyAnim.SetPlaybackSpeed(speed);
    }

    private bool ShouldApplyChainReaction()
    {
        if (!IsChainSlowActive()) return false;

        var playerTransform = ResolvePlayer();
        if (playerTransform == null) return false;
        if (chainChaseSlowRadius <= 0f) return true;

        var playerPosition = playerTransform.position;
        playerPosition.y = transform.position.y;
        var maxDistance = chainChaseSlowRadius * chainChaseSlowRadius;
        return (playerPosition - transform.position).sqrMagnitude <= maxDistance;
    }

    private void OnDrawGizmos()
    {
        if (enemySO == null) return;

        Gizmos.color = Color.red;
        if (enemySO.attackType == AttackType.Melee)
        {
            Gizmos.DrawWireSphere(transform.position, enemySO.attackRange);
        }

        if (enemySO.attackType == AttackType.Corn)
        {
            Gizmos.DrawWireSphere(transform.position, enemySO.attackRange);
            Vector3 leftDir = Quaternion.Euler(0, -enemySO.attackAngle / 2, 0) * transform.forward;
            Vector3 rightDir = Quaternion.Euler(0, enemySO.attackAngle / 2, 0) * transform.forward;
            Gizmos.DrawLine(transform.position, transform.position + leftDir * enemySO.attackRange);
            Gizmos.DrawLine(transform.position, transform.position + rightDir * enemySO.attackRange);
        }
    }

    public EnemyConfigSO GetEnemySO()
    {
        return enemySO;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (enemySO == null) return;
        if (enemySO.attackType != AttackType.Dash) return;
        if (!other.gameObject.CompareTag("Player")) return;
        if (!IsDash) return;
        if (dashHitConsumed) return;

        dashHitConsumed = true;
        TryApplyPlayerDamage(other.gameObject, enemySO.strength);
    }
}
