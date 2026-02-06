using UnityEngine;
using DG.Tweening;

public partial class SlashDashController
{
    private void Update()
    {
        if (state != DashState.Dashing) return;

        var previousPosition = transform.position;
        var delta = Time.deltaTime;
        var useEase = useDashEase && (!dashEaseOnlyDuringChain || (chainCombat != null && chainCombat.IsSlowActive));
        float step;
        if (useEase && dashTotalTime > 0f && dashTotalDistance > 0f)
        {
            var elapsed = dashTotalTime - dashTimer;
            var t = Mathf.Clamp01(elapsed / dashTotalTime);
            var tNext = Mathf.Clamp01((elapsed + delta) / dashTotalTime);
            var easedT = DOVirtual.EasedValue(0f, 1f, t, dashEase);
            var easedTNext = DOVirtual.EasedValue(0f, 1f, tNext, dashEase);
            step = (easedTNext - easedT) * dashTotalDistance;
            if (step < 0f) step = 0f;
        }
        else
        {
            step = dashSpeed * delta;
        }

        if (dashRemainingDistance > 0f && step > dashRemainingDistance) step = dashRemainingDistance;

        var move = dashDirection * step;
        Move(move);
        TryTriggerContactStop(previousPosition, transform.position);

        dashTimer -= delta;
        dashRemainingDistance -= step;
        if (dashTimer <= 0f || dashRemainingDistance <= 0f)
        {
            TriggerImpactOnce();
            state = DashState.Idle;
            ApplyPendingDamage();
            RestorePhysics();
            SetMovementLock(false);
            SyncMoveRotation();
        }
    }
}
