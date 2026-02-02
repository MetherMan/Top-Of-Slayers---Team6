using UnityEngine;
public class SlashDashController : MonoBehaviour
{
    [Header("연동")]
    [SerializeField] private AttackInputController input;
    [SerializeField] private TargetingSystem targetingSystem;
    [SerializeField] private DamageSystem damageSystem;
    [SerializeField] private AttackSpecSO spec;
    [SerializeField] private PlayerMoveController moveController;
    [SerializeField] private HitSequenceController hitSequence;
    [Header("대시")]
    [SerializeField] private float defaultDashSpeed = 10f;
    [SerializeField] private float defaultDashDuration = 0.2f;
    [SerializeField] private bool useFixedDashTime = true;
    [SerializeField, Min(0.01f)] private float fixedDashTime = 0.1f;
    [SerializeField] private float overshootDistance = 0.5f;
    [SerializeField] private bool ignorePhysicsDuringDash = true;
    [SerializeField] private bool lockMovementDuringDash = true;
    [Header("연출")]
    [SerializeField, Min(0f)] private float contactDistance = 0.3f;
    private DashState state = DashState.Idle;
    private float dashSpeed;
    private float dashTimer;
    private float dashRemainingDistance;
    private Vector3 dashDirection;
    private Transform pendingTarget;
    private int pendingDamage;
    private bool contactStopTriggered;
    private Rigidbody cachedRigidbody;
    private bool cachedKinematic;
    private bool cachedUseGravity;
    private void Awake()
    {
        cachedRigidbody = GetComponent<Rigidbody>();
        if (hitSequence == null) hitSequence = GetComponent<HitSequenceController>();
    }
    private void OnEnable()
    {
        if (input != null) input.OnAttackTriggered += HandleAttackTriggered;
    }
    private void OnDisable()
    {
        if (input != null) input.OnAttackTriggered -= HandleAttackTriggered;
    }
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
            state = DashState.Idle;
            ApplyPendingDamage();
            RestorePhysics();
            SetMovementLock(false);
        }
    }
    private void HandleAttackTriggered(TimingGrade grade)
    {
        var aimDirection = input != null ? input.AimDirection : transform.forward;
        var aimDistance = input != null ? input.AimDistance : 0f;
        var searchRange = aimDistance;
        if (searchRange <= 0f && spec != null)
        {
            searchRange = Mathf.Max(0f, spec.dashSpeed * spec.dashDuration);
        }
        var target = targetingSystem != null ? targetingSystem.GetTarget(transform.position, aimDirection, searchRange) : null;
        if (target != null)
        {
            var toTarget = target.position - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude > 0f)
            {
                aimDirection = toTarget.normalized;
                aimDistance = toTarget.magnitude;
            }
        }
        if (!TryStartDash(target, aimDirection, aimDistance, spec)) return;
        if (grade != TimingGrade.Miss && target != null && spec != null)
        {
            pendingTarget = target;
            pendingDamage = CalculateDamage(grade, spec);
        }
    }
    private int CalculateDamage(TimingGrade grade, AttackSpecSO spec)
    {
        if (spec == null) return 0;
        switch (grade)
        {
            case TimingGrade.Perfect:
                return Mathf.RoundToInt(spec.baseDamage * spec.criticalMultiplier);
            case TimingGrade.Good:
                return spec.baseDamage;
            default:
                return 0;
        }
    }
    private void ApplyPendingDamage()
    {
        if (damageSystem == null || pendingTarget == null || pendingDamage <= 0)
        {
            pendingTarget = null;
            pendingDamage = 0;
            return;
        }
        damageSystem.ApplyDamage(pendingTarget, pendingDamage);
        pendingTarget = null; pendingDamage = 0;
        contactStopTriggered = false;
    }
    private void TryTriggerContactStop(Vector3 previousPosition, Vector3 currentPosition)
    {
        if (contactStopTriggered) return;
        if (hitSequence == null) return;
        if (pendingTarget == null || pendingDamage <= 0) return;

        var targetPosition = pendingTarget.position;
        if (contactDistance > 0f && IsSegmentClose(previousPosition, currentPosition, targetPosition, contactDistance))
        {
            contactStopTriggered = true;
            hitSequence.TriggerHitStop();
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
        if (aimDistance > 0f)
        {
            dashRemainingDistance = aimDistance;
        }
        if (target != null)
        {
            var toTarget = target.position - transform.position;
            toTarget.y = 0f;
            var distance = toTarget.magnitude;
            if (distance > 0f)
            {
                dashDirection = toTarget.normalized;
                dashRemainingDistance = distance + Mathf.Max(0f, overshootDistance);
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
        state = DashState.Dashing;
        PreparePhysics();
        SetMovementLock(true);
        return true;
    }
    public void ForceStop()
    {
        state = DashState.Idle;
        dashTimer = 0f;
        dashRemainingDistance = 0f;
        pendingTarget = null;
        pendingDamage = 0;
        contactStopTriggered = false;
        RestorePhysics();
        SetMovementLock(false);
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
        if (!lockMovementDuringDash) return;
        if (moveController == null) return;
        moveController.SetMovementLocked(locked);
    }
    private enum DashState
    {
        Idle,
        Dashing
    }
}
