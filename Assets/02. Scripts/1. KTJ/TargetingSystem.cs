using System.Collections.Generic;
using UnityEngine;

public enum TargetingStrategyType
{
    Nearest,
    ForwardCone,
    Line
}

public class TargetingSystem : MonoBehaviour
{
    [Header("타겟 설정")]
    [SerializeField] private float maxRange = 8f;
    [SerializeField] private float coneAngle = 60f;
    [SerializeField, Min(0f)] private float lineWidth = 1.5f;

    [Header("전략")]
    [SerializeField] private TargetingStrategyType strategyType = TargetingStrategyType.Line;

    private readonly List<Transform> targets = new List<Transform>();
    private ITargetingStrategy strategy;

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

    public Transform GetTarget(Vector3 origin, Vector3 forward)
    {
        return GetTarget(origin, forward, 0f);
    }

    public Transform GetTarget(Vector3 origin, Vector3 forward, float rangeOverride)
    {
        CleanupTargets();
        if (strategy == null)
        {
            strategy = CreateStrategy(strategyType);
        }

        var param = strategyType == TargetingStrategyType.Line ? lineWidth : coneAngle;
        var range = rangeOverride > 0f ? rangeOverride : maxRange;
        return strategy.SelectTarget(origin, forward, targets, range, param);
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

    private interface ITargetingStrategy
    {
        Transform SelectTarget(Vector3 origin, Vector3 forward, List<Transform> candidates, float range, float angle);
    }

    private class NearestTargetStrategy : ITargetingStrategy
    {
        public Transform SelectTarget(Vector3 origin, Vector3 forward, List<Transform> candidates, float range, float angle)
        {
            Transform best = null;
            float bestSqr = range * range;

            foreach (var candidate in candidates)
            {
                var diff = candidate.position - origin;
                diff.y = 0f;
                var sqr = diff.sqrMagnitude;
                if (sqr > bestSqr) continue;

                best = candidate;
                bestSqr = sqr;
            }

            return best;
        }
    }

    private class ForwardConeTargetStrategy : ITargetingStrategy
    {
        public Transform SelectTarget(Vector3 origin, Vector3 forward, List<Transform> candidates, float range, float angle)
        {
            Transform best = null;
            float bestSqr = range * range;
            float halfAngle = angle * 0.5f;

            foreach (var candidate in candidates)
            {
                var diff = candidate.position - origin;
                diff.y = 0f;
                var sqr = diff.sqrMagnitude;
                if (sqr > bestSqr) continue;

                var flatForward = forward;
                flatForward.y = 0f;
                if (flatForward.sqrMagnitude <= 0f) continue;

                var dir = diff.normalized;
                var currentAngle = Vector3.Angle(flatForward, dir);
                if (currentAngle > halfAngle) continue;

                best = candidate;
                bestSqr = sqr;
            }

            return best;
        }
    }

    private class LineTargetStrategy : ITargetingStrategy
    {
        public Transform SelectTarget(Vector3 origin, Vector3 forward, List<Transform> candidates, float range, float angle)
        {
            forward.y = 0f;
            if (forward.sqrMagnitude <= 0f) return null;

            Transform best = null;
            float bestDot = 0f;
            float rangeSqr = range * range;
            float lineWidthSqr = angle * angle;
            var dir = forward.normalized;

            foreach (var candidate in candidates)
            {
                var diff = candidate.position - origin;
                diff.y = 0f;
                var sqr = diff.sqrMagnitude;
                if (sqr > rangeSqr) continue;

                var dot = Vector3.Dot(dir, diff);
                if (dot <= 0f || dot > range) continue;

                var perpSqr = sqr - dot * dot;
                if (perpSqr > lineWidthSqr) continue;

                if (dot > bestDot)
                {
                    bestDot = dot;
                    best = candidate;
                }
            }

            return best;
        }
    }
}
