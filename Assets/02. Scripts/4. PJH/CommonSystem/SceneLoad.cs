using UnityEngine;

public class SceneLoad : Singleton<SceneLoad>
{   /*
    !!우선 GameStageMachine은 배제하고 구현한 다음 수정하도록 한다.
    !!상태구분은 GameStageMachine에서 정의하고 상태를 전환한다.

    SceneSlot.cs
        : 버튼 UI에 붙일 컴포넌트
        : 해당 스테이지의 키값을 담고 있는 slot
        : 역할 - 버튼을 터치했을 때, 1-1스테이지 입니다 라는 키값을 반환

            [slot 상태구분 : enum으로 씬 구분] - GameStageMachine
            -로그인창 : 로그아웃 누를 경우
            -로비에서 스테이지 선택 : 넘버, 난이도 
            -빠른 스테이지 이동
                : 진행단계가 없을 경우 첫 번째 스테이지
                :: 진행단계가 있을 경우 데이터를 받아올 것

    SceneLoad.cs 
        : 1-1 스테이지라는 키값을 받아온다
        : 키 값을 기준으로 AddressableManager에서 해당되는 StageDataSO와 Scene을 가져온다.
        : 역할 - 터치한 키 값을 받아와 SceneLoadManager에 ReQuest를 보내 해당되는
        -Result value를 가져와 씬을 불러온다.
            ^

            [SceneLoad 상태구분] - GameStageMachine
            -로그인창 : 사전에 값을 기입
            -로비 : 사전에 값을 기입
            -스테이지 : 클릭 이벤트로 데이터를 가져와 addressable로 씬 데이터를 가져온다

    SceneLoadManager.cs 
        : AddressableManager에 모든 스테이지 데이터와 씬 데이터를 받아와 각 딕셔너리에 보관한다.
        : SceneLoad에서 Quest가 올 경우 해당되는 Scene과 StageDataSO를 보내준다.
        : 역할 - 모든 스테이지 데이터, 씬 데이터를 관리하고 요청이 올 경우 해당 데이터를 보내준다.
           + GameFlowManager에서 보내오는 요청 실행한다. 게임실행 / 게임종료

    [SRP 위반 조심]
    씬을 실행시키는 메서드는 SceneLoad에서 할 것인가? Manager에서 해야 하는 건가?
        :: SceneLoad에서 씬을 불러오게 한다. - 역할 구분
    */
    #region field
    private bool _isLoading = false;
    #endregion

    protected override void Awake()
    {
        base.Awake();
    }

    void Update()
    {
        
    }

    #region method
    public void LoadData(SceneSlot sceneSlot)
    {
        //중복 터치로 인한 오류 방지
        if (_isLoading) return;
        _isLoading = true;


    }
    #endregion
}