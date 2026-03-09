using Firebase;
using Firebase.Auth;
using Firebase.Extensions; //ContinueWithOnMainThread 사용을 위해 필요
using UnityEngine;

public class FirebaseManager : Singleton<FirebaseManager>
{
    public FirebaseAuth auth;

    protected override void Awake()
    {
        base.Awake();
        Init();
    }

    private void Init()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            DependencyStatus dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                //초기화 성공
                auth = FirebaseAuth.DefaultInstance;
                Debug.Log("Firebase 초기화 성공");
            }
            else
            {
                Debug.LogErrorFormat("Firebase 초기화 실패 : {0}", dependencyStatus);
            }
        });
    }
}
