using UnityEngine;

public class LobbyState : IGameState
{
    private GameStateMachine _machine;

    public LobbyState(GameStateMachine machine)
    {
        _machine = machine;
    }

    public void Enter()
    {
        Debug.Log("로비 UI 활성화");
    }

    public void Execute()
    {
        //상태 전환 조건
        if (true)
        {
            _machine.ChangeState(new PlayState(_machine, false));
        }
    }

    public void Exit()
    {
        Debug.Log("로비 UI 비활성화 -> 씬 전환");
    }
}
