using UnityEngine;

public class EventState : IGameState
{
    private GameFlowMachine _machine;
    public bool isSuccess;

    public EventState(GameFlowMachine machine)
    {
        _machine = machine;
    }

    public void Enter()
    {
        Debug.Log("이벤트 웨이브 진입");
    }

    public void Execute()
    {
        if (isSuccess)
        {
            _machine.ChangeState(new PlayState(_machine, true));
        }
        else if (!isSuccess)
        {
            _machine.ChangeState(new PlayState(_machine, false));
        }
    }

    public void Exit()
    {
        Debug.Log("이벤트 웨이브 종료");
    }
}
