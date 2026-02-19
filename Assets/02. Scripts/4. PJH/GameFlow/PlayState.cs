using UnityEngine;

public class PlayState : IGameState
{
    private GameStateMachine _machine;
    private bool _eventCleared; //이벤트 진행 종료 후 값 변경
    //GameStateMachine에서 값을 확인하고 받아오는 방식으로 수정 ↓
    public bool cleared; //스테이지 룰에서 판정하고 GameFlowManager에서 값 변경
    public bool isEventActive; //스테이지 룰에서 판정하고 GameFlowManager에서 값 변경

    //이벤트 진행 여부 확인하기 위해 매개변수 추가
    public PlayState(GameStateMachine machine, bool eventCleared)
    {
        _machine = machine;
        _eventCleared = eventCleared;
    }

    public void Enter()
    {
        Debug.Log("스테이지 활성화");
    }

    public void Execute()
    {
        //이벤트 활성화 조건 : 스테이지 웨이브 종료 후 활성화
        if (isEventActive)
        {
            _machine.ChangeState(new EventState(_machine));
        }
        //스테이지 종료 조건
        if (!isEventActive)
        {
            _machine.ChangeState(new ResultState(_machine, cleared, _eventCleared));
        }
    }

    public void Exit()
    {
        Debug.Log("스테이지 비활성화");
    }
}
