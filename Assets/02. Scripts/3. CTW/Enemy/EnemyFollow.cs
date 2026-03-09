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
        if (toTarget.sqrMagnitude <= enemy.attackRange * enemy.attackRange)
        {
            if (enemy.attackType == AttackType.Ranged && !enemy.HasLineOfSightToPlayer(playerTransform))
            {
                return;
            }

            enemyStateMachine.ChangeState(enemy.AttackState);
        }
    }

    public void FixedUpdate()
    {
        var playerTransform = enemy.ResolvePlayer();
        if (playerTransform == null) return;

        var currentPosition = enemy.rb != null ? enemy.rb.position : enemy.transform.position;
        var targetPosition = playerTransform.position;
        targetPosition.y = currentPosition.y;

        var toTarget = targetPosition - currentPosition;
        var sqrDistance = toTarget.sqrMagnitude;
        var inAttackRange = sqrDistance <= enemy.attackRange * enemy.attackRange;
        if (inAttackRange && (enemy.attackType != AttackType.Ranged || enemy.HasLineOfSightToPlayer(playerTransform))) return;
        if (sqrDistance <= 0.0001f) return;

        var moveDirection = toTarget.normalized;
        var chaseSpeedMultiplier = enemy.GetChaseSpeedMultiplier(currentPosition);
        var moveStep = enemy.moveSpeed * chaseSpeedMultiplier * Time.fixedDeltaTime;
        if (moveStep <= 0f) return;

        var nextPosition = currentPosition + moveDirection * moveStep;
        var targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        var currentRotation = enemy.rb != null ? enemy.rb.rotation : enemy.transform.rotation;
        var nextRotation = Quaternion.RotateTowards(
            currentRotation,
            targetRotation,
            enemy.TurnSpeedDegrees * Time.fixedDeltaTime);

        if (enemy.rb != null)
        {
            enemy.rb.MovePosition(nextPosition);
            enemy.rb.MoveRotation(nextRotation);
            return;
        }

        enemy.transform.position = nextPosition;
        enemy.transform.rotation = nextRotation;
    }

    public void Exit()
    {
        enemy.enemyAnim.EnemyRunning(false);
    }
}
