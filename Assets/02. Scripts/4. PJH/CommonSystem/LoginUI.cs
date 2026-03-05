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
    [SerializeField] Button googleBtn;

    //회원가입 가입하기 버튼
    [SerializeField] Button signupCheckBtn;

    [Header("panel")]
    [SerializeField] GameObject panel;

    [Header("Comfirm UI")]
    [SerializeField] GameObject confirmUI;
    [SerializeField] TextMeshProUGUI confirmText;
    [SerializeField] Button confirmBtn;
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
            //람다식 안에 외부 변수를 사용하면 클로저가 형성되어 해당 변수들이 메모리에 더 오래 남을 수 있다.
            //LoginUI가 씬에서 계속 유지되는 동안에는 큰 문제가 되지 않는다.
            string id = inputID.text;
            string pw = inputPW.text;

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw))
            {
                confirmText.text = "아이디 / 비밀번호를 입력하시오";
                ConfirmUIOpen();
            }
            else if (inputID.text.Trim().Length < 5 || inputPW.text.Trim().Length < 8)
            {
                confirmText.text = "아이디 최소 5자 / 비밀번호 최소 8자";
                ConfirmUIOpen();
            }
            else
            {
                this.onClickLogin?.Invoke(new Tuple<string, string>(id, pw));

                //if ()
                //{
                //    //사용자 데이터 확인 후 로비 씬으로 이동
                //}
            }
        });

        //회원가입
        signupBtn.onClick.AddListener(() =>
        {
            SignupUIOpen();
        });

        signupCheckBtn.onClick.AddListener(() =>
        {
            //아이디, 패스워드 확인 후
            SignupUIClose();
        });

        //구글회원가입 || 로그인
        googleBtn.onClick.AddListener(() =>
        {

        });

        //
        findBtn.onClick.AddListener(() =>
        {

        });

        //알림문구
        confirmBtn.onClick.AddListener(ConfirmUIClose);
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

    public void ConfirmUIOpen()
    {
        panel.SetActive(true);
        confirmUI.SetActive(true);
    }

    public void ConfirmUIClose()
    {
        panel.SetActive(false);
        confirmUI.SetActive(false);
    }
    #endregion


    #endregion
}
