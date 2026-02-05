using System.Collections.Generic;
using UnityEngine;

public partial class SlashDashController : MonoBehaviour
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
    private bool impactTriggered;
    private Rigidbody cachedRigidbody;
    private bool cachedKinematic;
    private bool cachedUseGravity;

    public bool IsDashing => state == DashState.Dashing;
    public AttackSpecSO Spec => spec;
    public event System.Action OnDashStarted;
    public event System.Action OnDashImpact;

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

    private enum DashState
    {
        Idle,
        Dashing
    }
}
