using TMPro;
using UnityEngine;
using DG.Tweening;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("DOTween")]
    [SerializeField] private float punchScale = 0.2f;   //커지는 배율
    [SerializeField] private float punchDuration = 0.2f;//커지고 돌아오는 시간
    [SerializeField] private float countDuration = 0.1f;//숫자가 변하는 시간

    private int currentScore = 0;

    private void OnEnable()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.onScoreChanged += UpdateScoreUI;
        }
    }

    private void OnDisable()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.onScoreChanged -= UpdateScoreUI;
        }
    }

    private void UpdateScoreUI(int score)
    {
        //숫자 증가
        DOTween.To(() => currentScore, x =>
        {
            currentScore = x;
            scoreText.text = currentScore.ToString("N0");
        }, score, countDuration);
        scoreText.text = $"{score}";

        scoreText.transform.DOKill(); //이전 트윈이 있으면 제거
        scoreText.transform.localScale = Vector3.one;
        //커졌다가 돌아오기
        scoreText.transform.DOPunchScale(Vector3.one * punchScale, punchDuration, 1, 0.5f);
    }
}
