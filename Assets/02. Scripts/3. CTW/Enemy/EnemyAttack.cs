using UnityEngine;

public class EnemyAttack : IEnemyState
{
    private readonly EnemyBase enemy;
    private readonly EnemyStateMachine enemyStateMachine;

    private float attackDuration;
    private float timer;
    private bool isAttackEnded;
    private bool isShoot;
    private bool isStateChaged;
    private float coolTimer;
    private bool isCooldown;
    private bool isDashed;
    private Vector3 dashDir;
    private float dashTimer;
    private Animator cachedAnimator;
    private bool hasAttackTiming;

    public EnemyAttack(EnemyBase enemy, EnemyStateMachine enemyStateMachine)
    {
        this.enemy = enemy;
        this.enemyStateMachine = enemyStateMachine;
    }

    public void Enter()
    {
        isAttackEnded = false;
        isShoot = false;
        timer = 0f;
        coolTimer = 0f;
        isCooldown = false;
        isDashed = false;
        dashTimer = 0f;

        enemy.enemyAnim.EnemyRunning(false);
        enemy.enemyAnim.EnemyAttack(true);
        enemy.EndDashAttack();

        cachedAnimator = enemy.GetComponent<Animator>();
        attackDuration = ResolveAttackDuration();
        hasAttackTiming = attackDuration > 0f;
        isStateChaged = false;
    }

    public void Update()
    {
        var playerTransform = enemy.ResolvePlayer();
        if (playerTransform != null)
        {
            var lookDir = playerTransform.position - enemy.transform.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.0001f)
            {
                var targetRotation = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
                enemy.transform.rotation = Quaternion.RotateTowards(
                enemy.transform.rotation,
                targetRotation,
                enemy.TurnSpeedDegrees * Time.deltaTime);
            }
        }

        if (enemy.attackType == AttackType.Ranged && playerTransform != null && !enemy.HasLineOfSightToPlayer(playerTransform))
        {
            if (!isStateChaged)
            {
                isStateChaged = true;
                enemyStateMachine.ChangeState(enemy.FollowState);
            }
            return;
        }

        if (!hasAttackTiming || !isShoot)
        {
            TryRefreshAttackTiming();
        }

        if (enemy.attackType == AttackType.Dash && isDashed)
        {
            UpdateDashAttack();
        }

        timer += Time.deltaTime;

        if (!isShoot)
        {
            if (enemy.attackType == AttackType.Ranged)
            {
                if (timer >= attackDuration * 0.5f)
                {
                    SpawnBullet();
                    isShoot = true;
                }
            }
            else if (enemy.attackType == AttackType.Melee || enemy.attackType == AttackType.Corn)
            {
                if (timer >= attackDuration * 0.9f)
                {
                    MeleeAttack();
                    isShoot = true;
                }
            }
            else if (enemy.attackType == AttackType.Dash && timer >= attackDuration * 0.5f)
            {
                BeginDashAttack(playerTransform);
                isShoot = true;
            }
        }

        var dashFinished = enemy.attackType != AttackType.Dash || !isDashed;
        if (!isAttackEnded)
        {
            if (timer >= attackDuration && dashFinished)
            {
                isAttackEnded = true;
                isCooldown = true;
                coolTimer = 0f;
            }
            return;
        }

        if (isCooldown)
        {
            coolTimer += Time.deltaTime;

            if (coolTimer < enemy.attackCooldown)
            {
                return;
            }
            isCooldown = false;
        }

        if (playerTransform == null) return;

        float distance = Vector3.Distance(enemy.transform.position, playerTransform.position);

        if (distance > enemy.attackRange && !isStateChaged)
        {
            isStateChaged = true;
            enemyStateMachine.ChangeState(enemy.FollowState);
        }
        else
        {
            Enter();
        }
    }

    public void Exit()
    {
        enemy.enemyAnim.EnemyAttack(false);
        if (isDashed || enemy.IsDash)
        {
            isDashed = false;
            enemy.EndDashAttack();
        }
    }

    private void SpawnBullet()
    {
        var playerTransform = enemy.ResolvePlayer();
        if (playerTransform == null) return;
        if (enemy.bulletPrefab == null) return;

        Vector3 spawnPos = enemy.transform.position + Vector3.up * 1.5f + enemy.transform.forward * 0.5f;

        GameObject bulletObj = ObjectPoolManager.Instance.SpawnPool(
            enemy.bulletPrefab,
            spawnPos,
            Quaternion.identity);

        if (bulletObj == null) return;

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet == null) return;

        Vector3 target = playerTransform.position + Vector3.up * 1.5f;
        Vector3 shootDir = (target - spawnPos).normalized;

        bullet.Init(enemy.bulletPrefab, enemy.bulletSpeed, shootDir, enemy.attackDamage);
    }

    private void MeleeAttack()
    {
        var playerTransform = enemy.ResolvePlayer();
        if (playerTransform == null) return;

        Vector3 enemyPos = enemy.transform.position;
        Vector3 playerPos = playerTransform.position;

        enemyPos.y = 0f;
        playerPos.y = 0f;

        float distance = Vector3.Distance(enemyPos, playerPos);
        if (distance <= enemy.attackRange + 0.2f)
        {
            EnemyBase.TryApplyPlayerDamage(playerTransform.gameObject, enemy.attackDamage);
        }
    }

    private void BeginDashAttack(Transform playerTransform)
    {
        if (playerTransform == null) return;

        var direction = playerTransform.position - enemy.transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0f)
        {
            direction = enemy.transform.forward;
        }

        if (direction.sqrMagnitude <= 0f) return;

        dashDir = direction.normalized;
        dashTimer = 0f;
        isDashed = true;
        enemy.BeginDashAttack();
    }

    private void UpdateDashAttack()
    {
        if (!isDashed) return;
        dashTimer += Time.deltaTime;

        if (dashTimer <= enemy.dashTime)
        {
            var currentPosition = enemy.rb != null ? enemy.rb.position : enemy.transform.position;
            var dashSpeedMultiplier = enemy.GetChaseSpeedMultiplier(currentPosition);
            var finalDashSpeed = enemy.dashSpeed * dashSpeedMultiplier;

            if (enemy.rb != null)
            {
                enemy.rb.velocity = dashDir * finalDashSpeed;
            }
            else
            {
                enemy.transform.position += dashDir * finalDashSpeed * Time.deltaTime;
            }
            return;
        }

        isDashed = false;
        enemy.EndDashAttack();
    }

    public void FixedUpdate()
    {
    }

    private void TryRefreshAttackTiming()
    {
        attackDuration = ResolveAttackDuration();
        hasAttackTiming = attackDuration > 0f;
    }

    private float ResolveAttackDuration()
    {
        if (cachedAnimator == null)
        {
            return 0.5f;
        }

        var clipLength = GetClipLength(cachedAnimator.GetNextAnimatorClipInfo(0));
        if (clipLength <= 0f)
        {
            clipLength = GetClipLength(cachedAnimator.GetCurrentAnimatorClipInfo(0));
        }

        if (clipLength <= 0f)
        {
            var stateInfo = cachedAnimator.IsInTransition(0)
                ? cachedAnimator.GetNextAnimatorStateInfo(0)
                : cachedAnimator.GetCurrentAnimatorStateInfo(0);
            clipLength = stateInfo.length;
        }

        return Mathf.Max(0.1f, clipLength);
    }

    private static float GetClipLength(AnimatorClipInfo[] clipInfos)
    {
        if (clipInfos == null || clipInfos.Length == 0) return 0f;

        for (int i = 0; i < clipInfos.Length; i++)
        {
            if (clipInfos[i].clip != null)
            {
                return clipInfos[i].clip.length;
            }
        }

        return 0f;
    }
}
