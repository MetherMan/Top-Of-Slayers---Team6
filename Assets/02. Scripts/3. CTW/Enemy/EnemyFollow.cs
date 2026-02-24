using UnityEngine;

public class EnemyFollow : IEnemyState
{
    private readonly EnemyBase enemy;
    private readonly EnemyStateMachine enemyStateMachine;

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
        var playerTransform = enemy.ResolvePlayer();
        if (playerTransform == null) return;

        var currentPosition = enemy.rb != null ? enemy.rb.position : enemy.transform.position;
        var targetPosition = playerTransform.position;
        targetPosition.y = currentPosition.y;

        var toTarget = targetPosition - currentPosition;
        var sqrDistance = toTarget.sqrMagnitude;
        if (sqrDistance <= enemy.attackRange * enemy.attackRange)
        {
            enemyStateMachine.ChangeState(enemy.AttackState);
            return;
        }

        if (sqrDistance <= 0.0001f) return;

        var moveDirection = toTarget.normalized;
        var moveStep = enemy.moveSpeed * Time.deltaTime;
        if (moveStep <= 0f) return;

        var nextPosition = currentPosition + moveDirection * moveStep;
        var targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);

        if (enemy.rb != null)
        {
            enemy.rb.MovePosition(nextPosition);
            enemy.rb.MoveRotation(targetRotation);
            return;
        }

        enemy.transform.position = nextPosition;
        enemy.transform.rotation = targetRotation;
    }

    public void Exit()
    {
        enemy.enemyAnim.EnemyRunning(false);
    }
}
