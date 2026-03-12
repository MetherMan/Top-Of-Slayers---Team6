using Firebase;
using Firebase.Auth;
using Firebase.Extensions; //ContinueWithOnMainThread 사용을 위해 필요
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseManager : Singleton<FirebaseManager>
{
    #region field
    FirebaseApp app;
    FirebaseAuth auth;
    FirebaseUser user;

    //인증상태 변경
    string displayName;
    string emailAddress;
    string uid;
    //System.Uri photoUrl;

    //에러로그 처리
    public System.Action<string, bool> inputFailedEvent;
    public System.Action<string, bool> createFailedEvent;

    FirebaseFirestore db;

    //유저 게임 데이터
    List<InventoryItem> iventory = new List<InventoryItem>();
    List<InventoryItem> equipment = new List<InventoryItem>();

    //로그인 시 : User.userId keyValue / iventory, equipment 데이터 가져오기

    //아이템 획득 : iventory 리스트 추가
    //아이템 제거 : iventory 리스트 수정/제거 4개중 3개만 팔고 1개 남았을 경우

    //장비 착용 : equipment 리스트 추가
    //장비 해제 : equipment 리스트 제거

    //골드 획득, 사용 : user[2] int 값 수정

    //스테이지 클리어 : 아이템 획득, 골드 획득
    //가챠 : 아이템 획득
    //인벤토리 : 장비 착용, 아이템 제거
    //상점 : 골드 사용 - 터치를 여러 번 할 수 있다.
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
                db = FirebaseFirestore.DefaultInstance;

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

    #region 로그인, 계정관리
    //회원가입
    public void CreateID(Tuple<string, string> idPw)
    {
        auth.CreateUserWithEmailAndPasswordAsync(idPw.Item1, idPw.Item2).ContinueWithOnMainThread(task =>
        {
            string message;
            if (task.IsCanceled)
            {
                Debug.LogWarning("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Firebase.FirebaseException e
                = task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;

                if (e != null)
                {
                    Firebase.Auth.AuthError errorCode = (Firebase.Auth.AuthError)e.ErrorCode;

                    switch (errorCode)
                    {
                        case Firebase.Auth.AuthError.EmailAlreadyInUse:
                            {
                                message = "이미 존재하는 계정입니다";
                                createFailedEvent?.Invoke(message, false);
                            }
                            break;
                        case Firebase.Auth.AuthError.InvalidEmail:
                            {
                                message = "올바른 이메일 주소를 입력하세요";
                                createFailedEvent?.Invoke(message, false);
                            }
                            break;
                        case Firebase.Auth.AuthError.WeakPassword:
                            {
                                message = "최소 6자 이상";
                                createFailedEvent?.Invoke(message, false);
                            }
                            break;
                        case Firebase.Auth.AuthError.MissingEmail:
                            {
                                message = "이메일 칸이 비어 있습니다.";
                                createFailedEvent?.Invoke(message, false);
                            }
                            break;
                        case Firebase.Auth.AuthError.MissingPassword:
                            {
                                message = "비밀번호 칸이 비어 있습니다.";
                                createFailedEvent?.Invoke(message, false);
                            }
                            break;
                    }
                }
            }
            else if (task.IsCompleted)
            {
                //Firebase user has been created.
                AuthResult result = task.Result;
                Debug.LogFormat("Firebase user created successfully : {0} ({1})",
                    result.User.DisplayName, result.User.UserId);

                CreateUserData(result); //파이어베이스 데이터 생성

                message = "계정 생성 성공";
                createFailedEvent?.Invoke(message, true);
            }

        });
    }

    //기존 사용자 로그인
    public void Login(Tuple<string, string> idPw)
    {
        auth.SignInWithEmailAndPasswordAsync(idPw.Item1, idPw.Item2).ContinueWithOnMainThread(task =>
        {
            string message = "";
            if (task.IsCanceled)
            {
                Debug.LogWarning("SignInWithEmailAndPasswordAsync was cancled.");
                return;
            }
            if (task.IsFaulted)
            {
                Firebase.FirebaseException e
                = task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;


                if (e != null)
                {
                    Firebase.Auth.AuthError errorCode = (Firebase.Auth.AuthError)e.ErrorCode;

                    switch (errorCode)
                    {
                        case Firebase.Auth.AuthError.InvalidEmail:
                            {
                                message = "이메일 형식이 올바르지 않습니다.";
                                inputFailedEvent?.Invoke(message, false);
                            }
                            break;
                        case Firebase.Auth.AuthError.WrongPassword:
                            {
                                message = "비밀번호가 틀렸습니다";
                                inputFailedEvent?.Invoke(message, false);
                            }
                            break;
                        case Firebase.Auth.AuthError.UserNotFound:
                            {
                                message = "존재하지 않는 계정입니다.";
                                inputFailedEvent?.Invoke(message, false);
                            }
                            break;
                        default:
                            {
                                message = "로그인 오류: " + errorCode.ToString();
                                inputFailedEvent?.Invoke(message, false);
                            }
                            break;
                    }
                }

                Debug.LogWarning(message);
            }
            if (task.IsCompleted)
            {
                Firebase.Auth.AuthResult result = task.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                    result.User.DisplayName, result.User.UserId);

                message = "로그인 성공";
                inputFailedEvent?.Invoke(message, true);
            }
        });
    }

    //인증 상태 변경 이벤트 핸들러 설정 및 사용자 데이터 가져오기
    private void AuthStateChanged(object sender, System.EventArgs eventArgs)
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

    //유저 데이터 생성
    private void CreateUserData(AuthResult result)
    {
        DocumentReference documentReference = db.Collection("UserData").Document(result.User.UserId);
        Dictionary<string, object> initialization = new Dictionary<string, object>()
        {
            { "level", 1 },
            { "Exp", 0 }
        };

        Dictionary<string, object> inventory = new Dictionary<string, object>()
        {
            { "0", null }
        };

        Dictionary<string, object> equipment = new Dictionary<string, object>()
        {
            { "weapon", null },
            { "shoes", null },
            { "gloves", null },
            { "armor", null },
            { "emblem", null }
        };

        Dictionary<string, object> cost = new Dictionary<string, object>()
        {
            { "gold", 0 }, //골드
            { "energe", 60 } //행동력
        };

        Dictionary<string, object> user = new Dictionary<string, object>
        {
            { "User", initialization },
            { "Inventory", inventory },
            { "Equipment", equipment },
            { "cost", cost }
        };

        documentReference.SetAsync(user).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.IsFaulted)
            {
                Debug.Log("Firebase: 유저 데이터 생성 성공");
            }
            else
            {
                Debug.LogError("Firebase: 유저데이터 생성 실패");
            }
        });
    }

    public void GetUserData(string uid)
    {
        CollectionReference userRef = db.Collection("UserData");
        userRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.IsFaulted)
            {
                QuerySnapshot snapshot = task.Result;
                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    if (document.Id == uid)
                    {
                        Dictionary<string, object> documentDictionary = document.ToDictionary();
                        RefreshUserData(documentDictionary);
                        break;
                    }
                }
            }
            else
            {
                Debug.Log("GetSnapshotAsync failed");
            }
        });
    }

    private void RefreshUserData(Dictionary<string, object> uid)
    {
        foreach (KeyValuePair<string, object> user in uid)
        {
            if (user.Key == "User")
            {
                Dictionary<string, object> userDetail = user.Value as Dictionary<string, object>;
                if (userDetail != null)
                {
                    
                }
                else
                {
                    Debug.LogWarning("User Data Refresh Failed");
                }
            }
            else if (user.Key == "Inventory")
            {
                Dictionary<string, object> inventory = user.Value as Dictionary<string, object>;
                if (inventory != null)
                {

                }
                else
                {
                    Debug.LogWarning("Inventory Data Refresh Failed");
                }
            }
            else if (user.Key == "Equipment")
            {
                Dictionary<string, object> equipment = user.Value as Dictionary<string, object>;
                if (equipment != null)
                {

                }
                else
                {
                    Debug.LogWarning("Equipment Data Refresh Failed");
                }
            }
            else if (user.Key == "cost")
            {
                Dictionary<string, object> cost = user.Value as Dictionary<string, object>;
                if (cost != null)
                {

                }
                else
                {
                    Debug.LogWarning("Cost Data Refresh Failed");
                }
            }
            
        }
    }

    public void FirebaseRefreshItem(string uid, bool get)
    {
        if (get)
        {
            //db.Collection("UserData").Document(uid).
        }
        else if (!get)
        {

        }
    }

    //메모리
    public void registerAuthEvent()
    {
        auth.StateChanged += AuthStateChanged;
        Debug.Log("AuthStateChanged 이벤트 등록");
    }

    public void UnregisterAuthEvent()
    {
        auth.StateChanged -= AuthStateChanged;
        Debug.Log("AuthStateChanged 이벤트 해제");
    }
    #endregion

    #region 유저 데이터관리

    #endregion

    #endregion
}