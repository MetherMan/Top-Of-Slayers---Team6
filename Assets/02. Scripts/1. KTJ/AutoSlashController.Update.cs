using UnityEngine;

public partial class AutoSlashController
{
    private void Update()
    {
        if (dashController == null || targetingSystem == null) return;
        if (Mathf.Approximately(Time.timeScale, 0f)) return;
        if (IsPlayerDead())
        {
            CancelAttackFlowOnDeath();
            return;
        }

        var isChainActive = IsChainActive();
        var justExitedChain = wasChainActiveLastFrame && !isChainActive;
        wasChainActiveLastFrame = isChainActive;
        var delta = isChainActive ? Time.unscaledDeltaTime : Time.deltaTime;
        UpdatePostChainAttackGrace(isChainActive, justExitedChain, delta);

        if (!isChainActive)
        {
            ResetChainTargetConfirm();
            ResetSameTargetRelease();
            if (justExitedChain)
            {
                ResetPostChainAttackState();
            }
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

        CleanupInvalidAttackTargetState();
        UpdateInitialAimStability(rawAimDirection, delta, isChainActive);
        UpdateSameTargetRelease(rawAimDirection);
        UpdateSameTargetReattackIntent(rawAimDirection, delta, isChainActive);

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

        if (!HasAttackAimInput(isChainActive))
        {
            if (isChainActive)
            {
                ResetChainTargetConfirm();
                ResetSameTargetRelease();
            }
            else
            {
                ResetInitialTargetConfirm();
            }
            return;
        }

        if (isChainActive && requireInputDuringChain && moveController != null)
        {
            if (!moveController.HasAimInput(chainInputDeadZone) && !HasSameTargetReattackRequest())
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

        Transform target = null;
        if (isChainActive)
        {
            if (!TryResolveChainAttackTarget(aimOrigin, rawAimDirection, searchRange, ignoreTarget, delta, out target, out aimDirection))
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
        if (!isChainActive && TryGetInitialLineAnchorTarget(aimOrigin, aimDirection, searchRange, ignoreTarget, out var anchorTarget, out var anchorDirection))
        {
            target = anchorTarget;
            aimDirection = anchorDirection;
        }
        if (target == null)
        {
            if (!isChainActive)
            {
                ResetInitialTargetConfirm();
            }
            return;
        }

        if (!IsAttackableTarget(target))
        {
            if (isChainActive)
            {
                ResetChainTargetConfirm();
            }
            else
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

        if (isChainActive && useSameTargetRelease && !sameTargetReleased && !HasSameTargetReattackRequest() && AreSameAttackTargets(target, lastAttackTarget))
        {
            return;
        }

        var aimDistance = searchRange > 0f ? searchRange : 0f;
        var damageMultiplier = chainCombat != null ? chainCombat.GetDamageMultiplier(target) : 1f;
        var attack = new PendingAttack(target, aimDirection, aimDistance, autoGrade, damageMultiplier, rawAimDirection, null, false, target.position, false);

        if (ShouldUseReadyDelay(isChainActive))
        {
            BeginReadyDelay(attack, isChainActive);
            return;
        }

        TryStartAttack(attack);
    }

    private bool IsPlayerDead()
    {
        EnsureCombatResource();
        return combatResource != null && combatResource.IsDead;
    }

    private bool HasAttackAimInput(bool isChainActive)
    {
        if (moveController == null) return true;
        if (isChainActive && HasSameTargetReattackRequest())
        {
            return true;
        }
        if (!isChainActive && HasPostChainAttackGrace())
        {
            return moveController.HasAimInput(GetChainInputDeadZone());
        }

        var deadZone = isChainActive
            ? GetChainInputDeadZone()
            : GetInitialInputDeadZone();
        return moveController.HasAimInput(deadZone);
    }

    private float GetInitialInputDeadZone()
    {
        if (HasPostChainAttackGrace())
        {
            return GetChainInputDeadZone();
        }

        var baseDeadZone = chainInputDeadZone > 0f ? chainInputDeadZone : 0.1f;
        if (!requireStrongerInputForInitialAttack)
        {
            return baseDeadZone;
        }

        return Mathf.Max(baseDeadZone, initialInputDeadZone);
    }

    private float GetChainInputDeadZone()
    {
        return chainInputDeadZone > 0f ? chainInputDeadZone : 0.1f;
    }

    private void UpdatePostChainAttackGrace(bool isChainActive, bool justExitedChain, float delta)
    {
        if (isChainActive)
        {
            postChainAttackGraceTimer = 0f;
            return;
        }

        if (justExitedChain)
        {
            postChainAttackGraceTimer = Mathf.Max(0f, postChainAttackGraceTime);
            return;
        }

        if (postChainAttackGraceTimer <= 0f) return;
        postChainAttackGraceTimer = Mathf.Max(0f, postChainAttackGraceTimer - Mathf.Max(0f, delta));
    }

    private bool HasPostChainAttackGrace()
    {
        return postChainAttackGraceTimer > 0f;
    }

    // 체인 중 멈춰 있던 일반 공격 타이머를 비워 체인 종료 직후 첫 공격이 밀리지 않게 한다.
    private void ResetPostChainAttackState()
    {
        cooldownTimer = 0f;
        detectTimer = 0f;
        ResetSameTargetRelease();
    }

    private void CancelAttackFlowOnDeath()
    {
        ClearReadyDelay();
        ResetChainTargetConfirm();
        ResetInitialTargetConfirm();
        ResetSameTargetRelease();

        if (dashController != null && dashController.IsDashing)
        {
            dashController.ForceStop();
        }
    }
}
