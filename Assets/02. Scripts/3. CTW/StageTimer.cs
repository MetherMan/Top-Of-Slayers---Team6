using UnityEngine;
using TMPro;
using DG.Tweening;

public class StageTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText;

    private int lastTime = -1;
    private bool isZoomed = false;

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
        timeText.text = $"{time}";
        //int min = time / 60;
        //int sec = time % 60;
        //timeText.text = $"{min:D1}:{sec:D2}";

        if (time <= 10)
        {
            timeText.color = Color.red;
        }

        if(time == 0 && !isZoomed)
        {
            isZoomed = true;

            timeText.transform.DOKill();
            Sequence seq = DOTween.Sequence();
            seq.Append(timeText.transform.DOScale(2f, 0.3f).SetEase(Ease.OutBack));//확대
            seq.AppendInterval(0.5f);//유지
            seq.Append(timeText.transform.DOScale(1f, 0.3f));//축소
        }
    }
}
