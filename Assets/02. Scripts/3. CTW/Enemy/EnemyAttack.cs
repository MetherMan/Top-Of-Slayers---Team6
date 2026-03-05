using UnityEngine;

public class EnemyAttack : IEnemyState
{
    private EnemyBase enemy;
    private EnemyStateMachine enemyStateMachine;

    private float attackDuration;
    private float timer;
    private bool isAttackEnded;
    private bool isShoot;

    private bool isStateChaged = false;

    private float coolTimer;
    private bool isCooldown;

    private bool isDashed;
    private Vector3 dashDir;
    private float dashTimer;

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

        //재생중인 애니메이션 길이 가져오기
        attackDuration = enemy.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length;

        isStateChaged = false;
    }

    public void Update()
    {
        var playerTransform = enemy.ResolvePlayer();
        if (playerTransform == null) return;

        var lookDir = playerTransform.position;
        lookDir.y = enemy.transform.position.y;
        enemy.transform.LookAt(lookDir);

        if (enemy.attackType == AttackType.Ranged && !isShoot)
        {
            //애니메이션길이 절반에 총알 발사
            if (timer >= attackDuration * 0.5f)
            {
                SpawnBullet();
                isShoot = true;
            }
        }
        else if(enemy.attackType == AttackType.Melee || enemy.attackType == AttackType.Corn)
        {
            //애니메이션 끝에 공격
            if(timer >= attackDuration * 0.9f && !isShoot)
            {
                MeleeAttack();
                isShoot = true;
            }
        }

        else if(enemy.attackType == AttackType.Dash && !isShoot)
        {
            if (timer >= attackDuration * 0.5f)
            {
                DashAttack();
                isShoot = true;
            }
        }

        if (!isAttackEnded)
        {
            timer += Time.deltaTime;
            if (timer >= attackDuration)
            {
                isAttackEnded = true;
                isCooldown = true;
            }
            return;
        }

        if (isCooldown)
        {
            coolTimer += Time.deltaTime;

            //어택쿨타임 동안 가만히 있기
            if(coolTimer < enemy.attackCooldown)
            {
                return;
            }
            isCooldown = false;
        }

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
    }

    private void SpawnBullet()
    {
        var playerTransform = enemy.ResolvePlayer();
        if (playerTransform == null) return;
        if(enemy.bulletPrefab == null) return;

        Vector3 spawnPos = enemy.transform.position + Vector3.up * 1.5f + enemy.transform.forward * 0.5f;

        GameObject bulletObj = ObjectPoolManager.Instance.SpawnPool
            (enemy.bulletPrefab, spawnPos, Quaternion.identity);  

        if(bulletObj != null)
        {
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if(bullet != null)
            {
                Vector3 target = playerTransform.position + Vector3.up * 1.5f;
                Vector3 shootDir = (target - spawnPos).normalized;

                bullet.Init(enemy.bulletPrefab, enemy.bulletSpeed, shootDir, enemy.attackDamage);
            }
        }
    }

    private void MeleeAttack()
    {
        var playerTransform = enemy.ResolvePlayer();
        if(playerTransform == null) return;

        Vector3 enemyPos = enemy.transform.position;
        Vector3 playerPos = playerTransform.position;

        enemyPos.y = 0f;
        playerPos.y = 0f;

        float distance = Vector3.Distance(enemyPos, playerPos);

        if(distance <= enemy.attackRange + 0.2f)
        {
            PlayerHP playerHP = playerTransform.GetComponent<PlayerHP>();
            if(playerHP != null)
            {
                playerHP.TakeDamage(enemy.attackDamage);
                Debug.Log($"{enemy.name}이 때림({enemy.attackDamage})");
            }
        }
        else
        {
             Debug.Log($"{enemy.name}의 공격이 빗나감");
        }
    }

    private void DashAttack()
    {
        var playerTransform = enemy.ResolvePlayer();
        if (playerTransform == null) return;

        if (!isDashed)
        {
            dashDir = (playerTransform.position - enemy.transform.position).normalized;
            isDashed = true;
            enemy.IsDash = true;
        }

        dashTimer += Time.deltaTime;

        if (dashTimer <= enemy.dashTime)
        {
            enemy.rb.velocity = dashDir * enemy.dashSpeed;
        }
        else
        {
            enemy.rb.velocity = Vector3.zero;
            isDashed = false;
            enemy.IsDash = false;
        }
    }

    public void FixedUpdate()
    {

    }
}
