using System.Collections.Generic;
using UnityEngine;

public partial class TargetingSystem
{
    public Transform GetTarget(Vector3 origin, Vector3 forward)
    {
        return GetTarget(origin, forward, 0f, null);
    }

    public Transform GetTarget(Vector3 origin, Vector3 forward, float rangeOverride)
    {
        return GetTarget(origin, forward, rangeOverride, null);
    }

    public Transform GetTarget(Vector3 origin, Vector3 forward, float rangeOverride, Transform ignoreTarget)
    {
        CleanupTargets();
        if (strategy == null)
        {
            strategy = CreateStrategy(strategyType);
        }

        var param = strategyType == TargetingStrategyType.Line ? lineWidth : coneAngle;
        var range = rangeOverride > 0f ? rangeOverride : maxRange;
        if (strategyType == TargetingStrategyType.Line && lineEndPadding > 0f)
        {
            range += lineEndPadding;
        }
        return strategy.SelectTarget(origin, forward, targets, range, param, ignoreTarget);
    }

    public List<Transform> GetTargetsInLine(Vector3 origin, Vector3 forward, float rangeOverride, Transform ignoreTarget)
    {
        var result = new List<Transform>();
        GetTargetsInLineNonAlloc(origin, forward, rangeOverride, ignoreTarget, result);
        return result;
    }

    public void GetTargetsInLineNonAlloc(
        Vector3 origin,
        Vector3 forward,
        float rangeOverride,
        Transform ignoreTarget,
        List<Transform> result)
    {
        if (result == null) return;

        result.Clear();
        CleanupTargets();

        forward.y = 0f;
        if (forward.sqrMagnitude <= 0f) return;

        var range = rangeOverride > 0f ? rangeOverride : maxRange;
        if (lineEndPadding > 0f)
        {
            range += lineEndPadding;
        }

        var dir = forward.normalized;
        var rangeSqr = range * range;
        var lineWidthSqr = lineWidth * lineWidth;

        for (int i = 0; i < targets.Count; i++)
        {
            var candidate = targets[i].Target;
            if (candidate == null || candidate == ignoreTarget) continue;

            var diff = candidate.position - origin;
            diff.y = 0f;
            if (diff.sqrMagnitude <= TargetCoincidentSqrThreshold) continue;
            var sqr = diff.sqrMagnitude;
            if (sqr > rangeSqr) continue;

            var dot = Vector3.Dot(dir, diff);
            if (dot < 0f || dot > range) continue;

            var perpSqr = sqr - dot * dot;
            if (perpSqr > lineWidthSqr) continue;

            result.Add(candidate);
        }
    }

    public Transform GetTargetNearPoint(Vector3 point, float radius, Transform ignoreTarget)
    {
        CleanupTargets();
        if (radius <= 0f) return null;

        Transform best = null;
        var bestSqr = radius * radius;

        for (int i = 0; i < targets.Count; i++)
        {
            var candidate = targets[i].Target;
            if (candidate == null || candidate == ignoreTarget) continue;

            var diff = candidate.position - point;
            diff.y = 0f;
            if (diff.sqrMagnitude <= TargetCoincidentSqrThreshold) continue;
            var sqr = diff.sqrMagnitude;
            if (sqr > bestSqr) continue;

            best = candidate;
            bestSqr = sqr;
        }

        return best;
    }

    public Transform GetTargetByAngle(Vector3 origin, Vector3 forward, float rangeOverride, Transform ignoreTarget)
    {
        CleanupTargets();

        forward.y = 0f;
        if (forward.sqrMagnitude <= 0f) return null;

        var range = rangeOverride > 0f ? rangeOverride : maxRange;
        var rangeSqr = range * range;
        var dir = forward.normalized;

        Transform best = null;
        float bestDirectionScore = float.MinValue;
        float bestSqr = float.MaxValue;

        for (int i = 0; i < targets.Count; i++)
        {
            var candidate = targets[i].Target;
            if (candidate == null || candidate == ignoreTarget) continue;

            var diff = candidate.position - origin;
            diff.y = 0f;
            if (diff.sqrMagnitude <= TargetCoincidentSqrThreshold) continue;
            var sqr = diff.sqrMagnitude;
            if (sqr > rangeSqr) continue;

            var directionScore = Vector3.Dot(dir, diff) / Mathf.Sqrt(sqr);
            var isBetterDirection = directionScore > bestDirectionScore + TargetDirectionScoreEpsilon;
            var isSameDirection = Mathf.Abs(directionScore - bestDirectionScore) <= TargetDirectionScoreEpsilon;
            if (isBetterDirection || (isSameDirection && sqr < bestSqr))
            {
                best = candidate;
                bestDirectionScore = directionScore;
                bestSqr = sqr;
            }
        }

        return best;
    }
}
