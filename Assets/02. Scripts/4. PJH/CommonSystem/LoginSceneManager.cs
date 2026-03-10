using UnityEngine;
/*
    추후 작업하는 걸로 지금은 핵심 기능 구현만 
*/
public class LoginSceneManager : Singleton<LoginSceneManager>
{
    #region field
    public bool confirmDownload;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        
    }

    #region method
    public void CheckIn()
    {
        LoadingSceneController.LoadScene("MainSceneVer2");
    }
    #endregion
}
