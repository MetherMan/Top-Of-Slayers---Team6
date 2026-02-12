using UnityEngine;
using UnityEngine.EventSystems;

public enum SceneType
{
    Login,
    Lobby,
    Stage
}

public class SceneSlot : MonoBehaviour, IPointerClickHandler
{
    #region field
    [SerializeField] SceneType sceneType;
    [SerializeField] int? stageNum = null;

    #endregion

    void Awake()
    {
        //fairbase 유저 데이터 받아오기 : 로그인 접속 시
        //if ()
        //{
        //    sceneType = ;
        //    stageNum = ;
        //}
    }

    #region method
    public void OnPointerClick(PointerEventData eventData)
    {
        if (SceneLoad.Instance != null)
        {
            SceneLoad.Instance.LoadData(this);
        }
        else
        {
            Debug.LogError("SceneLoad를 찾을 수 없습니다.");
        }
    }

    #endregion
}