using System.Collections.Generic;
using UnityEngine;

public enum TargetingStrategyType
{
    Nearest,
    ForwardCone,
    Line
}

public partial class TargetingSystem : MonoBehaviour
{
    [Header("타겟 설정")]
    [SerializeField] private float maxRange = 8f;
    [SerializeField] private float coneAngle = 60f;
    [SerializeField, Min(0f)] private float lineWidth = 1.5f;
    [SerializeField, Min(0f)] private float lineEndPadding = 0.1f;

    [Header("전략")]
    [SerializeField] private TargetingStrategyType strategyType = TargetingStrategyType.Line;

    private readonly List<Transform> targets = new List<Transform>();
    private ITargetingStrategy strategy;

    public float MaxRange => maxRange;
    public float LineWidth => lineWidth;
    public float LineEndPadding => lineEndPadding;
    public TargetingStrategyType StrategyType => strategyType;

    private void Awake()
    {
        strategy = CreateStrategy(strategyType);
    }

    public void RegisterTarget(Transform target)
    {
        if (target == null) return;
        if (!targets.Contains(target))
        {
            targets.Add(target);
        }
    }

    public void UnregisterTarget(Transform target)
    {
        if (target == null) return;
        targets.Remove(target);
    }

    public int GetActiveTargetCount()
    {
        CleanupTargets();
        return targets.Count;
    }

    public void SetStrategy(TargetingStrategyType type)
    {
        strategyType = type;
        strategy = CreateStrategy(type);
    }

    private void CleanupTargets()
    {
        for (int i = targets.Count - 1; i >= 0; i--)
        {
            if (targets[i] == null)
            {
                targets.RemoveAt(i);
            }
        }
    }

    private ITargetingStrategy CreateStrategy(TargetingStrategyType type)
    {
        switch (type)
        {
            case TargetingStrategyType.ForwardCone:
                return new ForwardConeTargetStrategy();
            case TargetingStrategyType.Line:
                return new LineTargetStrategy();
            default:
                return new NearestTargetStrategy();
        }
    }
}
