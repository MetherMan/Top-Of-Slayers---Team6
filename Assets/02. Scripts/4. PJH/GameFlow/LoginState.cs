using UnityEngine;

public class LoginState : IGameState
{
    private GameFlowMachine _machine;

    public LoginState(GameFlowMachine machine)
    {
        _machine = machine;
    }

    public void Enter()
    {
        Debug.Log("로그인 UI / 회원가입 UI 활성화");
    }

    public void Execute()
    {
        Debug.Log("로그인 완료");
        //해당 유저 데이터 가져오기, 신규 유저일 경우 건너뛰기
        _machine.ChangeState(new LobbyState(_machine));
    }

    public void Exit()
    {
        Debug.Log("로그인 UI / 회원가입 UI 비활성화 -> 씬 이동 또는 게임 종료");
    }
}