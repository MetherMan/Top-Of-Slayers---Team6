using UnityEngine;

public class SlowModeManager : MonoBehaviour
{
    [Header("세팅")]
    [SerializeField] private float slowTimeScale = 0.5f; //얼마나 느려지는지
    [SerializeField] private int slowChainCount = 5;     //슬로우 들어가는 체인카운트
    [SerializeField] private float recoverSpeed = 3f;    //슬로우에서 돌아가는 속도

    private bool isSlowMode;

    void Update()
    {
        if (!isSlowMode) return;

        //슬로우모드 타임스케일에서 1f까지 서서히 속도 올라가기
        Time.timeScale = Mathf.Lerp(Time.timeScale, 1.01f, Time.unscaledDeltaTime * recoverSpeed);

        if (Time.timeScale >= 1f)
        {
            CommonMode();
        }
    }

    public void OnChainChanged(int chain)
    {
        //슬로우체인카운트 넘어가면 슬로우모드
        if (chain >= slowChainCount)
        {
            SlowMode();
        }
    }

    private void SlowMode()
    {
        //슬로우모드 시작
        isSlowMode = true;
        Time.timeScale = slowTimeScale;
        Debug.Log("슬로우모드");
    }

    private void CommonMode()
    {
        //슬로우모드에서 복귀
        isSlowMode = false;
        Time.timeScale = 1f;
        Debug.Log("슬로우복귀");
    }
}
