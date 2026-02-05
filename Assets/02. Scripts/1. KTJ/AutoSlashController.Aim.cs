using UnityEngine;

public partial class AutoSlashController
{
    private Vector3 confirmedAimDirection = Vector3.forward;
    private Vector3 pendingAimDirection;
    private float pendingAimTimer;
    private bool hasConfirmedAim;
    private bool hasPendingAim;
    private Vector3 lastAimOrigin;
    private Vector3 lastAimDirection = Vector3.forward;
    private bool hasAimPreview;

    private Vector3 GetAimDirection()
    {
        if (moveController != null)
        {
            return moveController.GetAimDirection();
        }

        return transform.forward;
    }

    private void UpdateAimPreview(Vector3 aimOrigin, Vector3 aimDirection)
    {
        hasAimPreview = true;
        lastAimOrigin = aimOrigin;
        lastAimDirection = aimDirection.sqrMagnitude > 0f ? aimDirection.normalized : transform.forward;
    }

    public bool TryGetAimPreview(out Vector3 origin, out Vector3 direction)
    {
        origin = transform.position;
        direction = transform.forward;
        if (!hasAimPreview) return false;

        origin = lastAimOrigin;
        direction = lastAimDirection;
        return true;
    }

    private Vector3 GetStableAimDirection(bool isChainActive, float deltaTime)
    {
        var rawDirection = GetAimDirection();
        rawDirection.y = 0f;
        if (rawDirection.sqrMagnitude <= 0f)
        {
            return transform.forward;
        }

        var normalized = rawDirection.normalized;
        if (!useChainAimConfirm || !isChainActive || chainAimConfirmTime <= 0f)
        {
            ResetAimConfirm(normalized);
            return normalized;
        }

        if (!hasConfirmedAim)
        {
            hasConfirmedAim = true;
            confirmedAimDirection = normalized;
            return confirmedAimDirection;
        }

        var angle = Vector3.Angle(confirmedAimDirection, normalized);
        if (chainAimSnapAngle > 0f && angle >= chainAimSnapAngle)
        {
            confirmedAimDirection = normalized;
            hasPendingAim = false;
            pendingAimTimer = 0f;
            return confirmedAimDirection;
        }
        if (angle < chainAimConfirmAngle)
        {
            hasPendingAim = false;
            pendingAimTimer = 0f;
            return confirmedAimDirection;
        }

        if (!hasPendingAim || Vector3.Angle(pendingAimDirection, normalized) > chainAimConfirmAngle)
        {
            hasPendingAim = true;
            pendingAimDirection = normalized;
            pendingAimTimer = 0f;
        }

        pendingAimTimer += deltaTime;
        if (pendingAimTimer >= chainAimConfirmTime)
        {
            confirmedAimDirection = pendingAimDirection;
            hasPendingAim = false;
            pendingAimTimer = 0f;
        }

        return confirmedAimDirection;
    }

    private void ResetAimConfirm(Vector3 normalizedDirection)
    {
        confirmedAimDirection = normalizedDirection;
        hasConfirmedAim = true;
        hasPendingAim = false;
        pendingAimTimer = 0f;
    }
}
