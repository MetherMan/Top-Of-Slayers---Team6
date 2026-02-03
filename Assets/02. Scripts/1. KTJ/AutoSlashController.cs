using UnityEngine;

public class AutoSlashController : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private TargetingSystem targetingSystem;
    [SerializeField] private SlashDashController dashController;
    [SerializeField] private AttackSpecSO spec;
    [SerializeField] private PlayerMoveController moveController;
    [SerializeField] private ChainCombatController chainCombat;

    [Header("감지")]
    [SerializeField] private float detectInterval = 0f;

    [Header("쿨타임")]
    [SerializeField] private bool useSpecCooldown = true;
    [SerializeField, Min(0f)] private float manualCooldown = 0.2f;

    [Header("체인 조준 보정")]
    [SerializeField] private bool useChainAimConfirm = true;
    [SerializeField, Min(0f)] private float chainAimConfirmTime = 0.07f;
    [SerializeField, Range(0f, 90f)] private float chainAimConfirmAngle = 8f;

    [Header("체인 재타격")]
    [SerializeField] private bool ignoreLastTargetDuringChain = true;
    [SerializeField, Range(0f, 90f)] private float allowSameTargetAngle = 12f;

    [Header("판정")]
    [SerializeField] private TimingGrade autoGrade = TimingGrade.Good;
    [SerializeField] private bool useTargetingRange = true;
    [SerializeField, Min(0f)] private float manualRange = 0f;

    private float detectTimer;
    private float cooldownTimer;
    private Vector3 confirmedAimDirection = Vector3.forward;
    private Vector3 pendingAimDirection;
    private float pendingAimTimer;
    private bool hasConfirmedAim;
    private bool hasPendingAim;

    private void Awake()
    {
        if (dashController == null) dashController = GetComponent<SlashDashController>();
        if (dashController == null) dashController = GetComponentInParent<SlashDashController>();
        if (targetingSystem == null) targetingSystem = GetComponent<TargetingSystem>();
        if (targetingSystem == null) targetingSystem = GetComponentInParent<TargetingSystem>();
        if (moveController == null) moveController = GetComponent<PlayerMoveController>();
        if (moveController == null) moveController = GetComponentInParent<PlayerMoveController>();
        if (chainCombat == null) chainCombat = GetComponent<ChainCombatController>();
        if (chainCombat == null) chainCombat = GetComponentInParent<ChainCombatController>();
        if (spec == null && dashController != null) spec = dashController.Spec;
    }

    private void Update()
    {
        if (dashController == null || targetingSystem == null) return;
        if (Mathf.Approximately(Time.timeScale, 0f)) return;

        var isChainActive = chainCombat != null && chainCombat.IsSlowActive;
        var delta = isChainActive ? Time.unscaledDeltaTime : Time.deltaTime;

        if (!isChainActive && cooldownTimer > 0f) cooldownTimer -= delta;

        if (!isChainActive && detectInterval > 0f)
        {
            detectTimer -= delta;
            if (detectTimer > 0f) return;
            detectTimer = detectInterval;
        }

        if (!isChainActive && cooldownTimer > 0f) return;
        if (dashController.IsDashing) return;

        var aimDirection = GetStableAimDirection(isChainActive, delta);
        if (aimDirection.sqrMagnitude <= 0f) return;

        var searchRange = GetAttackRange();
        var ignoreTarget = GetIgnoreTarget(isChainActive, aimDirection);
        var target = targetingSystem.GetTarget(transform.position, aimDirection, searchRange, ignoreTarget);
        if (target == null) return;

        if (chainCombat != null)
        {
            chainCombat.CancelSlow();
        }

        var aimDistance = searchRange > 0f ? searchRange : 0f;
        var damageMultiplier = chainCombat != null ? chainCombat.GetDamageMultiplier(target) : 1f;
        if (dashController.TryStartAutoSlash(target, aimDirection, aimDistance, autoGrade, damageMultiplier))
        {
            cooldownTimer = GetCooldown();
        }
    }

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

    public float AttackRange => GetAttackRange();

    private Vector3 GetAimDirection()
    {
        if (moveController != null)
        {
            return moveController.GetAimDirection();
        }

        return transform.forward;
    }

    private Transform GetIgnoreTarget(bool isChainActive, Vector3 aimDirection)
    {
        if (!isChainActive || !ignoreLastTargetDuringChain) return null;
        if (chainCombat == null) return null;
        var lastTarget = chainCombat.LastTarget;
        if (lastTarget == null) return null;
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
