using UnityEngine;

public partial class AutoSlashController : MonoBehaviour
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
    [SerializeField, Range(0f, 180f)] private float chainAimSnapAngle = 60f;

    [Header("체인 전환 제한")]
    [SerializeField] private bool blockAttackWhileAimChanging = true;
    [SerializeField, Range(0f, 90f)] private float blockAttackAngle = 8f;

    [Header("체인 입력")]
    [SerializeField] private bool requireInputDuringChain = true;
    [SerializeField, Range(0f, 1f)] private float chainInputDeadZone = 0.15f;

    [Header("체인 조준 제한")]
    [SerializeField, Range(0f, 180f)] private float chainAimMaxAngle = 35f;

    [Header("체인 타겟 유지")]
    [SerializeField] private bool useChainTargetRetention = true;
    [SerializeField, Range(0f, 45f)] private float chainRetainTargetAngle = 15f;

    [Header("체인 타겟 확정")]
    [SerializeField] private bool useChainTargetConfirm = true;
    [SerializeField, Min(0f)] private float chainTargetConfirmTime = 0.05f;
    [SerializeField, Min(0f)] private float chainSameTargetConfirmTime = 0.08f;
    [SerializeField, Range(0f, 45f)] private float chainTargetInstantAngle = 6f;

    [Header("디버그")]
    [SerializeField] private bool enableAimDebugLog = false;
    [SerializeField] private bool logAimPreview = false;
    [SerializeField, Min(0f)] private float aimLogInterval = 0.25f;
    [SerializeField, Range(0f, 45f)] private float aimLogAngleThreshold = 3f;
    [SerializeField, Min(0f)] private float aimLogOriginThreshold = 0.2f;

    [Header("체인 재타격")]
    [SerializeField] private bool ignoreLastTargetDuringChain = true;
    [SerializeField, Range(0f, 90f)] private float allowSameTargetAngle = 12f;

    [Header("체인 동일 타겟 재공격")]
    [SerializeField] private bool useSameTargetRelease = true;
    [SerializeField, Range(0f, 90f)] private float sameTargetReleaseAngle = 25f;

    [Header("조준 자동 보정")]
    [SerializeField] private bool useAimAssist = true;
    [SerializeField] private bool aimAssistOnlyDuringChain = true;
    [SerializeField, Min(0f)] private float aimAssistRadius = 1.2f;
    [SerializeField, Range(0f, 90f)] private float aimAssistAngle = 12f;

    [Header("체인 조준 원점")]
    [SerializeField] private bool useLastTargetAsAimOrigin = true;

    [Header("체인 라인 관통")]
    [SerializeField] private bool useChainLinePierce = true;

    [Header("체인 사거리")]
    [SerializeField] private bool useChainRangeBoost = true;
    [SerializeField, Min(1f)] private float chainRangeMultiplier = 1.5f;

    [Header("판정")]
    [SerializeField] private TimingGrade autoGrade = TimingGrade.Good;
    [SerializeField] private bool useTargetingRange = true;
    [SerializeField, Min(0f)] private float manualRange = 0f;

    private float detectTimer;
    private float cooldownTimer;
    private float lastAttackRange;

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

        if (!isChainActive)
        {
            ResetChainTargetConfirm();
            ResetSameTargetRelease();
        }

        var rawAimDirection = GetAimDirection();
        rawAimDirection.y = 0f;
        if (rawAimDirection.sqrMagnitude <= 0f)
        {
            rawAimDirection = transform.forward;
        }
        else
        {
            rawAimDirection = rawAimDirection.normalized;
        }
        UpdateSameTargetRelease(rawAimDirection);

        var searchRange = GetAttackRange();
        if (isChainActive && useChainRangeBoost)
        {
            searchRange *= chainRangeMultiplier;
        }
        lastAttackRange = searchRange;
        var aimOrigin = GetAimOrigin(isChainActive);
        var previewIgnoreTarget = GetIgnoreTarget(isChainActive, rawAimDirection);
        var previewDirection = rawAimDirection;
        if (TryGetAimAssistDirection(isChainActive, aimOrigin, rawAimDirection, searchRange, previewIgnoreTarget, out var previewAdjustedDirection, out _))
        {
            previewDirection = previewAdjustedDirection;
        }
        UpdateAimPreview(aimOrigin, previewDirection);
        TryLogAimPreview(aimOrigin, rawAimDirection, previewDirection, searchRange);

        if (isChainActive && requireInputDuringChain && moveController != null)
        {
            if (!moveController.HasAimInput(chainInputDeadZone))
            {
                ResetChainTargetConfirm();
                ResetSameTargetRelease();
                return;
            }
        }

        if (!isChainActive && cooldownTimer > 0f) cooldownTimer -= delta;

        if (!isChainActive && detectInterval > 0f)
        {
            detectTimer -= delta;
            if (detectTimer > 0f) return;
            detectTimer = detectInterval;
        }

        if (!isChainActive && cooldownTimer > 0f) return;
        if (dashController.IsDashing) return;

        var baseAimDirection = GetStableAimDirection(isChainActive, delta);
        if (isChainActive && useChainAimConfirm && blockAttackWhileAimChanging)
        {
            var angle = Vector3.Angle(rawAimDirection, baseAimDirection);
            if (angle > blockAttackAngle)
            {
                if (enableAimDebugLog)
                {
                    Debug.Log($"[조준] 공격보류 angle:{angle:F1} raw:{rawAimDirection} confirm:{baseAimDirection}");
                }
                return;
            }
        }
        var ignoreTarget = GetIgnoreTarget(isChainActive, rawAimDirection);
        var aimDirection = baseAimDirection;
        Transform target = null;

        if (isChainActive && targetingSystem != null)
        {
            if (TryGetChainPriorityTarget(aimOrigin, rawAimDirection, searchRange, ignoreTarget, out var chainTarget, out var chainDirection))
            {
                if (!TryConfirmChainTarget(chainTarget, chainDirection, rawAimDirection, delta, out var confirmedTarget, out var confirmedDirection))
                {
                    return;
                }
                target = confirmedTarget;
                aimDirection = confirmedDirection;
            }
            else
            {
                return;
            }
        }

        if (target == null)
        {
            Transform assistTarget = null;
            if (TryGetAimAssistDirection(isChainActive, aimOrigin, baseAimDirection, searchRange, ignoreTarget, out var adjustedDirection, out var resolvedAssistTarget))
            {
                aimDirection = adjustedDirection;
                assistTarget = resolvedAssistTarget;
            }

            target = assistTarget ?? targetingSystem.GetTarget(aimOrigin, aimDirection, searchRange, ignoreTarget);
        }
        if (target == null) return;
        if (isChainActive && useSameTargetRelease && !sameTargetReleased && target == lastAttackTarget)
        {
            LogSameTargetBlock(target);
            return;
        }

        var aimDistance = searchRange > 0f ? searchRange : 0f;
        var damageMultiplier = chainCombat != null ? chainCombat.GetDamageMultiplier(target) : 1f;
        var pierceTargets = GetPierceTargets(isChainActive, aimOrigin, aimDirection, searchRange, ignoreTarget, target);
        if (pierceTargets != null && pierceTargets.Count > 1)
        {
            if (dashController.TryStartAutoSlashPierce(target, aimDirection, aimDistance, autoGrade, damageMultiplier, pierceTargets))
            {
                RegisterAttackTarget(target, rawAimDirection);
                cooldownTimer = GetCooldown();
                if (enableAimDebugLog)
                {
                    Debug.Log($"[조준] 관통 타겟:{pierceTargets.Count} first:{target.name} aim:{aimDirection} range:{searchRange:F2}");
                }
            }
            return;
        }

        if (dashController.TryStartAutoSlash(target, aimDirection, aimDistance, autoGrade, damageMultiplier))
        {
            RegisterAttackTarget(target, rawAimDirection);
            cooldownTimer = GetCooldown();
            if (enableAimDebugLog)
            {
                Debug.Log($"[조준] 단일 타겟:{target.name} aim:{aimDirection} range:{searchRange:F2}");
            }
        }
    }

}
