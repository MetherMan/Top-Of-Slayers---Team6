using System.Collections.Generic;
using UnityEngine;

public partial class AutoSlashController
{
    [Header("공격 준비")]
    [SerializeField] private bool useReadyDelay = true;
    [SerializeField, Min(0f)] private float readyDelay = 0.08f;
    [SerializeField] private bool readyDelayOnlyWhenNotChain = true;

    private bool isReadyWaiting;
    private float readyTimer;
    private float readyTotalDuration;
    private PendingAttack pendingAttack;
    private bool readyLockApplied;
    private bool readyRotationLockApplied;

    public bool IsReadyWaiting => isReadyWaiting;

    private bool ShouldUseReadyDelay(bool isChainActive)
    {
        if (!useReadyDelay) return false;
        if (readyDelay <= 0f) return false;
        if (!isChainActive && HasPostChainAttackGrace()) return false;
        if (readyDelayOnlyWhenNotChain && isChainActive) return false;
        return true;
    }

    private void BeginReadyDelay(PendingAttack attack, bool isChainActive)
    {
        pendingAttack = attack;
        readyTotalDuration = Mathf.Max(0f, GetCurrentReadyDelay(isChainActive));
        readyTimer = readyTotalDuration;
        isReadyWaiting = readyTimer > 0f;
        OnAttackReady?.Invoke();
        if (moveController != null)
        {
            moveController.ForceLookDirection(attack.AimDirection);
        }
        if (isReadyWaiting)
        {
            ApplyReadyMovementLock(true);
            ApplyReadyRotationLock(true);
        }
        if (!isReadyWaiting)
        {
            StartPendingAttack();
        }
    }

    private void UpdateReadyDelay(float delta)
    {
        if (!isReadyWaiting) return;
        if (dashController == null)
        {
            ClearReadyDelay();
            return;
        }
        if (!IsAttackableTarget(pendingAttack.Target))
        {
            ClearReadyDelay();
            return;
        }

        readyTimer -= delta;
        if (readyTimer > 0f) return;
        StartPendingAttack();
    }

    private void StartPendingAttack()
    {
        var attack = pendingAttack;
        ClearReadyDelay();
        TryStartAttack(attack);
    }

    private void ClearReadyDelay()
    {
        isReadyWaiting = false;
        readyTimer = 0f;
        readyTotalDuration = 0f;
        pendingAttack = default;
        ApplyReadyMovementLock(false);
        ApplyReadyRotationLock(false);
    }

    private float GetCurrentReadyDelay(bool isChainActive)
    {
        if (isChainActive) return readyDelay;
        if (!useAdaptiveInitialResponse) return readyDelay;

        var multiplier = Mathf.Clamp(initialReadyDelayMultiplier, 0.1f, 1f);
        multiplier = Mathf.Max(0.5f, multiplier);
        return readyDelay * multiplier;
    }

    private void TryStartAttack(PendingAttack attack)
    {
        if (dashController == null) return;
        if (!IsAttackableTarget(attack.Target)) return;
        if (!CanStartAttackByCost()) return;

        var usePierce = attack.UsePierce && attack.PierceTargets != null && attack.PierceTargets.Count > 1;
        if (usePierce)
        {
            if (dashController.TryStartAutoSlashPierce(attack.Target, attack.AimDirection, attack.AimDistance, attack.Grade, attack.DamageMultiplier, attack.PierceTargets, attack.DashEndPoint, attack.UseDashEndPoint))
            {
                ConsumeAttackCost();
                RegisterAttackTarget(attack.Target, attack.AimDirection);
                cooldownTimer = IsChainActive() ? 0f : GetCooldown();
            }
            return;
        }

        if (dashController.TryStartAutoSlash(attack.Target, attack.AimDirection, attack.AimDistance, attack.Grade, attack.DamageMultiplier))
        {
            ConsumeAttackCost();
            RegisterAttackTarget(attack.Target, attack.AimDirection);
            cooldownTimer = IsChainActive() ? 0f : GetCooldown();
        }
    }

    private void ApplyReadyMovementLock(bool locked)
    {
        if (moveController == null) return;
        if (locked)
        {
            if (readyLockApplied) return;
            readyLockApplied = true;
            moveController.AddMovementLock();
            return;
        }

        if (!readyLockApplied) return;
        readyLockApplied = false;
        moveController.RemoveMovementLock();
    }

    private void ApplyReadyRotationLock(bool locked)
    {
        if (moveController == null) return;
        if (locked)
        {
            if (readyRotationLockApplied) return;
            readyRotationLockApplied = true;
            moveController.AddRotationLock();
            return;
        }

        if (!readyRotationLockApplied) return;
        readyRotationLockApplied = false;
        moveController.RemoveRotationLock();
    }

    private struct PendingAttack
    {
        public readonly Transform Target;
        public readonly Vector3 AimDirection;
        public readonly float AimDistance;
        public readonly TimingGrade Grade;
        public readonly float DamageMultiplier;
        public readonly Vector3 RawAimDirection;
        public readonly List<Transform> PierceTargets;
        public readonly bool UsePierce;
        public readonly Vector3 DashEndPoint;
        public readonly bool UseDashEndPoint;

        public PendingAttack(Transform target, Vector3 aimDirection, float aimDistance, TimingGrade grade, float damageMultiplier, Vector3 rawAimDirection, List<Transform> pierceTargets, bool usePierce, Vector3 dashEndPoint, bool useDashEndPoint)
        {
            Target = target;
            AimDirection = aimDirection;
            AimDistance = aimDistance;
            Grade = grade;
            DamageMultiplier = damageMultiplier;
            RawAimDirection = rawAimDirection;
            PierceTargets = pierceTargets;
            UsePierce = usePierce;
            DashEndPoint = dashEndPoint;
            UseDashEndPoint = useDashEndPoint;
        }
    }
}
