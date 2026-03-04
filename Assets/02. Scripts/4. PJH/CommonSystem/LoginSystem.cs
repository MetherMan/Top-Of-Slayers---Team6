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

    void Update()
    {
        
    }

    #region method
    public void PushUserData()
    {
        LoginUI.Instance.onClickLogin = (data) =>
        {
            Debug.LogFormat("id : {0}, pw : {1}", data.Item1, data.Item2);
        };
    }

    #endregion
}