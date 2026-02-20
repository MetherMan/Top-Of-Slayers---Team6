using System;

/*
    게임루프
*/
public enum State
{
    Loding,
    Login,
    Lobby,
    Play,
    Event,
    Result
}

public interface IGameState
{
    void Enter();
    void Execute();
    void Exit();
}

public class GameFlowMachine : Singleton<GameFlowMachine>
{
    /*
        1. 지금 현재 상태를 EVENT로 전달한다. ?.Invoke();
        
        A - E -> Result / A -> Result
        
    */
    #region field
    IGameState _currentState;
    public static Action<State> currentState; 
    #endregion

    private void Start()
    {
        //객체 생성의 효율성, 안전성
        //클래스의 인스턴스 선언과 같다. 요지는 '지금 즉시 만들어서 할당하는 것' 이다.
        //메모리에 잠깐 올렸다가 버리는 방식
        ChangeState(new LoadingState(this), State.Loding);
    }

    private void Update()
    {
        //'?' Null인지 확인 : Null이라면 아무것도 하지 않는다. :Null이 아니라면 Execute를 실행한다
        //if (_currentState != null) _currentState.Execute();
        _currentState?.Execute();
    }

    #region method
    public void ChangeState(IGameState newState, State state)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();

        //상태가 변경 될 경우 알림
        currentState?.Invoke(state);
    }
    #endregion
}