using System.Collections.Generic;
using UnityEngine;

public class SlashDashController : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private TargetingSystem targetingSystem;
    [SerializeField] private DamageSystem damageSystem;
    [SerializeField] private AttackSpecSO spec;
    [SerializeField] private PlayerMoveController moveController;
    [SerializeField] private HitSequenceController hitSequence;

    [Header("이동")]
    [SerializeField] private float defaultDashSpeed = 10f;
    [SerializeField] private float defaultDashDuration = 0.2f;
    [SerializeField] private bool useFixedDashTime = true;
    [SerializeField, Min(0.01f)] private float fixedDashTime = 0.1f;
    [SerializeField] private bool ignorePhysicsDuringDash = true;
    [SerializeField] private bool lockMovementDuringDash = true;
    [SerializeField, Min(0f)] private float behindOffset = 0.5f;
    [SerializeField] private bool useTargetForwardForBehind = false;

    [Header("체인")]
    [SerializeField] private ChainCombatController chainCombat;
    [SerializeField] private bool useChainBehindOffset = true;
    [SerializeField, Min(0f)] private float chainBehindOffset = 0f;

    [Header("연출")]
    [SerializeField, Min(0f)] private float contactDistance = 0.3f;

    private DashState state = DashState.Idle;
    private float dashSpeed;
    private float dashTimer;
    private float dashRemainingDistance;
    private Vector3 dashDirection;
    private Transform pendingTarget;
    private int pendingDamage;
    private readonly List<Transform> pendingPierceTargets = new List<Transform>();
    private bool contactStopTriggered;
    private Rigidbody cachedRigidbody;
    private bool cachedKinematic;
    private bool cachedUseGravity;

    public bool IsDashing => state == DashState.Dashing;
    public AttackSpecSO Spec => spec;
    public event System.Action OnDashStarted;
    public float DefaultDashDistance
    {
        get
        {
            var dashSpeedValue = spec != null ? spec.dashSpeed : defaultDashSpeed;
            var dashDurationValue = spec != null ? spec.dashDuration : defaultDashDuration;
            return Mathf.Max(0f, dashSpeedValue * dashDurationValue);
        }
    }

    private void Awake()
    {
        cachedRigidbody = GetComponent<Rigidbody>();
        if (hitSequence == null) hitSequence = GetComponent<HitSequenceController>();
        if (moveController == null) moveController = GetComponent<PlayerMoveController>();
        if (moveController == null) moveController = GetComponentInParent<PlayerMoveController>();
        if (chainCombat == null) chainCombat = GetComponent<ChainCombatController>();
        if (chainCombat == null) chainCombat = GetComponentInParent<ChainCombatController>();
        if (targetingSystem == null) targetingSystem = GetComponent<TargetingSystem>();
        if (targetingSystem == null) targetingSystem = GetComponentInParent<TargetingSystem>();
        if (targetingSystem == null) targetingSystem = FindObjectOfType<TargetingSystem>();
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
            SyncMoveRotation();
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
        if (damageSystem == null || pendingDamage <= 0)
        {
            ClearPendingDamage();
            return;
        }

        if (pendingPierceTargets.Count > 0)
        {
            for (int i = 0; i < pendingPierceTargets.Count; i++)
            {
                var target = pendingPierceTargets[i];
                if (target == null) continue;
                damageSystem.ApplyDamage(target, pendingDamage);
            }
        }
        else if (pendingTarget != null)
        {
            damageSystem.ApplyDamage(pendingTarget, pendingDamage);
        }

        ClearPendingDamage();
    }

    private void ClearPendingDamage()
    {
        pendingTarget = null;
        pendingDamage = 0;
        contactStopTriggered = false;
        pendingPierceTargets.Clear();
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

    public bool TryStartAutoSlash(Transform target, Vector3 aimDirection, float aimDistance, TimingGrade grade, float damageMultiplier)
    {
        if (target == null) return false;
        if (!TryStartDash(target, aimDirection, aimDistance, spec)) return false;
        if (grade == TimingGrade.Miss) return true;
        if (spec == null) return true;

        pendingPierceTargets.Clear();
        pendingTarget = target;
        var multiplier = Mathf.Max(0f, damageMultiplier);
        pendingDamage = Mathf.RoundToInt(CalculateDamage(grade, spec) * multiplier);
        return true;
    }

    public bool TryStartAutoSlashPierce(Transform target, Vector3 aimDirection, float aimDistance, TimingGrade grade, float damageMultiplier, List<Transform> pierceTargets)
    {
        if (target == null) return false;
        if (!TryStartDash(target, aimDirection, aimDistance, spec)) return false;
        if (grade == TimingGrade.Miss) return true;
        if (spec == null) return true;

        pendingPierceTargets.Clear();
        pendingTarget = target;
        var multiplier = Mathf.Max(0f, damageMultiplier);
        pendingDamage = Mathf.RoundToInt(CalculateDamage(grade, spec) * multiplier);

        if (pierceTargets != null)
        {
            for (int i = 0; i < pierceTargets.Count; i++)
            {
                var candidate = pierceTargets[i];
                if (candidate == null) continue;
                if (!pendingPierceTargets.Contains(candidate))
                {
                    pendingPierceTargets.Add(candidate);
                }
            }
        }

        if (pendingPierceTargets.Count == 0 && target != null)
        {
            pendingPierceTargets.Add(target);
        }

        return true;
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
        dashRemainingDistance = 0f;
        pendingTarget = null;
        pendingDamage = 0;
        contactStopTriggered = false;
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
        if (!lockMovementDuringDash) return;
        if (moveController == null) return;
        moveController.SetMovementLocked(locked);
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

    private enum DashState
    {
        Idle,
        Dashing
    }
}
