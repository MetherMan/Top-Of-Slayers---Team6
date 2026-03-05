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
        LoginUI.Instance.onClickLogin -= PushUserData;
        LoginUI.Instance.onClickLogin += PushUserData;
    }

    void Update()
    {
        
    }

    #region method
    public void PushUserData(Tuple<string, string> data)
    {
        Debug.LogFormat("<color=add8e6ff>id : {0}, pw : {1}</color>", data.Item1, data.Item2);
    }

    #endregion
}