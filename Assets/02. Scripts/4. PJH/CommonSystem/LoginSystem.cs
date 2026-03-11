using System;
using UnityEngine;
/*
기능만 작성
UI 활성화 비활성화는 LoginSceneManager에서
*/
public class LoginSystem : MonoBehaviour
{
    #region field
    
    #endregion

    void Awake()
    {
        
    }

    private void Start()
    {
        RefreshEvent();
    }

    void Update()
    {
        
    }

    #region method
    //로그인 확인
    public void PushUserData(Tuple<string, string> data)
    {
        Debug.LogFormat("<color=fff>id : {0}, pw : {1}</color>", data.Item1, data.Item2);

        //오류, 버그 검증 x 일단 테스트
        FirebaseManager.Instance.Login(data);
    }

    //회원가입 확인
    public void PushCreateData(Tuple<string, string> data)
    {
        Debug.LogFormat("<color=green>create id : {0}, pw : {1}</color>", data.Item1, data.Item2);

        FirebaseManager.Instance.CreateID(data);
    }

    private void RefreshEvent()
    {
        LoginUI.Instance.onClickLogin -= PushUserData;
        LoginUI.Instance.onClickLogin += PushUserData;

        LoginUI.Instance.onClickCreate -= PushCreateData;
        LoginUI.Instance.onClickCreate += PushCreateData;
    }
    #endregion
}