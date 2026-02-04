using System.Collections.Generic;
using UnityEngine;

public partial class AutoSlashController
{
    private List<Transform> GetPierceTargets(bool isChainActive, Vector3 origin, Vector3 aimDirection, float searchRange, Transform ignoreTarget, Transform selectedTarget)
    {
        if (!useChainLinePierce || !isChainActive) return null;
        if (targetingSystem == null) return null;
        if (targetingSystem.StrategyType != TargetingStrategyType.Line) return null;

        var targets = targetingSystem.GetTargetsInLine(origin, aimDirection, searchRange, ignoreTarget);
        if (targets == null || targets.Count == 0) return null;
        if (selectedTarget != null && !targets.Contains(selectedTarget))
        {
            targets.Add(selectedTarget);
        }
        return targets;
    }

    private bool TryGetAimAssistDirection(bool isChainActive, Vector3 aimOrigin, Vector3 baseAimDirection, float searchRange, Transform ignoreTarget, out Vector3 adjustedDirection, out Transform assistTarget)
    {
        adjustedDirection = baseAimDirection;
        assistTarget = null;

        if (!useAimAssist) return false;
        if (aimAssistOnlyDuringChain && !isChainActive) return false;
        if (targetingSystem == null) return false;

        var target = targetingSystem.GetTarget(aimOrigin, baseAimDirection, searchRange, ignoreTarget);
        if (target == null && aimAssistRadius > 0f)
        {
            var range = searchRange > 0f ? searchRange : targetingSystem.MaxRange;
            if (range > 0f)
            {
                var aimPoint = aimOrigin + baseAimDirection.normalized * range;
                target = targetingSystem.GetTargetNearPoint(aimPoint, aimAssistRadius, ignoreTarget);
            }
        }
        if (target == null) return false;

        var toTarget = target.position - aimOrigin;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude <= 0f) return false;

        var normalized = toTarget.normalized;
        var angle = Vector3.Angle(baseAimDirection, normalized);
        if (angle > aimAssistAngle) return false;

        adjustedDirection = normalized;
        assistTarget = target;
        return true;
    }
}
