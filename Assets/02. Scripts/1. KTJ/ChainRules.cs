using System;

/// <summary>
/// Pure chain-combat rules. Unity lifecycle, time scale and presentation stay
/// in ChainCombatController while progression can be tested independently.
/// </summary>
public sealed class ChainRules
{
    private readonly float damageIncreaseRate;
    private int lastTargetId;

    public ChainRules(float damageIncreaseRate)
    {
        this.damageIncreaseRate = Math.Max(0f, damageIncreaseRate);
    }

    public int CurrentChain { get; private set; }

    public float GetDamageMultiplier(int targetId)
    {
        if (targetId == 0) return 1f;

        var nextChain = targetId != lastTargetId ? CurrentChain + 1 : CurrentChain;
        if (nextChain <= 0) nextChain = 1;

        return 1f + damageIncreaseRate * (nextChain - 1);
    }

    public ChainTransition RegisterHit(int targetId)
    {
        if (targetId == 0)
        {
            return new ChainTransition(CurrentChain, CurrentChain, false);
        }

        var previousChain = CurrentChain;
        if (targetId != lastTargetId || CurrentChain <= 0)
        {
            CurrentChain = Math.Max(1, CurrentChain + 1);
        }

        lastTargetId = targetId;
        return new ChainTransition(previousChain, CurrentChain, CurrentChain != previousChain);
    }

    public void Reset()
    {
        CurrentChain = 0;
        lastTargetId = 0;
    }
}

public readonly struct ChainTransition
{
    public ChainTransition(int previousChain, int currentChain, bool advanced)
    {
        PreviousChain = previousChain;
        CurrentChain = currentChain;
        Advanced = advanced;
    }

    public int PreviousChain { get; }
    public int CurrentChain { get; }
    public bool Advanced { get; }
}
