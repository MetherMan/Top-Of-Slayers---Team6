using UnityEngine;

public partial class SlashDashController
{
    private void Update()
    {
        if (state != DashState.Dashing) return;

        var previousPosition = transform.position;
        var step = dashSpeed * Time.deltaTime;
        if (dashRemainingDistance > 0f && step > dashRemainingDistance) step = dashRemainingDistance;

        var move = dashDirection * step;
        Move(move);
        TryTriggerContactStop(previousPosition, transform.position);

        dashTimer -= Time.deltaTime;
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
