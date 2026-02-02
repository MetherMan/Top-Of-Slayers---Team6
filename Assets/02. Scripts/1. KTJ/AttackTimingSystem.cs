using UnityEngine;

public enum TimingGrade
{
    Miss,
    Good,
    Perfect
}

public class AttackTimingSystem : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private AttackSpecSO spec;

    [Header("전략")]
    [SerializeField] private TimingStrategyType strategyType = TimingStrategyType.Window;

    private IAttackTimingStrategy strategy;

    private void Awake()
    {
        strategy = CreateStrategy(strategyType);
    }

    public TimingGrade EvaluateTiming(float gauge01)
    {
        if (strategy == null)
        {
            strategy = CreateStrategy(strategyType);
        }

        return strategy.Evaluate(Mathf.Clamp01(gauge01), spec);
    }

    public void SetSpec(AttackSpecSO newSpec)
    {
        if (newSpec == null) return;
        spec = newSpec;
    }

    public void SetStrategy(TimingStrategyType type)
    {
        strategyType = type;
        strategy = CreateStrategy(type);
    }

    private IAttackTimingStrategy CreateStrategy(TimingStrategyType type)
    {
        switch (type)
        {
            case TimingStrategyType.Simple:
                return new SimpleTimingStrategy();
            default:
                return new WindowTimingStrategy();
        }
    }

    private interface IAttackTimingStrategy
    {
        TimingGrade Evaluate(float gauge01, AttackSpecSO spec);
    }

    private class WindowTimingStrategy : IAttackTimingStrategy
    {
        public TimingGrade Evaluate(float gauge01, AttackSpecSO spec)
        {
            if (spec == null) return TimingGrade.Miss;

            if (gauge01 >= spec.perfectMin && gauge01 <= spec.perfectMax)
            {
                return TimingGrade.Perfect;
            }

            if (gauge01 >= spec.goodMin && gauge01 <= spec.goodMax)
            {
                return TimingGrade.Good;
            }

            return TimingGrade.Miss;
        }
    }

    private class SimpleTimingStrategy : IAttackTimingStrategy
    {
        public TimingGrade Evaluate(float gauge01, AttackSpecSO spec)
        {
            if (gauge01 >= 0.5f)
            {
                return TimingGrade.Good;
            }

            return TimingGrade.Miss;
        }
    }

    public enum TimingStrategyType
    {
        Window,
        Simple
    }
}
