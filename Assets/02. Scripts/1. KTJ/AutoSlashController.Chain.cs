using UnityEngine;

public partial class AutoSlashController
{
    private Transform pendingChainTarget;
    private Vector3 pendingChainDirection;
    private float pendingChainTimer;

    private Vector3 GetAimOrigin(bool isChainActive)
    {
        if (!isChainActive || !useLastTargetAsAimOrigin) return transform.position;
        if (chainCombat == null) return transform.position;
        var lastTarget = chainCombat.LastTarget;
        if (lastTarget == null) return transform.position;
        return lastTarget.position;
    }

    private bool TryGetChainPriorityTarget(Vector3 origin, Vector3 rawDirection, float range, Transform ignoreTarget, out Transform target, out Vector3 aimDirection)
    {
        target = null;
        aimDirection = rawDirection;
        if (targetingSystem == null) return false;

        if (TryGetRetainedChainTarget(origin, rawDirection, range, out var retainedTarget, out var retainedDirection))
        {
            target = retainedTarget;
            aimDirection = retainedDirection;
            return true;
        }

        var lineTarget = GetLinePriorityTarget(origin, rawDirection, range, ignoreTarget, out var lineDirection);
        if (lineTarget != null)
        {
            if (!IsAimAngleAllowed(rawDirection, lineDirection))
            {
                return false;
            }
            target = lineTarget;
            aimDirection = lineDirection;
            return true;
        }

        var angleTarget = targetingSystem.GetTargetByAngle(origin, rawDirection, range, ignoreTarget);
        if (angleTarget == null) return false;

        var toTarget = angleTarget.position - origin;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude <= 0f) return false;

        if (!IsAimAngleAllowed(rawDirection, toTarget))
        {
            return false;
        }

        target = angleTarget;
        aimDirection = toTarget.normalized;
        return true;
    }

    private bool TryGetRetainedChainTarget(Vector3 origin, Vector3 rawDirection, float range, out Transform target, out Vector3 aimDirection)
    {
        target = null;
        aimDirection = rawDirection;
        if (!useChainTargetRetention) return false;
        if (chainCombat == null) return false;

        var lastTarget = chainCombat.LastTarget;
        if (lastTarget == null) return false;
        if (useSameTargetRelease && !sameTargetReleased && lastAttackTarget == lastTarget)
        {
            return false;
        }

        var toTarget = lastTarget.position - origin;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude <= 0f) return false;

        var checkRange = range > 0f ? range : (targetingSystem != null ? targetingSystem.MaxRange : 0f);
        if (checkRange > 0f && toTarget.sqrMagnitude > checkRange * checkRange) return false;

        if (!IsAimAngleAllowed(rawDirection, toTarget)) return false;

        var angle = Vector3.Angle(rawDirection, toTarget);
        if (angle > chainRetainTargetAngle) return false;

        target = lastTarget;
        aimDirection = toTarget.normalized;
        return true;
    }

    private Transform GetLinePriorityTarget(Vector3 origin, Vector3 rawDirection, float range, Transform ignoreTarget, out Vector3 aimDirection)
    {
        aimDirection = rawDirection;
        if (targetingSystem == null) return null;

        var candidates = targetingSystem.GetTargetsInLine(origin, rawDirection, range, ignoreTarget);
        if (candidates == null || candidates.Count == 0) return null;

        rawDirection.y = 0f;
        if (rawDirection.sqrMagnitude <= 0f) return null;

        var dir = rawDirection.normalized;
        Transform best = null;
        float bestDot = float.MinValue;
        float bestPerpSqr = float.MaxValue;

        for (int i = 0; i < candidates.Count; i++)
        {
            var candidate = candidates[i];
            if (candidate == null || candidate == ignoreTarget) continue;

            var diff = candidate.position - origin;
            diff.y = 0f;
            var dot = Vector3.Dot(dir, diff);
            var perpSqr = diff.sqrMagnitude - dot * dot;

            var isBetterDot = dot > bestDot + 0.001f;
            var isSameDot = Mathf.Abs(dot - bestDot) <= 0.001f;
            if (isBetterDot || (isSameDot && perpSqr < bestPerpSqr))
            {
                best = candidate;
                bestDot = dot;
                bestPerpSqr = perpSqr;
            }
        }

        if (best == null) return null;

        var toTarget = best.position - origin;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude > 0f)
        {
            aimDirection = toTarget.normalized;
        }

        return best;
    }

    private bool IsAimAngleAllowed(Vector3 rawDirection, Vector3 toTarget)
    {
        if (chainAimMaxAngle <= 0f) return true;

        rawDirection.y = 0f;
        toTarget.y = 0f;
        if (rawDirection.sqrMagnitude <= 0f || toTarget.sqrMagnitude <= 0f) return false;

        var angle = Vector3.Angle(rawDirection, toTarget);
        return angle <= chainAimMaxAngle;
    }

    private bool TryConfirmChainTarget(Transform candidate, Vector3 candidateDirection, Vector3 rawDirection, float deltaTime, out Transform confirmedTarget, out Vector3 confirmedDirection)
    {
        confirmedTarget = candidate;
        confirmedDirection = candidateDirection;
        var confirmTime = chainTargetConfirmTime;
        if (chainCombat != null && candidate != null && candidate == chainCombat.LastTarget && chainSameTargetConfirmTime > 0f)
        {
            confirmTime = chainSameTargetConfirmTime;
        }
        if (!useChainTargetConfirm || confirmTime <= 0f) return candidate != null;
        if (candidate == null) return false;

        rawDirection.y = 0f;
        candidateDirection.y = 0f;
        if (rawDirection.sqrMagnitude <= 0f || candidateDirection.sqrMagnitude <= 0f) return false;

        var angle = Vector3.Angle(rawDirection, candidateDirection);
        if (angle <= chainTargetInstantAngle)
        {
            pendingChainTarget = candidate;
            pendingChainDirection = candidateDirection;
            pendingChainTimer = 0f;
            return true;
        }

        if (candidate != pendingChainTarget)
        {
            pendingChainTarget = candidate;
            pendingChainDirection = candidateDirection;
            pendingChainTimer = 0f;
            return false;
        }

        pendingChainTimer += deltaTime;
        if (pendingChainTimer >= confirmTime)
        {
            confirmedTarget = pendingChainTarget;
            confirmedDirection = pendingChainDirection.sqrMagnitude > 0f ? pendingChainDirection.normalized : candidateDirection.normalized;
            return true;
        }

        return false;
    }

    private void ResetChainTargetConfirm()
    {
        pendingChainTarget = null;
        pendingChainDirection = Vector3.forward;
        pendingChainTimer = 0f;
    }
}
