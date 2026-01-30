using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private ChainComboSystem chainComboSystem;

    [Header("점수관련")]
    [SerializeField] private int killScore;
    [SerializeField] private float chainBonusRate = 0.1f;

    private int totalScore;
    private int currentChain;

    public event Action<int> OnScoreUpdated;

    void Awake()
    {
        totalScore = 0;
        currentChain = 0;

        //콤보시스템 스크립트에서 체인관련 가져오기
        chainComboSystem.OnChainChanged += UpdateChain;
        chainComboSystem.OnChainEnded += ChainEnd;
    }

    public void EnemyKillScore()
    {
        //1.1, 1.2 ...
        float multiplier = 1f + (currentChain * chainBonusRate);

        //float 를 int로 변환(반올림)
        int getScore = Mathf.RoundToInt(killScore * multiplier);
        totalScore += getScore;

        //스코어 업데이트되면 갱신(UI에 쓰기)
        OnScoreUpdated?.Invoke(totalScore);
        Debug.Log($"점수획득: {getScore}, 총 점수: {totalScore}");
    }

    public void UpdateChain(int chain)
    {
        //체인 값이 튀는 것 방지
        currentChain = Mathf.Max(chain, 0);
    }

    private void ChainEnd(int finalChain)
    {
        currentChain = 0;
    }
}
