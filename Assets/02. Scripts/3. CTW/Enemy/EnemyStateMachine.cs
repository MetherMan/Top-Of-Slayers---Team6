using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    public IEnemyState EnemyState { get; private set; }

    public void ChangeState(IEnemyState newState)
    {
        EnemyState?.Exit();
        EnemyState = newState;
        EnemyState.Enter();
    }

    void Update()
    {
        EnemyState?.Update();
    }
}
