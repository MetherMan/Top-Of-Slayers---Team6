using UnityEngine;

public class EnemyIdleState : IEnemyState
{
    private EnemyBase enemy;
    private EnemyStateMachine enemyStateMachine;

    private float idleTime = 1f;
    private float timer;

    public EnemyIdleState(EnemyBase enemy, EnemyStateMachine enemyStateMachine)
    {
        this.enemy = enemy;
        this.enemyStateMachine = enemyStateMachine;
    }
    public void Enter()
    {
        timer = 0f;

        enemy.enemyAnim.EnemyRunning(false);
        enemy.enemyAnim.EnemyAttack(false);
    }

    
    public void Update()
    {
        timer += Time.deltaTime;

        if(timer >= idleTime)
        {
            enemyStateMachine.ChangeState(enemy.FollowState);
        }
    }

    public void Exit()
    {

    }
}
