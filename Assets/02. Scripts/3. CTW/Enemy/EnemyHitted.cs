using UnityEngine;

public class EnemyHitted : IEnemyState
{
    private EnemyBase enemy;
    private EnemyStateMachine enemyStateMachine;

    private float hitTime = 0.5f;
    private float timer;
    public EnemyHitted(EnemyBase enemy, EnemyStateMachine enemyStateMachine)
    {
        this.enemy = enemy;
        this.enemyStateMachine = enemyStateMachine;
    }

    public void Enter()
    {
        timer = hitTime;

        enemy.enemyAnim.EnemyHitted();

        enemy.enemyAnim.EnemyRunning(false);
        enemy.enemyAnim.EnemyAttack(false);
    }

    public void Update()
    {
        timer -= Time.deltaTime;

        if(timer <= 0)
        {
            enemy.enemyStateMachine.ChangeState(enemy.FollowState);
        }
    }

    public void Exit()
    {

    }
}
