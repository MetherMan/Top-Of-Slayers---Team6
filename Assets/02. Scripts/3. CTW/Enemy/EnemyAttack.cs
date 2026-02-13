using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class EnemyAttack : IEnemyState
{
    private EnemyBase enemy;
    private EnemyStateMachine enemyStateMachine;

    private float attackDuration;
    private float timer;

    private bool isAttackEnded;
    private bool isShoot;   //원거리 공격
    private bool isMeleeAttack;//근거리 공격
    private bool isCornAttack; //부채꼴 공격

    public EnemyAttack(EnemyBase enemy, EnemyStateMachine enemyStateMachine)
    {
        this.enemy = enemy;
        this.enemyStateMachine = enemyStateMachine;
    }

    public void Enter()
    {
        isAttackEnded = false;
        isShoot = false;
        isMeleeAttack = false;
        isCornAttack = false;
        timer = 0f;

        enemy.enemyAnim.EnemyRunning(false);
        enemy.enemyAnim.EnemyAttack(true);
        //재생중인 애니메이션 길이 가져오기
        attackDuration = enemy.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length;
    }

    public void Update()
    {
        Vector3 lookDir = enemy.player.position;
        lookDir.y = enemy.player.position.y;
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

        if(enemy.attackType == AttackType.Melee && !isMeleeAttack)
        {
            //애니메이션길이 끝에 근접공격 판정
            if (timer >= attackDuration * 0.95f)
            {
                MeleeAttack();
                isMeleeAttack = true;
            }
        }

        if (enemy.attackType == AttackType.Corn && !isCornAttack)
        {
            //애니메이션길이 끝에 근접공격 판정
            if (timer >= attackDuration * 0.95f)
            {
                CornAttack();
                isCornAttack = true;
            }
        }

        if(!isAttackEnded)
        {
            timer += Time.deltaTime;
            if(timer >= attackDuration)
            {
                isAttackEnded = true;
            }
            return;
        }

        float distance = Vector3.Distance(enemy.transform.position, enemy.player.position);

        if(distance > enemy.attackRange)
        {
            //멀어지면 따라가고
            enemyStateMachine.ChangeState(enemy.FollowState);
        }
        else
        {
            //사거리 그대로 안이면 계속 공격
            Enter();
        }
    }

    public void Exit()
    {
        enemy.enemyAnim.EnemyAttack(false);
    }

    private void SpawnBullet()
    {
        if(enemy.bulletPrefab == null) return;

        Vector3 spawnPos = enemy.transform.position + Vector3.up * 1.5f + enemy.transform.forward * 0.5f;

        GameObject bulletObj = ObjectPoolManager.Instance.SpawnPool
            (enemy.bulletPrefab, spawnPos, Quaternion.identity);  

        if(bulletObj != null)
        {
            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if(bullet != null)
            {
                Vector3 target = enemy.player.position + Vector3.up * 1.5f;
                Vector3 shootDir = (target - spawnPos).normalized;

                bullet.Init(enemy.bulletPrefab, enemy.bulletSpeed, shootDir, enemy.attackDamage);
            }
        }
    }

    private void MeleeAttack()
    {
        //근접공격 판정
        Collider[] hitColliders = Physics.OverlapSphere(enemy.transform.position, enemy.attackRange);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                PlayerHP playerHP = hitCollider.GetComponent<PlayerHP>();
                if (playerHP != null)
                {
                    playerHP.TakeDamage(enemy.attackDamage);
                }
            }
        }
    }

    private void CornAttack()
    {
        //부채꼴 공격 판정
        Collider[] hitColliders = Physics.OverlapSphere(enemy.transform.position, enemy.attackRange);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                Vector3 dirToPlayer = (hitCollider.transform.position - enemy.transform.position).normalized;
                float angle = Vector3.Angle(enemy.transform.forward, dirToPlayer);
                if (angle <= enemy.attackAngle) //60도 이내면 공격
                {
                    PlayerHP playerHP = hitCollider.GetComponent<PlayerHP>();
                    if (playerHP != null)
                    {
                        playerHP.TakeDamage(enemy.attackDamage);
                    }
                }
            }
        }
    }

    
    //private void OnDrawGizmos()
    //{
    //    //근접공격 범위 시각화
    //    Gizmos.color = Color.red;
    //    if(enemy.attackType == AttackType.Melee)
    //    {
    //        Gizmos.DrawWireSphere(enemy.transform.position, 1.5f);
    //    }
    //
    //    if(enemy.attackType == AttackType.Corn)
    //    {
    //        Gizmos.DrawWireSphere(enemy.transform.position, 3.0f);
    //        //부채꼴 방향선
    //        Vector3 leftDir = Quaternion.Euler(0, -30f, 0) * enemy.transform.forward;
    //        Vector3 rightDir = Quaternion.Euler(0, 30f, 0) * enemy.transform.forward;
    //        Gizmos.DrawLine(enemy.transform.position, enemy.transform.position + leftDir * 3.0f);
    //        Gizmos.DrawLine(enemy.transform.position, enemy.transform.position + rightDir * 3.0f);
    //    }
    //}
}
