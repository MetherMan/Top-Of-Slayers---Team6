using UnityEngine;

public partial class SlashDashController
{
    public bool TryStartDash(Transform target, Vector3 aimDirection, float aimDistance, AttackSpecSO spec)
    {
        if (state != DashState.Idle) return false;

        aimDirection.y = 0f;
        if (aimDirection.sqrMagnitude <= 0f) return false;

        dashDirection = aimDirection.normalized;
        dashSpeed = spec != null ? spec.dashSpeed : defaultDashSpeed;
        var dashDuration = spec != null ? spec.dashDuration : defaultDashDuration;
        if (dashSpeed <= 0f && !useFixedDashTime) return false;

        dashRemainingDistance = dashSpeed * dashDuration;
        pendingTarget = null;
        pendingDamage = 0;
        contactStopTriggered = false;
        impactTriggered = false;
        pendingPierceTargets.Clear();

        if (aimDistance > 0f)
        {
            dashRemainingDistance = aimDistance;
        }

        if (target != null)
        {
            var endPosition = GetBehindPosition(target);
            var toEnd = endPosition - transform.position;
            toEnd.y = 0f;
            if (toEnd.sqrMagnitude > 0f)
            {
                dashDirection = toEnd.normalized;
                dashRemainingDistance = toEnd.magnitude;
            }
        }

        if (dashRemainingDistance <= 0f) return false;

        if (useFixedDashTime)
        {
            dashTimer = fixedDashTime;
            if (dashTimer <= 0f) return false;
            dashSpeed = dashRemainingDistance / dashTimer;
        }
        else
        {
            dashTimer = dashRemainingDistance / dashSpeed;
            if (dashTimer <= 0f) return false;
        }

        dashTotalTime = dashTimer;
        dashTotalDistance = dashRemainingDistance;

        state = DashState.Dashing;
        PreparePhysics();
        SetMovementLock(true);
        OnDashStarted?.Invoke();
        return true;
    }

    public void ForceStop()
    {
        state = DashState.Idle;
        dashTimer = 0f;
        dashTotalTime = 0f;
        dashRemainingDistance = 0f;
        dashTotalDistance = 0f;
        pendingTarget = null;
        pendingDamage = 0;
        contactStopTriggered = false;
        impactTriggered = false;
        RestorePhysics();
        SetMovementLock(false);
        SyncMoveRotation();
    }

    private void Move(Vector3 move)
    {
        if (ignorePhysicsDuringDash)
        {
            transform.position += move;
            return;
        }

        if (cachedRigidbody != null)
        {
            cachedRigidbody.MovePosition(cachedRigidbody.position + move);
        }
        else
        {
            transform.position += move;
        }
    }

    private void PreparePhysics()
    {
        if (!ignorePhysicsDuringDash || cachedRigidbody == null) return;

        cachedKinematic = cachedRigidbody.isKinematic;
        cachedUseGravity = cachedRigidbody.useGravity;
        cachedRigidbody.isKinematic = true;
        cachedRigidbody.useGravity = false;
    }

    private void RestorePhysics()
    {
        if (!ignorePhysicsDuringDash || cachedRigidbody == null) return;

        cachedRigidbody.isKinematic = cachedKinematic;
        cachedRigidbody.useGravity = cachedUseGravity;
    }

    private void SetMovementLock(bool locked)
    {
        if (moveController == null) return;

        if (lockMovementDuringDash)
        {
            moveController.SetMovementLocked(locked);
        }

        if (locked)
        {
            if (dashRotationLockApplied) return;
            dashRotationLockApplied = true;
            moveController.AddRotationLock();
            return;
        }

        if (!dashRotationLockApplied) return;
        dashRotationLockApplied = false;
        moveController.RemoveRotationLock();
    }

    private void SyncMoveRotation()
    {
        if (moveController == null) return;
        moveController.ForceSyncRotation();
    }

    private Vector3 GetBehindPosition(Transform target)
    {
        var offset = behindOffset;
        if (useChainBehindOffset && chainCombat != null && chainCombat.IsSlowActive && chainBehindOffset > 0f)
        {
            offset = chainBehindOffset;
        }
        offset = Mathf.Max(0f, offset);
        if (offset <= 0f) return target.position;

        Vector3 behindDirection;
        if (useTargetForwardForBehind)
        {
            behindDirection = -target.forward;
            behindDirection.y = 0f;
            if (behindDirection.sqrMagnitude <= 0f)
            {
                behindDirection = target.position - transform.position;
            }
        }
        else
        {
            behindDirection = target.position - transform.position;
        }

        behindDirection.y = 0f;
        if (behindDirection.sqrMagnitude <= 0f)
        {
            behindDirection = transform.forward;
            behindDirection.y = 0f;
        }

        if (behindDirection.sqrMagnitude <= 0f)
        {
            behindDirection = Vector3.forward;
        }

        return target.position + behindDirection.normalized * offset;
    }
}
