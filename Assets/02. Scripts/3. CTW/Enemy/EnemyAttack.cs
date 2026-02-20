using UnityEditor;
using UnityEngine;

public class EnemyAttack : IEnemyState
{
    private EnemyBase enemy;
    private EnemyStateMachine enemyStateMachine;

    private float attackDuration;
    private float timer;
    private bool isAttackEnded;
    private bool isShoot;

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

                bullet.Init(enemy.bulletPrefab, enemy.bulletSpeed, shootDir);
            }
        }
    }
}
