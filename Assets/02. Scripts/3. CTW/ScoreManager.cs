using System;
using UnityEngine;

public class ScoreManager : ChainComboDecorator
{
    private int killScore;
    private float chainBonusRate;
    private Action<int> onScore;

    public ScoreManager(IChainCombo inner, int killScore, float chainBonusRate, Action<int> onScore) : base(inner)
    {
        this.killScore = killScore;
        this.chainBonusRate = chainBonusRate;
        this.onScore = onScore;
    }

    public override void ChainUp()
    {
        base.ChainUp();

        float multiplier = 1f + inner.CurrentChain * chainBonusRate;
        //정수로 반올림
        onScore?.Invoke(Mathf.RoundToInt(killScore * multiplier));
    }
}
