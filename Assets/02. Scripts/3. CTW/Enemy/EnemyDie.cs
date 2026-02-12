using UnityEngine;

public class EnemyDie : IEnemyState
{
    private EnemyBase enemy;
    private EnemyStateMachine enemyStateMachine;

    public EnemyDie(EnemyBase enemy, EnemyStateMachine enemyStateMachine)
    {
        this.enemy = enemy;
        this.enemyStateMachine = enemyStateMachine;
    }

    public void Enter()
    {
        enemy.enemyAnim.EnemyDie();
        enemy.enemyAnim.EnemyRunning(false);
        enemy.enemyAnim.EnemyAttack(false);

        enemy.rb.velocity = Vector3.zero;
        enemy.rb.isKinematic = true;
    }

    public void Update()
    {
        
    }

    public void Exit()
    {

    }
}
