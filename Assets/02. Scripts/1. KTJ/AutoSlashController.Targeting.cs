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
    private bool hadChainAimState;

    public float GetCurrentTargetConfirmProgress(Transform target)
    {
        if (target == null) return 0f;

        var isChainActive = chainCombat != null && chainCombat.IsSlowActive;
        var confirmProgress = isChainActive ? GetChainConfirmProgress(target) : GetInitialConfirmProgress(target);
        if (!isReadyWaiting) return confirmProgress;

        return Mathf.Max(confirmProgress, GetReadyDelayProgress());
    }

    private float GetReadyDelayProgress()
    {
        if (!isReadyWaiting) return 0f;
        if (readyTotalDuration <= 0f) return 1f;

        var normalized = 1f - (readyTimer / readyTotalDuration);
        return Mathf.Clamp01(normalized);
    }

    private float GetInitialConfirmProgress(Transform target)
    {
        var confirmTime = GetCurrentInitialConfirmTime();
        if (!useInitialTargetConfirm || confirmTime <= 0f) return 1f;
        if (!IsInitialAimStable()) return 0f;
        if (pendingInitialTarget != target) return 0f;
        if (pendingInitialTimer <= 0f) return 0f;
        return Mathf.Clamp01(pendingInitialTimer / confirmTime);
    }

    private float GetChainConfirmProgress(Transform target)
    {
        var confirmTime = GetCurrentChainConfirmTime(target);
        if (!useChainTargetConfirm || confirmTime <= 0f) return 1f;
        if (pendingChainTarget != target) return 0f;
        if (pendingChainTimer <= 0f) return 0f;
        return Mathf.Clamp01(pendingChainTimer / confirmTime);
    }

    private List<Transform> GetPierceTargets(bool isChainActive, Vector3 origin, Vector3 aimDirection, float searchRange, Transform ignoreTarget, Transform selectedTarget)
    {
        var allowLinePierce = isChainActive ? useChainLinePierce : useInitialLinePierce;
        if (!allowLinePierce) return null;
        if (targetingSystem == null) return null;
        if (targetingSystem.StrategyType != TargetingStrategyType.Line) return null;

        var targets = isChainActive
            ? GetTargetsInLineStrip(origin, aimDirection, searchRange, ignoreTarget, GetPierceLineWidth(), 0f, 0f)
            : GetInitialAnchoredPierceTargets(origin, aimDirection, searchRange, ignoreTarget, selectedTarget);
        if (targets == null || targets.Count == 0) return null;
        if (selectedTarget != null && !targets.Contains(selectedTarget))
        {
            targets.Add(selectedTarget);
        }

        DeduplicateAttackTargets(targets);
        if (targets.Count == 0) return null;

        SortTargetsAlongAim(targets, origin, aimDirection);
        return targets;
    }

    private bool TryGetInitialLineAnchorTarget(Vector3 origin, Vector3 aimDirection, float searchRange, Transform ignoreTarget, out Transform anchorTarget, out Vector3 anchorDirection)
    {
        anchorTarget = null;
        anchorDirection = aimDirection;

        if (!useInitialLineAnchor) return false;
        if (targetingSystem == null) return false;
        if (targetingSystem.StrategyType != TargetingStrategyType.Line) return false;

        var candidates = GetTargetsInLineStrip(
            origin,
            aimDirection,
            searchRange,
            ignoreTarget,
            GetInitialLineAnchorWidth(),
            0f,
            0f);
        if (candidates == null || candidates.Count == 0) return false;

        anchorTarget = GetPreferredInitialAnchorTarget(candidates, origin, aimDirection);
        if (anchorTarget == null) return false;

        var toAnchor = anchorTarget.position - origin;
        toAnchor.y = 0f;
        if (toAnchor.sqrMagnitude <= 0f) return false;

        anchorDirection = toAnchor.normalized;
        return true;
    }

    private List<Transform> GetInitialAnchoredPierceTargets(Vector3 origin, Vector3 aimDirection, float searchRange, Transform ignoreTarget, Transform anchorTarget)
    {
        if (anchorTarget == null)
        {
            return GetTargetsInLineStrip(origin, aimDirection, searchRange, ignoreTarget, GetPierceLineWidth(), 0f, 0f);
        }

        var totalRange = GetPierceTotalRange(searchRange);
        var anchorOrigin = anchorTarget.position;
        anchorOrigin.y = origin.y;
        var maxForwardDistance = Mathf.Max(0f, totalRange);
        return GetTargetsInLineStrip(
            anchorOrigin,
            aimDirection,
            maxForwardDistance,
            ignoreTarget,
            GetPierceLineWidth(),
            -Mathf.Max(0f, initialLinePierceBackPadding),
            totalRange,
            origin);
    }

    private List<Transform> GetTargetsInLineStrip(Vector3 stripOrigin, Vector3 aimDirection, float maxForwardDistance, Transform ignoreTarget, float stripWidth, float minForwardDistance, float maxRangeFromOrigin)
    {
        return GetTargetsInLineStrip(stripOrigin, aimDirection, maxForwardDistance, ignoreTarget, stripWidth, minForwardDistance, maxRangeFromOrigin, stripOrigin);
    }

    private List<Transform> GetTargetsInLineStrip(Vector3 stripOrigin, Vector3 aimDirection, float maxForwardDistance, Transform ignoreTarget, float stripWidth, float minForwardDistance, float maxRangeFromOrigin, Vector3 rangeOrigin)
    {
        if (targetingSystem == null) return null;

        aimDirection.y = 0f;
        if (aimDirection.sqrMagnitude <= 0f) return null;
        if (maxForwardDistance <= 0f) return null;

        var forwardRange = maxForwardDistance > 0f ? maxForwardDistance : targetingSystem.MaxRange;
        if (targetingSystem.LineEndPadding > 0f)
        {
            forwardRange += targetingSystem.LineEndPadding;
        }

        var maxRange = maxRangeFromOrigin > 0f ? maxRangeFromOrigin : forwardRange;
        if (targetingSystem.LineEndPadding > 0f && maxRangeFromOrigin <= 0f)
        {
            maxRange += targetingSystem.LineEndPadding;
        }

        var lineWidth = Mathf.Max(0f, stripWidth);
        var lineWidthSqr = lineWidth * lineWidth;
        var rangeSqr = maxRange * maxRange;
        var dir = aimDirection.normalized;
        var candidates = new List<Transform>();
        var result = new List<Transform>();
        targetingSystem.GetTargetsSnapshot(candidates);

        for (int i = 0; i < candidates.Count; i++)
        {
            var candidate = candidates[i];
            if (candidate == null || candidate == ignoreTarget) continue;

            var rangeDiff = candidate.position - rangeOrigin;
            rangeDiff.y = 0f;

            var sqr = rangeDiff.sqrMagnitude;
            if (sqr > rangeSqr) continue;

            var stripDiff = candidate.position - stripOrigin;
            stripDiff.y = 0f;
            if (stripDiff.sqrMagnitude <= CoincidentTargetSqrThreshold) continue;

            var dot = Vector3.Dot(dir, stripDiff);
            if (dot < minForwardDistance || dot > forwardRange) continue;

            var perpSqr = stripDiff.sqrMagnitude - dot * dot;
            if (perpSqr > lineWidthSqr) continue;

            result.Add(candidate);
        }

        return result;
    }

    private Transform GetPreferredInitialAnchorTarget(List<Transform> candidates, Vector3 origin, Vector3 aimDirection)
    {
        if (candidates == null || candidates.Count == 0) return null;

        aimDirection.y = 0f;
        if (aimDirection.sqrMagnitude <= 0f) return null;

        var dir = aimDirection.normalized;
        Transform best = null;
        var bestScore = float.MaxValue;
        var bestDot = float.MaxValue;
        var bestPerpSqr = float.MaxValue;

        for (int i = 0; i < candidates.Count; i++)
        {
            var candidate = candidates[i];
            if (candidate == null) continue;

            var diff = candidate.position - origin;
            diff.y = 0f;

            var dot = Vector3.Dot(dir, diff);
            if (dot < 0f) continue;

            var perpSqr = Mathf.Max(0f, diff.sqrMagnitude - (dot * dot));
            var score = dot + (perpSqr * Mathf.Max(0f, initialLineAnchorCenterBias));
            var isBetterScore = score < bestScore - 0.001f;
            var isSameScore = Mathf.Abs(score - bestScore) <= 0.001f;
            if (isBetterScore || (isSameScore && perpSqr < bestPerpSqr - 0.001f) || (isSameScore && Mathf.Abs(perpSqr - bestPerpSqr) <= 0.001f && dot < bestDot))
            {
                best = candidate;
                bestScore = score;
                bestDot = dot;
                bestPerpSqr = perpSqr;
            }
        }

        return best;
    }

    private float GetInitialLineAnchorWidth()
    {
        if (targetingSystem == null) return 0f;
        return Mathf.Max(0f, targetingSystem.LineWidth + Mathf.Max(0f, initialLineAnchorWidthPadding));
    }

    private float GetPierceLineWidth()
    {
        if (targetingSystem == null) return 0f;

        var baseWidth = Mathf.Max(0f, targetingSystem.LineWidth);
        var multiplier = Mathf.Max(1f, linePierceWidthMultiplier);
        return (baseWidth * multiplier) + Mathf.Max(0f, linePierceWidthPadding);
    }

    private float GetPierceTotalRange(float searchRange)
    {
        var baseRange = searchRange > 0f ? searchRange : (targetingSystem != null ? targetingSystem.MaxRange : 0f);
        return Mathf.Max(0f, baseRange + Mathf.Max(0f, initialLinePierceRangeBonus));
    }

    private Transform GetPierceDashTarget(List<Transform> pierceTargets, Vector3 origin, Vector3 aimDirection, Transform fallbackTarget)
    {
        if (pierceTargets == null || pierceTargets.Count == 0) return fallbackTarget;

        aimDirection.y = 0f;
        if (aimDirection.sqrMagnitude <= 0f) return fallbackTarget;

        var dir = aimDirection.normalized;
        Transform best = fallbackTarget;
        var bestDistanceSqr = float.MinValue;
        var bestDot = float.MinValue;
        var bestPerpSqr = float.MaxValue;

        for (int i = 0; i < pierceTargets.Count; i++)
        {
            var candidate = pierceTargets[i];
            if (candidate == null) continue;

            var diff = candidate.position - origin;
            diff.y = 0f;
            var distanceSqr = diff.sqrMagnitude;
            var dot = Vector3.Dot(dir, diff);
            var perpSqr = Mathf.Max(0f, distanceSqr - (dot * dot));

            var isFarther = distanceSqr > bestDistanceSqr + 0.01f;
            var isSameDistance = Mathf.Abs(distanceSqr - bestDistanceSqr) <= 0.01f;
            var isBetterDot = dot > bestDot + 0.01f;
            var isSameDot = Mathf.Abs(dot - bestDot) <= 0.01f;
            if (isFarther || (isSameDistance && isBetterDot) || (isSameDistance && isSameDot && perpSqr < bestPerpSqr))
            {
                bestDistanceSqr = distanceSqr;
                bestDot = dot;
                bestPerpSqr = perpSqr;
                best = candidate;
            }
        }

        return best;
    }

    private bool TryGetPierceDashEndPoint(List<Transform> pierceTargets, Vector3 origin, Vector3 aimDirection, Transform fallbackTarget, out Vector3 dashEndPoint)
    {
        dashEndPoint = origin;

        aimDirection.y = 0f;
        if (aimDirection.sqrMagnitude <= 0f) return false;

        var dir = aimDirection.normalized;
        var bestDot = float.MinValue;

        if (pierceTargets != null)
        {
            for (int i = 0; i < pierceTargets.Count; i++)
            {
                var candidate = pierceTargets[i];
                if (candidate == null) continue;

                var diff = candidate.position - origin;
                diff.y = 0f;
                var dot = Vector3.Dot(dir, diff);
                if (dot > bestDot)
                {
                    bestDot = dot;
                }
            }
        }

        if (bestDot <= float.MinValue && fallbackTarget != null)
        {
            var fallbackDiff = fallbackTarget.position - origin;
            fallbackDiff.y = 0f;
            bestDot = Vector3.Dot(dir, fallbackDiff);
        }

        if (bestDot <= 0f) return false;

        dashEndPoint = origin + dir * (bestDot + Mathf.Max(0f, pierceDashOvershootDistance));
        return true;
    }

    private void SortTargetsAlongAim(List<Transform> targets, Vector3 origin, Vector3 aimDirection)
    {
        if (targets == null || targets.Count <= 1) return;

        aimDirection.y = 0f;
        if (aimDirection.sqrMagnitude <= 0f) return;

        var dir = aimDirection.normalized;
        targets.Sort((left, right) =>
        {
            if (left == right) return 0;
            if (left == null) return 1;
            if (right == null) return -1;

            var leftDiff = left.position - origin;
            var rightDiff = right.position - origin;
            leftDiff.y = 0f;
            rightDiff.y = 0f;

            var leftDistanceSqr = leftDiff.sqrMagnitude;
            var rightDistanceSqr = rightDiff.sqrMagnitude;
            var leftDot = Vector3.Dot(dir, leftDiff);
            var rightDot = Vector3.Dot(dir, rightDiff);
            var distanceCompare = leftDistanceSqr.CompareTo(rightDistanceSqr);
            if (distanceCompare != 0) return distanceCompare;

            var dotCompare = leftDot.CompareTo(rightDot);
            if (dotCompare != 0) return dotCompare;

            var leftPerpSqr = Mathf.Max(0f, leftDistanceSqr - (leftDot * leftDot));
            var rightPerpSqr = Mathf.Max(0f, rightDistanceSqr - (rightDot * rightDot));
            return leftPerpSqr.CompareTo(rightPerpSqr);
        });
    }

    private void DeduplicateAttackTargets(List<Transform> targets)
    {
        if (targets == null || targets.Count <= 1) return;

        for (int i = targets.Count - 1; i >= 0; i--)
        {
            var current = targets[i];
            if (current == null)
            {
                targets.RemoveAt(i);
                continue;
            }

            for (int j = 0; j < i; j++)
            {
                if (!AreSameAttackTarget(targets[j], current)) continue;
                targets.RemoveAt(i);
                break;
            }
        }
    }

    private bool AreSameAttackTarget(Transform left, Transform right)
    {
        if (left == right) return true;
        if (left == null || right == null) return false;

        var leftIdentity = GetAttackTargetIdentity(left);
        var rightIdentity = GetAttackTargetIdentity(right);
        if (leftIdentity != null && rightIdentity != null)
        {
            return leftIdentity == rightIdentity;
        }

        return false;
    }

    private Transform GetAttackTargetIdentity(Transform target)
    {
        if (target == null) return null;

        var damageable = ResolveDamageableTarget(target);
        if (damageable is Component component)
        {
            return component.transform;
        }

        return target;
    }

    private bool TryGetAimAssistDirection(bool isChainActive, Vector3 aimOrigin, Vector3 baseAimDirection, float searchRange, Transform ignoreTarget, out Vector3 adjustedDirection, out Transform assistTarget)
    {
        adjustedDirection = baseAimDirection;
        assistTarget = null;

        if (!useAimAssist) return false;
        if (aimAssistOnlyDuringChain && !isChainActive) return false;
        if (targetingSystem == null) return false;

        var target = targetingSystem.GetTarget(aimOrigin, baseAimDirection, searchRange, ignoreTarget);
        if (target == null)
        {
            target = targetingSystem.GetTargetByAngle(aimOrigin, baseAimDirection, searchRange, ignoreTarget);
        }
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
        var confirmTime = GetCurrentInitialConfirmTime();
        if (!useInitialTargetConfirm || confirmTime <= 0f) return candidate != null;
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
        if (!IsInitialAimAngleAllowed(angle))
        {
            ResetInitialTargetConfirm();
            return false;
        }

        if (ShouldUseInitialInstantConfirm() && angle <= GetCurrentInitialInstantAngle())
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
        if (pendingInitialTimer >= confirmTime)
        {
            confirmedTarget = pendingInitialTarget;
            confirmedDirection = pendingInitialDirection.sqrMagnitude > 0f ? pendingInitialDirection.normalized : candidateDirection.normalized;
            return true;
        }

        return false;
    }

    private bool TryGetPostChainGraceTarget(Vector3 rawAimDirection, float searchRange, Transform ignoreTarget, out Transform target, out Vector3 aimDirection)
    {
        target = null;
        aimDirection = rawAimDirection;

        if (IsChainActive()) return false;
        if (!HasPostChainAttackGrace()) return false;
        if (!TryGetPostChainFallbackOrigin(out var fallbackOrigin)) return false;
        if (targetingSystem == null) return false;

        Transform assistTarget = null;
        if (TryGetAimAssistDirection(true, fallbackOrigin, rawAimDirection, searchRange, ignoreTarget, out _, out var resolvedAssistTarget))
        {
            assistTarget = resolvedAssistTarget;
        }

        target = assistTarget ?? targetingSystem.GetTarget(fallbackOrigin, rawAimDirection, searchRange, ignoreTarget);
        if (target == null)
        {
            if (!TryGetInitialLineAnchorTarget(fallbackOrigin, rawAimDirection, searchRange, ignoreTarget, out var anchorTarget, out _))
            {
                return false;
            }

            target = anchorTarget;
        }

        if (target == null) return false;

        var toTarget = target.position - transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude <= CoincidentTargetSqrThreshold)
        {
            return false;
        }

        var maxRange = searchRange > 0f ? searchRange : (targetingSystem != null ? targetingSystem.MaxRange : 0f);
        if (targetingSystem != null && targetingSystem.LineEndPadding > 0f)
        {
            maxRange += targetingSystem.LineEndPadding;
        }

        if (maxRange > 0f && toTarget.sqrMagnitude > maxRange * maxRange)
        {
            return false;
        }

        aimDirection = toTarget.normalized;
        return true;
    }

    private bool TryGetCloseRangeFallbackTarget(Vector3 origin, Vector3 rawAimDirection, float searchRange, Transform ignoreTarget, out Transform target, out Vector3 aimDirection)
    {
        target = null;
        aimDirection = rawAimDirection;

        if (IsChainActive()) return false;
        if (targetingSystem == null) return false;
        if (targetingSystem.StrategyType != TargetingStrategyType.Line) return false;

        rawAimDirection.y = 0f;
        if (rawAimDirection.sqrMagnitude <= 0f) return false;

        var maxRange = searchRange > 0f ? searchRange : targetingSystem.MaxRange;
        if (maxRange <= 0f) return false;

        var normalizedAim = rawAimDirection.normalized;
        var probeDistance = Mathf.Min(maxRange, Mathf.Max(0.6f, targetingSystem.LineWidth));
        var probeRadius = Mathf.Max(0.9f, targetingSystem.LineWidth + 0.35f);
        var probePoint = origin + normalizedAim * probeDistance;

        target = targetingSystem.GetTargetNearPoint(probePoint, probeRadius, ignoreTarget);
        if (target == null)
        {
            var closeRange = Mathf.Min(maxRange, probeDistance + probeRadius);
            target = targetingSystem.GetTargetByAngle(origin, normalizedAim, closeRange, ignoreTarget);
        }

        if (target == null) return false;

        var toTarget = target.position - origin;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude <= CoincidentTargetSqrThreshold) return false;
        if (toTarget.sqrMagnitude > maxRange * maxRange) return false;

        var angleLimit = Mathf.Max(18f, initialAimMaxAngle, aimAssistAngle);
        if (angleLimit > 0f && Vector3.Angle(normalizedAim, toTarget) > angleLimit)
        {
            return false;
        }

        aimDirection = toTarget.normalized;
        return true;
    }

    private bool IsInitialAimAngleAllowed(float angle)
    {
        if (!useInitialAimAngleLimit) return true;
        if (initialAimMaxAngle <= 0f) return true;
        return angle <= initialAimMaxAngle;
    }

    private void UpdateInitialAimStability(Vector3 rawDirection, float deltaTime, bool isChainActive)
    {
        if (isChainActive)
        {
            hadChainAimState = true;
            ResetInitialAimStability();
            return;
        }
        var stableTime = GetCurrentInitialAimStableTime();
        if (!useInitialAimStability || stableTime <= 0f)
        {
            hadChainAimState = false;
            initialAimStableTimer = stableTime;
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
        if (hadChainAimState)
        {
            hadChainAimState = false;
            hasInitialAimDirection = true;
            lastInitialAimDirection = normalized;
            initialAimStableTimer = stableTime;
            return;
        }

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
        var stableTime = GetCurrentInitialAimStableTime();
        if (!useInitialAimStability || stableTime <= 0f) return true;
        return initialAimStableTimer >= stableTime;
    }

    private float GetCurrentInitialConfirmTime()
    {
        if (HasPostChainAttackGrace()) return 0f;
        if (!useAdaptiveInitialResponse) return initialTargetConfirmTime;

        var multiplier = Mathf.Clamp(initialConfirmTimeMultiplier, 0.1f, 1f);
        multiplier = Mathf.Max(0.65f, multiplier);
        return initialTargetConfirmTime * multiplier;
    }

    private float GetCurrentInitialAimStableTime()
    {
        if (HasPostChainAttackGrace()) return 0f;
        if (!useAdaptiveInitialResponse) return initialAimStableTime;

        var multiplier = Mathf.Clamp(initialAimStableTimeMultiplier, 0.1f, 1f);
        multiplier = Mathf.Max(0.65f, multiplier);
        return initialAimStableTime * multiplier;
    }

    private bool ShouldUseInitialInstantConfirm()
    {
        if (HasPostChainAttackGrace()) return true;
        return useInitialInstantConfirm;
    }

    private float GetCurrentInitialInstantAngle()
    {
        if (!useAdaptiveInitialResponse) return initialTargetInstantAngle;
        return initialTargetInstantAngle + Mathf.Max(0f, initialInstantAngleBonus);
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
