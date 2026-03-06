using UnityEngine;

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
        LoadingSceneController.LoadScene("MainSceneVer2");
    }
    #endregion
}