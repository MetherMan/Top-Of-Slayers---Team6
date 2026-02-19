using UnityEngine;

public class ResultState : IGameState
{
    private GameStateMachine _machine;
    private bool _isClear;
    private bool _eventClear;

    public ResultState(GameStateMachine machine, bool isClear, bool eventClear)
    {
        _machine = machine;
        _isClear = isClear;
        _eventClear = eventClear;
    }

    public void Enter()
    {
        if (_isClear)
        {
            if (_eventClear)
            {
                Debug.Log("결과창 UI / 스테이지 보상 + 이벤트 보상 획득");
                return;
            }
            Debug.Log("결과창 UI / 스테이지 보상 획득");
        }
        else if (!_isClear)
        {
            if (_eventClear)
            {
                Debug.Log("결과창 UI 활성화 / 보상 획득 불가 / 이벤트 보상 획득");
                return;
            }
            Debug.Log("결과창 UI 활성화 / 보상 획득 불가");
        }
    }

    public void Execute()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) //UI '나가기' 터치로 변경
        {
            //로비로 돌아가기
            _machine.ChangeState(new LobbyState(_machine));
        }
        else if (Input.GetKeyDown(KeyCode.R)) //UI '재시작' 터치로 변경
        {
            //스테이지 재도전
            _machine.ChangeState(new LoadingState(_machine));
        }
    }

    public void Exit()
    {
        Debug.Log("결과 창 UI 비활성화 -> 씬 전환");
    }
}