using UnityEngine;
using UnityEngine.UI;

public class StageTimer : MonoBehaviour
{
    [SerializeField] private Image clockFill;
    [SerializeField] private RectTransform clockNeedle;

    private float maxTime;
    private int lastTime = -1;

    private void Start()
    {
        if (StageFlowManager.Instance != null)
        {
            maxTime = StageFlowManager.Instance.remainingTime;

            lastTime = (int)maxTime;
            TimeUI(lastTime);
        }
    }

    void Update()
    {
        if (StageFlowManager.Instance == null) return;
        int time = StageFlowManager.Instance.remainingTime;

        if (time != lastTime)
        {
            lastTime = time;
            TimeUI(time);
        }
    }

    private void TimeUI(int time)
    {
        if (maxTime <= 0)
        {
            Debug.LogWarning("maxTime이 0이라서 회전값을 계산할 수 없습니다");
            return;
        }

        float ratio = ((float)time / maxTime);
        clockFill.fillAmount = ratio;

        float angle = ratio * 360f;

        if (float.IsNaN(angle)) return;
        clockNeedle.localRotation = Quaternion.Euler(0, 0, angle);

        if (angle <= 90)
        {
            clockFill.color = Color.red;
        }
    }
}
