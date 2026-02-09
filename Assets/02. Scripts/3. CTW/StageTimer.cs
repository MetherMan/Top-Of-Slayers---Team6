using UnityEngine;
using TMPro;

public class StageTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText;

    private int lastTime = -1;

    void Update()
    {
        if(StageFlowManager.Instance == null) return;

        int time = StageFlowManager.Instance.remainingTime;

        //시간이 바뀔때만 UI갱신
        if(time != lastTime)
        {
            lastTime = time;
            TimeUI(time);
        }
    }

    private void TimeUI(int time)
    {
        int min = time / 60;
        int sec = time % 60;
        timeText.text = $"{min:D1}:{sec:D2}";
    }
}
