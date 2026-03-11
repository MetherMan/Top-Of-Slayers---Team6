using Firebase;
using Firebase.Auth;
using Firebase.Extensions; //ContinueWithOnMainThread 사용을 위해 필요
using System;
using UnityEngine;

public class FirebaseManager : Singleton<FirebaseManager>
{
    #region field
    public FirebaseApp app;
    public FirebaseAuth auth;
    public FirebaseUser user;

    //인증상태 변경
    string displayName;
    string emailAddress;
    string uid;
    //System.Uri photoUrl;

    #endregion

    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    #region method
    private void Init()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            DependencyStatus dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                //초기화 성공
                app = FirebaseApp.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;

                registerAuthEvent();
                AuthStateChanged(this, null);
                Debug.Log("Firebase 초기화 성공");
            }
            else
            {
                Debug.LogErrorFormat("Firebase 초기화 실패 : {0}", dependencyStatus);
            }
        });
    }

    //회원가입
    public void CreateID(Tuple<string, string> idPw)
    {
        auth.CreateUserWithEmailAndPasswordAsync(idPw.Item1, idPw.Item2).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                //Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogWarning("CreateUserWithEmailAndPasswordAsync encountered an error : "
                    + task.Exception);
                return;
            }

            //Firebase user has been created.
            AuthResult result = task.Result;
            Debug.LogFormat("Firebase user created successfully : {0} ({1})",
                result.User.DisplayName, result.User.UserId);
        });
    }

    //기존 사용자 로그인
    public void Login(Tuple<string, string> idPw)
    {
        auth.SignInWithEmailAndPasswordAsync(idPw.Item1, idPw.Item2).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                //Debug.LogError("SignInWithEmailAndPasswordAsync was cancled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogWarning("SignInWithEmailAndPasswordAsync encountered an error: "
                    + task.Exception);
            }

            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);
        });
    }

    //인증 상태 변경 이벤트 핸들러 설정 및 사용자 데이터 가져오기
    public void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            /*
            1. IsValid()가 필요한 이유 (C++와 C#의 차이) 파이어베이스 유니티 SDK는 내부적으로
            **C++**로 작성된 엔진을 **C#**에서 쓸 수 있게 연결(Wrapper)한 구조입니다.

            null 체크: "비어 있는가?"만 확인합니다.

            IsValid() 체크: "비어 있지는 않은데, 혹시 내부 엔진(C++)에서 이미 파괴되었거나
            연결이 끊긴 '껍데기' 상태는 아닌가?"를 확인합니다. 비유하자면, null 체크는
            **"내 손에 핸드폰이 있는가?"**를 보는 것이고, IsValid()는 **"그 핸드폰이
            *고장 나지 않고 전원이 켜지는가?"**를 확인하는 과정이라고 보시면 됩니다. 
            */
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null
                && auth.CurrentUser.IsValid();

            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }

            user = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
                //Null Coalescing Operator / 널 병합 연산자
                displayName = user.DisplayName ?? "";
                emailAddress = user.Email ?? "";
                uid = user.UserId ?? "";
                //photoUrl = user.PhotoUrl ?? "";
            }
        }
    }

    public void registerAuthEvent()
    {
        auth.StateChanged += AuthStateChanged;
        Debug.Log("AuthStateChanged 이벤트 등록");
    }

    //메모리 : 구독 해제
    public void UnregisterAuthEvent()
    {
        auth.StateChanged -= AuthStateChanged;
        Debug.Log("AuthStateChanged 이벤트 해제");
    }
    #endregion
}