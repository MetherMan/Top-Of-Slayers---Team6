using UnityEngine;

using System.Collections.Generic;

public partial class AutoSlashController : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private TargetingSystem targetingSystem;
    [SerializeField] private SlashDashController dashController;
    [SerializeField] private AttackSpecSO spec;
    [SerializeField] private PlayerMoveController moveController;
    [SerializeField] private ChainCombatController chainCombat;
    [SerializeField] private PlayerCombatResource combatResource;

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

    [Header("초기 타겟 확정")]
    [SerializeField] private bool useInitialTargetConfirm = true;
    [SerializeField, Min(0f)] private float initialTargetConfirmTime = 0.05f;
    [SerializeField, Range(0f, 45f)] private float initialTargetInstantAngle = 6f;
    [SerializeField] private bool useInitialInstantConfirm;

    [Header("초기 조준 안정")]
    [SerializeField] private bool useInitialAimStability = true;
    [SerializeField, Min(0f)] private float initialAimStableTime = 0.05f;
    [SerializeField, Range(0f, 720f)] private float initialAimMaxAngularSpeed = 180f;

    [Header("초기 진입 입력")]
    [SerializeField] private bool requireStrongerInputForInitialAttack = true;
    [SerializeField, Range(0f, 1f)] private float initialInputDeadZone = 0.28f;

    [Header("초기 진입 각도 제한")]
    [SerializeField] private bool useInitialAimAngleLimit = true;
    [SerializeField, Range(0f, 45f)] private float initialAimMaxAngle = 12f;

    [Header("초기 반응 보정")]
    [SerializeField] private bool useAdaptiveInitialResponse = true;
    [SerializeField, Range(0.1f, 1f)] private float initialConfirmTimeMultiplier = 0.4f;
    [SerializeField, Range(0.1f, 1f)] private float initialAimStableTimeMultiplier = 0.4f;
    [SerializeField, Range(0.1f, 1f)] private float initialReadyDelayMultiplier = 0.3f;
    [SerializeField, Range(0f, 15f)] private float initialInstantAngleBonus = 2f;
    [SerializeField, Min(0f)] private float postChainAttackGraceTime = 0.18f;

    [Header("체인 재타격")]
    [SerializeField] private bool ignoreLastTargetDuringChain = true;
    [SerializeField, Range(0f, 90f)] private float allowSameTargetAngle = 12f;

    [Header("체인 동일 타겟 재공격")]
    [SerializeField] private bool useSameTargetRelease = true;
    [SerializeField, Range(0f, 90f)] private float sameTargetReleaseAngle = 25f;
    [SerializeField, Min(0f)] private float sameTargetAutoReleaseTime = 0.15f;
    [SerializeField] private bool allowForcedSameTargetReattack = true;
    [SerializeField] private bool useSameTargetReattackInputGate = true;
    [SerializeField, Range(0f, 90f)] private float sameTargetReattackOppositeAngle = 35f;
    [SerializeField, Range(0f, 90f)] private float sameTargetReattackInputAngle = 12f;
    [SerializeField, Min(0f)] private float sameTargetReattackHoldTime = 0.07f;
    [SerializeField, Min(0f)] private float sameTargetReattackInputBufferTime = 0.08f;
    [SerializeField, Min(0f)] private float sameTargetReattackDashLatchTime = 0.22f;

    [Header("조준 자동 보정")]
    [SerializeField] private bool useAimAssist = true;
    [SerializeField] private bool aimAssistOnlyDuringChain = true;
    [SerializeField, Min(0f)] private float aimAssistRadius = 1.2f;
    [SerializeField, Range(0f, 90f)] private float aimAssistAngle = 12f;

    [Header("체인 조준 원점")]
    [SerializeField] private bool useLastTargetAsAimOrigin = true;

    [Header("체인 라인 관통")]
    [SerializeField] private bool useChainLinePierce = true;

    [Header("초기 라인 관통")]
    [SerializeField] private bool useInitialLinePierce = true;

    [Header("초기 라인 앵커")]
    [SerializeField] private bool useInitialLineAnchor = true;
    [SerializeField, Min(0f)] private float initialLineAnchorWidthPadding = 0.05f;
    [SerializeField, Min(0f)] private float initialLineAnchorCenterBias = 4f;
    [SerializeField, Min(0f)] private float initialLinePierceRangeBonus = 1.5f;
    [SerializeField, Min(0f)] private float initialLinePierceBackPadding = 0.25f;

    [Header("라인 관통 여유")]
    [SerializeField, Min(1f)] private float linePierceWidthMultiplier = 1.35f;
    [SerializeField, Min(0f)] private float linePierceWidthPadding = 0.35f;

    [Header("라인 관통 대시")]
    [SerializeField, Min(0f)] private float pierceDashOvershootDistance = 0.6f;

    [Header("체인 반응 보정")]
    [SerializeField] private bool useAdaptiveChainConfirm = true;
    [SerializeField, Range(0.1f, 1f)] private float chainConfirmTimeMultiplier = 0.65f;
    [SerializeField, Range(0f, 20f)] private float chainInstantAngleBonus = 4f;

    [Header("단일 적 재공격 보정")]
    [SerializeField] private bool useSoloTargetRepeatDelay = true;
    [SerializeField, Min(0f)] private float soloTargetRepeatDelay = 0.1f;

    [Header("체인 먼 타깃 보정")]
    [SerializeField] private bool useExtendedChainTargetSearch = true;
    [SerializeField, Min(0f)] private float chainExtendedSearchRangeBonus = 2f;

    [Header("체인 사거리")]
    [SerializeField] private bool useChainRangeBoost = true;
    [SerializeField, Min(1f)] private float chainRangeMultiplier = 3f;

    [Header("판정")]
    [SerializeField] private TimingGrade autoGrade = TimingGrade.Good;
    [SerializeField] private bool useTargetingRange = true;
    [SerializeField, Min(0f)] private float manualRange = 0f;

    [Header("공격 코스트")]
    [SerializeField] private bool useSpecAttackCost = true;
    [SerializeField, Min(0)] private int manualAttackCost = 1;

    private float detectTimer;
    private float cooldownTimer;
    private float lastAttackRange;
    private bool wasChainActiveLastFrame;
    private float postChainAttackGraceTimer;
    private readonly List<Transform> linePriorityTargets = new List<Transform>(64);
    private readonly List<MonoBehaviour> damageableSearchBuffer = new List<MonoBehaviour>(16);

    public event System.Action OnAttackReady;
    public bool IsChainSlowActive => chainCombat != null && chainCombat.IsSlowActive;

    private void Awake()
    {
        CacheLocalRefs();
        CacheSceneRefs();
        if (spec == null && dashController != null) spec = dashController.Spec;
    }

    private void CacheLocalRefs()
    {
        if (dashController == null) dashController = GetComponent<SlashDashController>();
        if (dashController == null) dashController = GetComponentInParent<SlashDashController>();

        if (targetingSystem == null) targetingSystem = GetComponent<TargetingSystem>();
        if (targetingSystem == null) targetingSystem = GetComponentInParent<TargetingSystem>();

        if (moveController == null) moveController = GetComponent<PlayerMoveController>();
        if (moveController == null) moveController = GetComponentInParent<PlayerMoveController>();

        if (chainCombat == null) chainCombat = GetComponent<ChainCombatController>();
        if (chainCombat == null) chainCombat = GetComponentInParent<ChainCombatController>();

        if (combatResource == null) combatResource = GetComponent<PlayerCombatResource>();
        if (combatResource == null) combatResource = GetComponentInParent<PlayerCombatResource>();
    }

    private void CacheSceneRefs()
    {
        if (chainCombat == null) chainCombat = FindObjectOfType<ChainCombatController>();
        if (combatResource == null) combatResource = FindObjectOfType<PlayerCombatResource>();
    }
}
