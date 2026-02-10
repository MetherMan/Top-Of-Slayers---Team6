using UnityEngine;

public class SceneLoadManager : Singleton<SceneLoadManager>
{
    /*
        slot : enum으로 씬 구분
            -로그인창 : 로그아웃 누를 경우
            -로비에서 스테이지 선택 : 넘버, 난이도 
                 :: 진행단계가 없을 경우 첫 번째 스테이지
                 ::: 진행단계가 있을 경우 데이터를 받아올 것

        SceneLoad
            -로그인창 : 사전에 값을 기입
            -로비 : 사전에 값을 기입
            -스테이지 : 클릭 이벤트로 데이터를 가져와 addressable로 씬 데이터를 가져온다

        SceneLoadManager : 기능 실행

        GameFlowManager에서 보내오는 요청 실행
    */
    #region field

    #endregion

    protected override void Awake()
    {
        base.Awake();
    }

    #region method
    
    #endregion
}