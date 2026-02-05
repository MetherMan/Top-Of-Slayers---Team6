using System.Collections.Generic;
using UnityEngine;

public partial class TargetingSystem
{
    private interface ITargetingStrategy
    {
        Transform SelectTarget(Vector3 origin, Vector3 forward, List<Transform> candidates, float range, float angle, Transform ignoreTarget);
    }

    private class NearestTargetStrategy : ITargetingStrategy
    {
        public Transform SelectTarget(Vector3 origin, Vector3 forward, List<Transform> candidates, float range, float angle, Transform ignoreTarget)
        {
            Transform best = null;
            float bestSqr = range * range;

            foreach (var candidate in candidates)
            {
                if (candidate == null || candidate == ignoreTarget) continue;
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
        public Transform SelectTarget(Vector3 origin, Vector3 forward, List<Transform> candidates, float range, float angle, Transform ignoreTarget)
        {
            Transform best = null;
            float bestSqr = range * range;
            float halfAngle = angle * 0.5f;

            foreach (var candidate in candidates)
            {
                if (candidate == null || candidate == ignoreTarget) continue;
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
        public Transform SelectTarget(Vector3 origin, Vector3 forward, List<Transform> candidates, float range, float angle, Transform ignoreTarget)
        {
            forward.y = 0f;
            if (forward.sqrMagnitude <= 0f) return null;

            Transform best = null;
            float bestDot = 0f;
            float bestPerpSqr = float.MaxValue;
            float rangeSqr = range * range;
            float lineWidthSqr = angle * angle;
            var dir = forward.normalized;

            foreach (var candidate in candidates)
            {
                if (candidate == null || candidate == ignoreTarget) continue;
                var diff = candidate.position - origin;
                diff.y = 0f;
                var sqr = diff.sqrMagnitude;
                if (sqr > rangeSqr) continue;

                var dot = Vector3.Dot(dir, diff);
                if (dot < 0f || dot > range) continue;

                var perpSqr = sqr - dot * dot;
                if (perpSqr > lineWidthSqr) continue;

                var isBetterDot = dot > bestDot + 0.001f;
                var isSameDot = Mathf.Abs(dot - bestDot) <= 0.001f;
                if (isBetterDot || (isSameDot && perpSqr < bestPerpSqr))
                {
                    bestDot = dot;
                    bestPerpSqr = perpSqr;
                    best = candidate;
                }
            }

            return best;
        }
    }
}
