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
    public System.Action<Tuple<string, string>> onClickCreate;

    [Header("로그인 입력란")]
    //TMP_InputField : console.Readline(); 같이 사용자가 입력한 값 받아오는 타입
    [SerializeField] TMP_InputField inputEmail;
    [SerializeField] TMP_InputField inputPW;

    [Header("로그인 버튼")]
    [SerializeField] Button loginBtn;
    [SerializeField] Button findBtn;
    [SerializeField] Button signupBtn;
    [SerializeField] Button googleBtn;

    [Header("회원가입 입력란")]
    [SerializeField] TMP_InputField upInputEmail;
    [SerializeField] TMP_InputField upInputPW;
    [SerializeField] TMP_InputField upInputRePW;
    [SerializeField] Button signupCheckBtn;
    [SerializeField] Button cancleBtn;

    [Header("panel")]
    [SerializeField] GameObject panel;

    [Header("Comfirm UI")]
    [SerializeField] GameObject confirmUI;
    [SerializeField] public TextMeshProUGUI confirmText;
    [SerializeField] Button confirmBtn;

    [Header("Download UI")]
    [SerializeField] GameObject downloadUI;
    [SerializeField] public TextMeshProUGUI downloadText;
    [SerializeField] Button yBtn;
    [SerializeField] Button nBtn;
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

        FirebaseManager.Instance.inputFailedEvent += CheckInputFailed;
        FirebaseManager.Instance.createFailedEvent += CreateInputFailed;

        /*
        사용되지 않는 익명함수(핸들러)는 가비지 콜렉터가 사용하지 않을 시 수거
        클로저가 염려될 경우 변수를 선언해서 사용하고 변수를 null 처리해 GC가 캡처된 변수를 수거하게 한다
        */
        loginBtn.onClick.AddListener(() =>
        {
            //람다식 안에 외부 변수를 사용하면 클로저가 형성되어 해당 변수들이 메모리에 더 오래 남을 수 있다.
            //LoginUI가 씬에서 계속 유지되는 동안에는 큰 문제가 되지 않는다.
            string email = inputEmail.text;
            string pw = inputPW.text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pw))
            {
                confirmText.text = "이메일 / 비밀번호를 입력하시오";
                ConfirmUIOpen();
            }
            else if (pw.Trim().Length < 6)
            {
                confirmText.text = "비밀번호 최소 6자";
                ConfirmUIOpen();
            }
            else
            {
                this.onClickLogin?.Invoke(new Tuple<string, string>(email, pw));
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
            string upId = upInputEmail.text;
            string upPw = upInputPW.text;
            string upRePw = upInputRePW.text;

            if (upPw.CompareTo(upRePw) != 0)
            {
                //문자열 비교 : https://developer-talk.tistory.com/223
                confirmText.text = "비밀번호가 확인란과 동일하지 않습니다.";
                ConfirmUIOpen();
            }
            else
            {
                onClickCreate?.Invoke(new Tuple<string, string>(upId, upPw));
            }
        });

        cancleBtn.onClick.AddListener(SignupUIClose);

        //일단 보류
        findBtn.onClick.AddListener(() =>
        {

        });

        //알림문구
        confirmBtn.onClick.AddListener(ConfirmUIClose);

        yBtn.onClick.AddListener(() =>
        {
            LoginSceneManager.Instance.confirmDownload = true;
            DownloadUIClose();
        });

        //어플리케이션 종료 : https://intunknown.tistory.com/entry/unity-%EA%B2%8C%EC%9E%84%EC%A2%85%EB%A3%8C-%EB%B2%84%ED%8A%BC-%EB%A7%8C%EB%93%A4%EA%B8%B0
        nBtn.onClick.AddListener(Application.Quit);
    }

    public void CompleteLoding()
    {
        lodingUI.SetActive(false);
        loginUI.SetActive(true);
    }

    public void CheckInputFailed(string message, bool check)
    {
        if(!check)
        {
            confirmText.text = message;
            ConfirmUIOpen();
        }
        if (check)
        {
            CancleEvent();
            LoginSceneManager.Instance.CheckIn();
        }

    }

    public void CreateInputFailed(string message, bool check)
    {
        if(!check)
        {
            confirmText.text = message;
            ConfirmUIOpen();
        }
        else if (check)
        {
            SignupUIClose();
        }
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

    public void DownloadUIOpen()
    {
        panel.SetActive(true);
        downloadUI.SetActive(true);
    }

    public void DownloadUIClose()
    {
        panel.SetActive(false);
        downloadUI.SetActive(false);
    }
    #endregion

    //메모리 : 구독 해제
    public void CancleEvent()
    {
        FirebaseManager.Instance.inputFailedEvent -= CheckInputFailed;
        FirebaseManager.Instance.createFailedEvent -= CreateInputFailed;
    }
    #endregion
}
