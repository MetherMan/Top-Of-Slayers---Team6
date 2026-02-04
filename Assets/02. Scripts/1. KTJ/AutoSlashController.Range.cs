using UnityEngine;

public partial class AutoSlashController
{
    private float GetCooldown()
    {
        if (useSpecCooldown && spec != null)
        {
            return Mathf.Max(0f, spec.cooldown);
        }

        return Mathf.Max(0f, manualCooldown);
    }

    private float GetAttackRange()
    {
        if (manualRange > 0f)
        {
            return manualRange;
        }

        if (useTargetingRange && targetingSystem != null)
        {
            return Mathf.Max(0f, targetingSystem.MaxRange);
        }

        if (dashController != null)
        {
            var distance = dashController.DefaultDashDistance;
            if (distance > 0f) return distance;
        }

        if (spec != null)
        {
            return Mathf.Max(0f, spec.dashSpeed * spec.dashDuration);
        }

        if (targetingSystem != null)
        {
            return Mathf.Max(0f, targetingSystem.MaxRange);
        }

        return 0f;
    }

    public float AttackRange => lastAttackRange > 0f ? lastAttackRange : GetAttackRange();
}
