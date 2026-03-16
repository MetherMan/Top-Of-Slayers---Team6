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
        StageFlowManager.Instance.stageIn = false;
        Debug.Log("<color=green>스테이지 활성화</color>");
    }

    public void Execute()
    {
        //스테이지 종료 조건
        if (GameFlowManager.Instance.Cleared) //failed인지 cleard인지는 외부에서 값을 받아오는 걸로
        {
            GameFlowManager.Instance.Cleared = false;
            _machine.ChangeState(new ResultState(_machine), State.Result);
        }
    }

    public void Exit()
    {
        Debug.Log("스테이지 비활성화");
    }
}
