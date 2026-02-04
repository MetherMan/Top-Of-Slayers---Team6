using UnityEngine;

public partial class AutoSlashController
{
    private Transform pendingChainTarget;
    private Vector3 pendingChainDirection;
    private float pendingChainTimer;
    private Transform lastAttackTarget;
    private bool sameTargetReleased = true;
    private Vector3 lastAttackAimDirection = Vector3.forward;
    private bool hasLastAttackAim;
    private Vector2 lastAttackInput;
    private bool hasLastAttackInput;
    private float sameTargetReleaseTimer;
    private float lastSameTargetBlockLogTime;

    private Vector3 GetAimOrigin(bool isChainActive)
    {
        if (!isChainActive || !useLastTargetAsAimOrigin) return transform.position;
        if (chainCombat == null) return transform.position;
        var lastTarget = chainCombat.LastTarget;
        if (lastTarget == null) return transform.position;
        return lastTarget.position;
    }

    private Transform GetIgnoreTarget(bool isChainActive, Vector3 aimDirection)
    {
        if (!isChainActive || !ignoreLastTargetDuringChain) return null;
        if (chainCombat == null) return null;
        var lastTarget = chainCombat.LastTarget;
        if (lastTarget == null) return null;
        if (useSameTargetRelease && !sameTargetReleased && lastAttackTarget == lastTarget)
        {
            return lastTarget;
        }
        if (IsSameTargetAllowed(lastTarget, aimDirection)) return null;
        return lastTarget;
    }

    private bool IsSameTargetAllowed(Transform target, Vector3 aimDirection)
    {
        if (target == null) return false;

        var toTarget = target.position - transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude <= 0f) return true;

        var angle = Vector3.Angle(aimDirection, toTarget.normalized);
        return angle <= allowSameTargetAngle;
    }

    private void RegisterAttackTarget(Transform target, Vector3 rawAimDirection)
    {
        if (target == null) return;
        lastAttackTarget = target;
        sameTargetReleased = false;
        rawAimDirection.y = 0f;
        if (rawAimDirection.sqrMagnitude > 0f)
        {
            lastAttackAimDirection = rawAimDirection.normalized;
            hasLastAttackAim = true;
        }
        else
        {
            hasLastAttackAim = false;
        }
        CaptureLastAttackInput();
        if (enableAimDebugLog)
        {
            var inputText = hasLastAttackInput ? lastAttackInput.ToString("F2") : "없음";
            var aimText = hasLastAttackAim ? lastAttackAimDirection.ToString("F2") : "없음";
            Debug.Log($"[조준] 타겟등록:{target.name} 입력:{inputText} aim:{aimText}");
        }
    }

    private void ResetSameTargetRelease()
    {
        if (enableAimDebugLog && lastAttackTarget != null)
        {
            Debug.Log($"[조준] 재공격리셋:{lastAttackTarget.name}");
        }
        lastAttackTarget = null;
        sameTargetReleased = true;
        hasLastAttackAim = false;
        hasLastAttackInput = false;
        sameTargetReleaseTimer = 0f;
    }

    private void UpdateSameTargetRelease(Vector3 rawAimDirection)
    {
        if (!useSameTargetRelease) return;
        if (sameTargetReleaseAngle <= 0f)
        {
            sameTargetReleased = true;
            return;
        }
        if (lastAttackTarget == null) return;

        var releaseHoldTime = chainTargetConfirmTime > 0f ? chainTargetConfirmTime : 0.05f;
        var delta = Time.unscaledDeltaTime;

        if (moveController != null && hasLastAttackInput)
        {
            var input = moveController.GetAimInput();
            if (input.sqrMagnitude > 0f)
            {
                var inputAngle = Vector2.Angle(input, lastAttackInput);
                if (inputAngle >= sameTargetReleaseAngle)
                {
                    sameTargetReleaseTimer += delta;
                    if (sameTargetReleaseTimer >= releaseHoldTime)
                    {
                        sameTargetReleased = true;
                        if (enableAimDebugLog)
                        {
                            Debug.Log($"[조준] 재공격해제(입력) 각도:{inputAngle:F1} 시간:{sameTargetReleaseTimer:F2}");
                        }
                    }
                    return;
                }

                sameTargetReleaseTimer = 0f;
                return;
            }
        }

        if (!hasLastAttackAim) return;

        rawAimDirection.y = 0f;
        if (rawAimDirection.sqrMagnitude <= 0f) return;
        var aimAngle = Vector3.Angle(rawAimDirection, lastAttackAimDirection);
        if (aimAngle >= sameTargetReleaseAngle)
        {
            sameTargetReleaseTimer += delta;
            if (sameTargetReleaseTimer >= releaseHoldTime)
            {
                sameTargetReleased = true;
                if (enableAimDebugLog)
                {
                    Debug.Log($"[조준] 재공격해제(에임) 각도:{aimAngle:F1} 시간:{sameTargetReleaseTimer:F2}");
                }
            }
            return;
        }
        sameTargetReleaseTimer = 0f;
    }

    private void LogSameTargetBlock(Transform target)
    {
        if (!enableAimDebugLog) return;
        var time = Time.unscaledTime;
        if (time - lastSameTargetBlockLogTime < 0.2f) return;
        lastSameTargetBlockLogTime = time;
        var targetName = target != null ? target.name : "없음";
        var lastName = lastAttackTarget != null ? lastAttackTarget.name : "없음";
        Debug.Log($"[조준] 재공격차단 target:{targetName} last:{lastName} released:{sameTargetReleased}");
    }

    private void CaptureLastAttackInput()
    {
        if (moveController == null)
        {
            hasLastAttackInput = false;
            return;
        }

        var input = moveController.GetAimInput();
        if (input.sqrMagnitude <= 0f)
        {
            hasLastAttackInput = false;
            return;
        }

        lastAttackInput = input.normalized;
        hasLastAttackInput = true;
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
