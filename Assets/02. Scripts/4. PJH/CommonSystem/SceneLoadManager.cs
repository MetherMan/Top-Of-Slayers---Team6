using UnityEngine;
using UnityEngine.AddressableAssets;

public class SceneLoadManager : Singleton<SceneLoadManager>
{
    /*
        SceneSlot.cs
        : StageConfigSO를 찾아올 Key값을 변수로 가지고 있다.
        : 버튼 UI에 컴포넌트로 할당된다.
        : 버튼을 터치할 경우 IPointerClickHandler 매서드를 실행해
            SceneSlot -> SceneLoadManager Key값을 전달

            slot : enum으로 씬 구분
            -로그인창 : 로그아웃 누를 경우
            -로비에서 스테이지 선택 : 넘버, 난이도 
                 : 진행단계가 없을 경우 첫 번째 스테이지
                 :: 진행단계가 있을 경우 데이터를 받아올 것

        SceneLoadManager
        : SceneSlot에서 enum, key값을 받아와 해당 씬을 활성화 한다.

            GameFlowManager에서 보내오는 요청 실행

    [SRP 주의]
    */
    #region field
    [Header("고정 씬 설정")]
    public AssetReference loginScene;
    public AssetReference lobbyScene;
    #endregion

    protected override void Awake()
    {
        base.Awake();
    }

    #region method
    public void LoadLoginScene()
    {
        LoadScene(loginScene);
    }

    public void LoadLobbyScene()
    {
        LoadScene(lobbyScene);
    }

    //고정 씬 메서드
    private void LoadScene(AssetReference sceneRef)
    {
        Addressables.LoadSceneAsync(sceneRef);
    }

    public void ActiveScene(SceneSlot slot)
    {
        SceneType sceneType = slot.sceneType;

        switch (sceneType)
        {
            case SceneType.Login:
                {
                    LoadLoginScene();
                }
                break;
            case SceneType.Lobby:
                {
                    LoadLobbyScene();
                }
                break;
            case SceneType.Stage:
                {
                    AddressableManager.Instance.RequestStageScene(slot.stageKey);
                }
                break;
        }
    }
    #endregion
}