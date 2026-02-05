using UnityEngine;

public partial class AutoSlashController
{
    private void Update()
    {
        if (dashController == null || targetingSystem == null) return;
        if (Mathf.Approximately(Time.timeScale, 0f)) return;

        var isChainActive = chainCombat != null && chainCombat.IsSlowActive;
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
        var previewIgnoreTarget = GetIgnoreTarget(isChainActive, rawAimDirection);
        var previewDirection = rawAimDirection;
        if (TryGetAimAssistDirection(isChainActive, aimOrigin, rawAimDirection, searchRange, previewIgnoreTarget, out var previewAdjustedDirection, out _))
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

        if (isReadyWaiting)
        {
            UpdateReadyDelay(delta);
            return;
        }

        if (!isChainActive && cooldownTimer > 0f) cooldownTimer -= delta;

        if (!isChainActive && detectInterval > 0f)
        {
            detectTimer -= delta;
            if (detectTimer > 0f) return;
            detectTimer = detectInterval;
        }

        if (!isChainActive && cooldownTimer > 0f) return;
        if (dashController.IsDashing) return;

        var baseAimDirection = GetStableAimDirection(isChainActive, delta);
        if (isChainActive && useChainAimConfirm && blockAttackWhileAimChanging)
        {
            var angle = Vector3.Angle(rawAimDirection, baseAimDirection);
            if (angle > blockAttackAngle)
            {
                return;
            }
        }

        var ignoreTarget = GetIgnoreTarget(isChainActive, rawAimDirection);
        var aimDirection = baseAimDirection;
        Transform target = null;

        if (isChainActive && targetingSystem != null)
        {
            if (TryGetChainPriorityTarget(aimOrigin, rawAimDirection, searchRange, ignoreTarget, out var chainTarget, out var chainDirection))
            {
                if (!TryConfirmChainTarget(chainTarget, chainDirection, rawAimDirection, delta, out var confirmedTarget, out var confirmedDirection))
                {
                    return;
                }
                target = confirmedTarget;
                aimDirection = confirmedDirection;
            }
            else
            {
                return;
            }
        }

        if (target == null)
        {
            Transform assistTarget = null;
            if (TryGetAimAssistDirection(isChainActive, aimOrigin, baseAimDirection, searchRange, ignoreTarget, out var adjustedDirection, out var resolvedAssistTarget))
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

        if (pierceTargets != null && pierceTargets.Count > 1)
        {
            if (ShouldUseReadyDelay(isChainActive))
            {
                BeginReadyDelay(new PendingAttack(target, aimDirection, aimDistance, autoGrade, damageMultiplier, rawAimDirection, pierceTargets, true));
                return;
            }

            OnAttackReady?.Invoke();
            if (dashController.TryStartAutoSlashPierce(target, aimDirection, aimDistance, autoGrade, damageMultiplier, pierceTargets))
            {
                RegisterAttackTarget(target, rawAimDirection);
                cooldownTimer = GetCooldown();
            }
            return;
        }

        if (ShouldUseReadyDelay(isChainActive))
        {
            BeginReadyDelay(new PendingAttack(target, aimDirection, aimDistance, autoGrade, damageMultiplier, rawAimDirection, null, false));
            return;
        }

        OnAttackReady?.Invoke();
        if (dashController.TryStartAutoSlash(target, aimDirection, aimDistance, autoGrade, damageMultiplier))
        {
            RegisterAttackTarget(target, rawAimDirection);
            cooldownTimer = GetCooldown();
        }
    }
}
