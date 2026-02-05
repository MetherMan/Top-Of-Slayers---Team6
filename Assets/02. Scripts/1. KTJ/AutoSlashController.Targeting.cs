using System.Collections.Generic;
using UnityEngine;

public partial class AutoSlashController
{
    private Transform pendingInitialTarget;
    private Vector3 pendingInitialDirection = Vector3.forward;
    private float pendingInitialTimer;
    private Vector3 lastInitialAimDirection = Vector3.forward;
    private bool hasInitialAimDirection;
    private float initialAimStableTimer;

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

    private bool TryConfirmInitialTarget(Transform candidate, Vector3 candidateDirection, Vector3 rawDirection, float deltaTime, out Transform confirmedTarget, out Vector3 confirmedDirection)
    {
        confirmedTarget = candidate;
        confirmedDirection = candidateDirection;
        if (!useInitialTargetConfirm || initialTargetConfirmTime <= 0f) return candidate != null;
        if (candidate == null)
        {
            ResetInitialTargetConfirm();
            return false;
        }
        if (!IsInitialAimStable())
        {
            ResetInitialTargetConfirm();
            return false;
        }

        rawDirection.y = 0f;
        candidateDirection.y = 0f;
        if (rawDirection.sqrMagnitude <= 0f || candidateDirection.sqrMagnitude <= 0f)
        {
            ResetInitialTargetConfirm();
            return false;
        }

        var angle = Vector3.Angle(rawDirection, candidateDirection);
        if (angle <= initialTargetInstantAngle)
        {
            pendingInitialTarget = candidate;
            pendingInitialDirection = candidateDirection;
            pendingInitialTimer = 0f;
            return true;
        }

        if (candidate != pendingInitialTarget)
        {
            pendingInitialTarget = candidate;
            pendingInitialDirection = candidateDirection;
            pendingInitialTimer = 0f;
            return false;
        }

        pendingInitialTimer += deltaTime;
        if (pendingInitialTimer >= initialTargetConfirmTime)
        {
            confirmedTarget = pendingInitialTarget;
            confirmedDirection = pendingInitialDirection.sqrMagnitude > 0f ? pendingInitialDirection.normalized : candidateDirection.normalized;
            return true;
        }

        return false;
    }

    private void UpdateInitialAimStability(Vector3 rawDirection, float deltaTime, bool isChainActive)
    {
        if (isChainActive)
        {
            ResetInitialAimStability();
            return;
        }
        if (!useInitialAimStability || initialAimStableTime <= 0f)
        {
            initialAimStableTimer = initialAimStableTime;
            return;
        }
        if (deltaTime <= 0f) return;

        rawDirection.y = 0f;
        if (rawDirection.sqrMagnitude <= 0f)
        {
            ResetInitialAimStability();
            return;
        }

        var normalized = rawDirection.normalized;
        if (!hasInitialAimDirection)
        {
            hasInitialAimDirection = true;
            lastInitialAimDirection = normalized;
            initialAimStableTimer = 0f;
            return;
        }

        var angle = Vector3.Angle(lastInitialAimDirection, normalized);
        lastInitialAimDirection = normalized;
        var angularSpeed = angle / deltaTime;
        if (angularSpeed > initialAimMaxAngularSpeed)
        {
            initialAimStableTimer = 0f;
            return;
        }

        initialAimStableTimer += deltaTime;
    }

    private bool IsInitialAimStable()
    {
        if (!useInitialAimStability || initialAimStableTime <= 0f) return true;
        return initialAimStableTimer >= initialAimStableTime;
    }

    private void ResetInitialTargetConfirm()
    {
        pendingInitialTarget = null;
        pendingInitialDirection = Vector3.forward;
        pendingInitialTimer = 0f;
    }

    private void ResetInitialAimStability()
    {
        initialAimStableTimer = 0f;
        hasInitialAimDirection = false;
        lastInitialAimDirection = Vector3.forward;
    }
}
