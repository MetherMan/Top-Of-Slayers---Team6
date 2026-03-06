using UnityEngine;

public partial class AutoSlashController
{
    private void Update()
    {
        if (dashController == null || targetingSystem == null) return;
        if (Mathf.Approximately(Time.timeScale, 0f)) return;

        var isChainActive = IsChainActive();
        var delta = isChainActive ? Time.unscaledDeltaTime : Time.deltaTime;

        if (!isChainActive)
        {
            ResetChainTargetConfirm();
            ResetSameTargetRelease();
        }
        else
        {
            ResetInitialTargetConfirm();
        }

        if (isReadyWaiting)
        {
            var readyOrigin = GetAimOrigin(isChainActive);
            var readyDirection = pendingAttack.AimDirection;
            if (readyDirection.sqrMagnitude <= 0f)
            {
                readyDirection = transform.forward;
            }

            UpdateAimPreview(readyOrigin, readyDirection);
            UpdateReadyDelay(delta);
            return;
        }

        var rawAimDirection = GetAimDirection();
        rawAimDirection.y = 0f;
        if (rawAimDirection.sqrMagnitude <= 0f)
        {
            rawAimDirection = transform.forward;
        }
        else
        {
            rawAimDirection = rawAimDirection.normalized;
        }

        UpdateInitialAimStability(rawAimDirection, delta, isChainActive);
        UpdateSameTargetRelease(rawAimDirection);

        var searchRange = GetAttackRange();
        if (isChainActive && useChainRangeBoost)
        {
            searchRange *= chainRangeMultiplier;
        }
        lastAttackRange = searchRange;

        var aimOrigin = GetAimOrigin(isChainActive);
        var ignoreTarget = GetIgnoreTarget(isChainActive, rawAimDirection);
        var previewDirection = rawAimDirection;
        if (TryGetAimAssistDirection(isChainActive, aimOrigin, rawAimDirection, searchRange, ignoreTarget, out var previewAdjustedDirection, out _))
        {
            previewDirection = previewAdjustedDirection;
        }
        UpdateAimPreview(aimOrigin, previewDirection);

        if (isChainActive && requireInputDuringChain && moveController != null)
        {
            if (!moveController.HasAimInput(chainInputDeadZone))
            {
                ResetChainTargetConfirm();
                ResetSameTargetRelease();
                return;
            }
        }

        if (!isChainActive && cooldownTimer > 0f)
        {
            cooldownTimer -= delta;
        }

        if (!isChainActive && detectInterval > 0f)
        {
            detectTimer -= delta;
            if (detectTimer > 0f) return;
            detectTimer = detectInterval;
        }

        if (!isChainActive && cooldownTimer > 0f) return;
        if (dashController.IsDashing) return;

        var aimDirection = GetStableAimDirection(isChainActive, delta);
        if (isChainActive && useChainAimConfirm && blockAttackWhileAimChanging)
        {
            var angle = Vector3.Angle(rawAimDirection, aimDirection);
            if (angle > blockAttackAngle)
            {
                return;
            }
        }

        Transform target = null;
        if (isChainActive)
        {
            if (!TryGetChainPriorityTarget(aimOrigin, rawAimDirection, searchRange, ignoreTarget, out var chainTarget, out var chainDirection))
            {
                return;
            }

            if (!TryConfirmChainTarget(chainTarget, chainDirection, rawAimDirection, delta, out target, out aimDirection))
            {
                return;
            }
        }

        if (target == null)
        {
            Transform assistTarget = null;
            if (TryGetAimAssistDirection(isChainActive, aimOrigin, aimDirection, searchRange, ignoreTarget, out var adjustedDirection, out var resolvedAssistTarget))
            {
                aimDirection = adjustedDirection;
                assistTarget = resolvedAssistTarget;
            }

            target = assistTarget ?? targetingSystem.GetTarget(aimOrigin, aimDirection, searchRange, ignoreTarget);
        }
        if (target == null)
        {
            if (!isChainActive)
            {
                ResetInitialTargetConfirm();
            }
            return;
        }

        if (!isChainActive)
        {
            if (!TryConfirmInitialTarget(target, aimDirection, rawAimDirection, delta, out var confirmedTarget, out var confirmedDirection))
            {
                return;
            }

            target = confirmedTarget;
            aimDirection = confirmedDirection;
            ResetInitialTargetConfirm();
        }

        if (isChainActive && useSameTargetRelease && !sameTargetReleased && target == lastAttackTarget)
        {
            return;
        }

        var aimDistance = searchRange > 0f ? searchRange : 0f;
        var damageMultiplier = chainCombat != null ? chainCombat.GetDamageMultiplier(target) : 1f;
        var pierceTargets = GetPierceTargets(isChainActive, aimOrigin, aimDirection, searchRange, ignoreTarget, target);
        var usePierce = pierceTargets != null && pierceTargets.Count > 1;
        var attack = new PendingAttack(target, aimDirection, aimDistance, autoGrade, damageMultiplier, rawAimDirection, pierceTargets, usePierce);

        if (ShouldUseReadyDelay(isChainActive))
        {
            BeginReadyDelay(attack);
            return;
        }

        TryStartAttack(attack);
    }
}
