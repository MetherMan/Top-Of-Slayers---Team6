using UnityEngine;

public partial class SlashDashController
{
    private void TryTriggerContactStop(Vector3 previousPosition, Vector3 currentPosition)
    {
        if (contactStopTriggered) return;
        if (pendingTarget == null) return;

        var targetPosition = pendingTarget.position;
        if (contactDistance > 0f && IsSegmentClose(previousPosition, currentPosition, targetPosition, contactDistance))
        {
            contactStopTriggered = true;
            TriggerImpactOnce();
            if (hitSequence != null)
            {
                hitSequence.TriggerHitStop();
            }
        }
    }

    private bool IsSegmentClose(Vector3 start, Vector3 end, Vector3 target, float radius)
    {
        start.y = 0f;
        end.y = 0f;
        target.y = 0f;

        var segment = end - start;
        var lengthSqr = segment.sqrMagnitude;
        if (lengthSqr <= 0f)
        {
            return (target - start).sqrMagnitude <= radius * radius;
        }

        var t = Mathf.Clamp01(Vector3.Dot(target - start, segment) / lengthSqr);
        var closest = start + segment * t;
        return (target - closest).sqrMagnitude <= radius * radius;
    }

    private void TriggerImpactOnce()
    {
        if (impactTriggered) return;
        impactTriggered = true;
        var impactTarget = pendingTarget;
        if (impactTarget == null && pendingPierceTargets.Count > 0)
        {
            impactTarget = pendingPierceTargets[0];
        }
        OnDashImpactTarget?.Invoke(impactTarget);
        OnDashImpact?.Invoke();
    }
}
