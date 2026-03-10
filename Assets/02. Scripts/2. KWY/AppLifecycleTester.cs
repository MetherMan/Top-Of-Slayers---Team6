using UnityEngine;

public class AppLifecycleTester : MonoBehaviour
{
    void Start()
    {
        Debug.Log("앱 시작");
    }

    void Update()
    {
#if UNITY_EDITOR
        // P 누르면 백그라운드 테스트
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("테스트: 백그라운드 상황");
            OnApplicationPause(true);
        }

        // R 누르면 앱 복귀 테스트
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("테스트: 앱 복귀 상황");
            OnApplicationPause(false);
        }

        // F 누르면 포커스 테스트
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("테스트: 포커스 잃음");
            OnApplicationFocus(false);
        }

        // Q 누르면 종료 테스트
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("테스트: 앱 종료");
            OnApplicationQuit();
        }
#endif
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            Debug.Log("OnApplicationPause(true) → 앱이 백그라운드로 이동");
        }
        else
        {
            Debug.Log("OnApplicationPause(false) → 앱이 다시 활성화");
        }
    }

    void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            Debug.Log("OnApplicationFocus(true) → 앱이 포커스를 얻음");
        }
        else
        {
            Debug.Log("OnApplicationFocus(false) → 앱이 포커스를 잃음");
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit → 앱 종료");
    }
}