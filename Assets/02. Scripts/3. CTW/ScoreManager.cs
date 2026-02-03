using System;
using UnityEngine;

public class ScoreManager : ChainComboDecorator
{
    private int killScore;
    private Action<int> onScore;

    public ScoreManager(IChainCombo inner, int killScore, Action<int> onScore) : base(inner)
    {
        this.killScore = killScore;
        this.onScore = onScore;
    }

    public override void ChainUp()
    {
        base.ChainUp();

        //스코어 변동되면 작동
        onScore?.Invoke(killScore);
    }
}
