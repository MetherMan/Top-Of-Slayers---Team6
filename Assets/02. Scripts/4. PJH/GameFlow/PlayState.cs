using UnityEngine;

public class PlayState : IGameState
{
    private GameFlowMachine _machine;
    //GameStateMachine에서 값을 확인하고 받아오는 방식으로 수정 ↓
    public bool isEventActive; //스테이지 룰에서 판정하고 GameFlowManager에서 값 변경

    //이벤트 진행 여부 확인하기 위해 매개변수 추가
    public PlayState(GameFlowMachine machine)
    {
        _machine = machine;
    }

    public void Enter()
    {
        Debug.Log("스테이지 활성화");
    }

    public void Execute()
    {
        //이벤트 활성화 조건 : 스테이지 웨이브 종료 후 활성화
        if (isEventActive) //외부에서 스테이지 웨이브 종료 유무까지 해야하고 그 값을 받아 조건식으로 추가
        {
            _machine.ChangeState(new EventState(_machine), State.Event);
        }
        //스테이지 종료 조건
        if (!isEventActive) //failed인지 cleard인지는 외부에서 값을 받아오는 걸로
        {
            _machine.ChangeState(new ResultState(_machine), State.Result);
        }
    }

    public void Exit()
    {
        Debug.Log("스테이지 비활성화");
    }
}
