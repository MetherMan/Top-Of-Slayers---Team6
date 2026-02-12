using UnityEngine;

public class EnemyFollow : IEnemyState
{
    private EnemyBase enemy;
    private EnemyStateMachine enemyStateMachine;

    public EnemyFollow(EnemyBase enemy, EnemyStateMachine enemyStateMachine)
    {
        this.enemy = enemy;
        this.enemyStateMachine = enemyStateMachine;
    }

    public void Enter()
    {
        enemy.enemyAnim.EnemyRunning(true);
    }

    public void Update()
    {
        float distance = Vector3.Distance(enemy.transform.position, enemy.player.position);

        if(distance <= enemy.attackRange)
        {
            enemyStateMachine.ChangeState(enemy.AttackState);
        }
        else
        {
            Vector3 dir = (enemy.player.position - enemy.transform.position).normalized;
            dir.y = 0; //y고정
            enemy.rb.MovePosition(enemy.transform.position + dir * enemy.moveSpeed * Time.deltaTime);

            Vector3 lookDir = enemy.player.position;
            lookDir.y = 0; //y고정
            enemy.transform.LookAt(lookDir);
        }
    }

    public void Exit()
    {
        enemy.enemyAnim.EnemyRunning(false);
    }
}
