using UnityEngine;

public class LoadingState : IGameState
{
    private GameFlowMachine _machine;
    private float _loadingTimer = 0f;

    /*
        상태 클래스는 하이어락키에 존재할 필요가 없는 '순수한 로직' 덩어리
        -만약 모든 상태를 컴포넌트로 만들면, 오브젝트에 수십 개의 스크립트가 붙어 관리가 어렵다.
        -new 키워드로 필요한 순간에만 메모리에 잠깐 올렸다 버리는 방식을 사용
    */
    public LoadingState(GameFlowMachine machine)
    {
        _machine = machine;
    }

    public void Enter()
    {
        //현재 상태를 if문으로 확인하고 해당되는 역할을 활성화 한다
        Debug.Log("서버 데이터 로딩 시작 ...");
        Debug.Log("유저 데이터 로딩 시작 ...");
        Debug.Log("스테이지 데이터 로딩 시작 ...");
    }

    public void Execute()
    {
        //!Enter에서 역할을 인지하고 해당되는 메서드를 여기서 실행한다
        _loadingTimer += Time.deltaTime;

        //상황에 맞춰 조건문 추가 작성 필요
        if (_loadingTimer > 2.0f) //'데이터를 전부 다 받아왔을 때'를 조건식으로 변경 필요
        {
            _machine.ChangeState(new LoginState(_machine), State.Login);
        }
    }

    public void Exit()
    {
        //상태 진입할 때 실행했던 메서드 들을 모두 비활성화한다.
        Debug.Log("로딩 완료 및 씬 전환");
    }
}
