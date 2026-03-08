using UnityEngine;
/*
    추후 작업하는 걸로 지금은 핵심 기능 구현만 
*/
public class LoginSceneManager : Singleton<LoginSceneManager>
{
    #region field
    #endregion

    protected override void Awake()
    {
        base.Awake();
        
    }

    #region method
    public void CheckIn()
    {
        SceneLoadManager.Instance.ActiveScene("MainSceneVer2");
        Debug.Log("씬이동");
    }
    #endregion
}