using UnityEngine;

public class LoginSceneManager : Singleton<LoginSceneManager>
{
    #region field
    [SerializeField] GameObject LodingUI;
    [SerializeField] GameObject LoginUI;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        Initialization();
    }

    #region method
    void Initialization()
    {
        if (!LodingUI.activeSelf) LodingUI.SetActive(true);
        if (LoginUI.activeSelf) LoginUI.SetActive(false);
    }

    public void CompleteLoding()
    {
        LodingUI.SetActive(false);
        LoginUI.SetActive(true);
    }
    #endregion
}