using UnityEngine;

public class LobbyState : IGameState
{
    private GameFlowMachine _machine;

    public LobbyState(GameFlowMachine machine)
    {
        _machine = machine;
    }

    public void Enter()
    {
        Debug.Log("로비 UI 활성화");
    }

    public void Execute()
    {
        //상태 전환 조건 : 스테이지 진입 UI 클릭 시
        if (true)
        {
            _machine.ChangeState(new PlayState(_machine), State.Play);
        }
    }

    public void Exit()
    {
        Debug.Log("로비 UI 비활성화 -> 씬 전환");
    }
}
