using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    private void OnEnable()
    {
        ScoreManager.Instance.onScoreChanged += UpdateScoreUI;
    }

    private void OnDisable()
    {
        ScoreManager.Instance.onScoreChanged -= UpdateScoreUI;
    }

    private void UpdateScoreUI(int score)
    {
        scoreText.text = $"{score}";
    }
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
