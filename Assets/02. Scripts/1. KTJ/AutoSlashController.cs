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

    [Header("초기 타겟 확정")]
    [SerializeField] private bool useInitialTargetConfirm = true;
    [SerializeField, Min(0f)] private float initialTargetConfirmTime = 0.05f;
    [SerializeField, Range(0f, 45f)] private float initialTargetInstantAngle = 6f;

    [Header("초기 조준 안정")]
    [SerializeField] private bool useInitialAimStability = true;
    [SerializeField, Min(0f)] private float initialAimStableTime = 0.05f;
    [SerializeField, Range(0f, 720f)] private float initialAimMaxAngularSpeed = 180f;

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

    public event System.Action OnAttackReady;
    public bool IsChainSlowActive => chainCombat != null && chainCombat.IsSlowActive;

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
        if (chainCombat == null) chainCombat = FindObjectOfType<ChainCombatController>();
        if (spec == null && dashController != null) spec = dashController.Spec;
    }
}
