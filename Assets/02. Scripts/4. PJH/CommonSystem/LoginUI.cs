using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginUI : Singleton<LoginUI>
{
    #region field
    [Header("메인 UI")]
    [SerializeField] GameObject lodingUI;
    [SerializeField] GameObject loginUI;

    [Header("찾기 / 회원가입 UI")]
    [SerializeField] GameObject signupUI;
    [SerializeField] GameObject findUI;

    public System.Action<Tuple<string, string>> onClickLogin;

    [Header("InputField")]
    //TMP_InputField : console.Readline(); 같이 사용자가 입력한 값 받아오는 타입
    [SerializeField] TMP_InputField inputID;
    [SerializeField] TMP_InputField inputPW;

    [Header("feature Btn")]
    [SerializeField] Button loginBtn;
    [SerializeField] Button findBtn;
    [SerializeField] Button signupBtn;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    #region method
    //데이터 전달 또는 초기화 할 때 사용
    void Init()
    {
        if (!lodingUI.activeSelf) lodingUI.SetActive(true);
        if (loginUI.activeSelf) loginUI.SetActive(false);

        /*
        사용되지 않는 익명함수(핸들러)는 가비지 콜렉터가 사용하지 않을 시 수거
        클로저가 염려될 경우 변수를 선언해서 사용하고 변수를 null 처리해 GC가 캡처된 변수를 수거하게 한다
        */
        loginBtn.onClick.AddListener(() =>
        {
            string id = inputID.text;
            string pw = inputPW.text;
            Debug.LogFormat("id : {0}", id);
            Debug.LogFormat("pw : {0}", pw);

            this.onClickLogin(new Tuple<string, string>(id, pw));
        });

        findBtn.onClick.AddListener(() =>
        {

        });

        signupBtn.onClick.AddListener(() =>
        {

        });
    }

    public void CompleteLoding()
    {
        lodingUI.SetActive(false);
        loginUI.SetActive(true);
    }


    #region UI open / close
    public void LoginUIOpen()
    {
        loginUI.SetActive(true);
    }

    public void LoginUIClose()
    {
        loginUI.SetActive(false);
    }

    public void FindUIOpen()
    {
        findUI.SetActive(true);
    }

    public void FindUIClose()
    {
        findUI.SetActive(false);
    }

    public void SignupUIOpen()
    {
        signupUI.SetActive(true);
    }

    public void SignupUIClose()
    {
        signupUI.SetActive(false);
    }
    #endregion


    #endregion
}
