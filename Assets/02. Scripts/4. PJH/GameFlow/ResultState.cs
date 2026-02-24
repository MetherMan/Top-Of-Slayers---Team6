using UnityEngine;

public class ResultState : IGameState
{
    /*
    어떤 역할?
    결과상태다 라고 상태만 전달?
    UI를 어떤걸 활성화 시킬지 확인하고 여기서 활성화 시킬 필요가?

    재시작 할 건지 로비로 갈 건지 어떤 선택을 했는지만 받아오면?
    */
    private GameFlowMachine _machine;

    public ResultState(GameFlowMachine machine)
    {
        _machine = machine;
    }

    public void Enter()
    {
     
    }

    public void Execute()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) //UI '나가기' 터치로 변경
        {
            //로비로 돌아가기
            _machine.ChangeState(new LobbyState(_machine), State.Lobby);
        }
        else if (Input.GetKeyDown(KeyCode.R)) //UI '재시작' 터치로 변경
        {
            //스테이지 재도전
            _machine.ChangeState(new LoadingState(_machine), State.Play);
        }
    }

    public void Exit()
    {
        Debug.Log("결과 창 UI 비활성화 -> 씬 전환");
    }
}