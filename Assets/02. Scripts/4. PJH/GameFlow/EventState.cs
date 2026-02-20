using UnityEngine;

public class EventState : IGameState
{
    private GameFlowMachine _machine;

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
        //이벤트 웨이브가 종료되었는지 확인하고 넘긴다
        if (true)
        {
            _machine.ChangeState(new ResultState(_machine), State.Result);
        }
    }

    public void Exit()
    {
        Debug.Log("이벤트 웨이브 종료");
    }
}
