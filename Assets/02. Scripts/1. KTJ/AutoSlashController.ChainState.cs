using UnityEngine;

public partial class AutoSlashController
{
    private Transform lastAttackTarget;
    private bool sameTargetReleased = true;
    private Vector3 lastAttackAimDirection = Vector3.forward;
    private bool hasLastAttackAim;
    private Vector2 lastAttackInput;
    private bool hasLastAttackInput;
    private float sameTargetReleaseTimer;

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
    }

    private void ResetSameTargetRelease()
    {
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
            }
            return;
        }
        sameTargetReleaseTimer = 0f;
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
}
