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
        if (LoginUI.Instance.changeState)
        {
            _machine.ChangeState(new LobbyState(_machine), State.Lobby);
        }
    }

    public void Exit()
    {
        Debug.Log("로그인 UI / 회원가입 UI 비활성화 -> 씬 이동 또는 게임 종료");
    }
}