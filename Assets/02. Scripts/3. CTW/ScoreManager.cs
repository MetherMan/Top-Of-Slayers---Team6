using System;
using UnityEngine;

public class ScoreManager : Singleton<ScoreManager>
{
    private int totalScore;
    public event Action<int> onScoreChanged;

    protected override void Awake()
    {
        base.Awake();
    }
    private void OnEnable()
    {
        EnemyBase.OnEnemyKilled += AddScore;
    }

    private void OnDisable()
    {
        EnemyBase.OnEnemyKilled -= AddScore;
    }

    public void AddScore(int score)
    {
        totalScore += score;
        onScoreChanged?.Invoke(totalScore);
    }

    public void RestScore()
    {
        totalScore = 0;
        onScoreChanged?.Invoke(totalScore);
    }
}
