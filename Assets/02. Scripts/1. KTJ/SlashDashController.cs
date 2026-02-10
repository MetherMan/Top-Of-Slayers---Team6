using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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
    [SerializeField] private bool useDashEase = true;
    [SerializeField] private bool dashEaseOnlyDuringChain = true;
    [SerializeField] private Ease dashEase = Ease.InOutQuad;

    [Header("경로 VFX")]
    [SerializeField] private bool useDashTrailVfx = false;
    [SerializeField] private bool autoCreateDashTrail = false;
    [SerializeField] private TrailRenderer dashTrail;
    [SerializeField, Min(0.01f)] private float dashTrailTime = 0.2f;
    [SerializeField, Min(0.005f)] private float dashTrailWidth = 0.25f;
    [SerializeField] private Color dashTrailStartColor = new Color(0.4f, 1f, 0.85f, 0.8f);
    [SerializeField] private Color dashTrailEndColor = new Color(0.4f, 1f, 0.85f, 0f);
    [SerializeField] private Material dashTrailMaterial;
    [SerializeField] private bool dashTrailOnlyDuringDash = true;

    [Header("경로 VFX 프리팹")]
    [SerializeField] private GameObject dashPathVfxPrefab;
    [SerializeField] private Transform dashPathVfxAnchor;
    [SerializeField] private bool keepDashPathVfxAfterDash = true;
    [SerializeField, Min(0f)] private float dashPathVfxDestroyDelay = 1f;

    private DashState state = DashState.Idle;
    private float dashSpeed;
    private float dashTimer;
    private float dashTotalTime;
    private float dashRemainingDistance;
    private float dashTotalDistance;
    private Vector3 dashDirection;
    private Transform pendingTarget;
    private int pendingDamage;
    private readonly List<Transform> pendingPierceTargets = new List<Transform>();
    private bool contactStopTriggered;
    private bool impactTriggered;
    private Rigidbody cachedRigidbody;
    private bool cachedKinematic;
    private bool cachedUseGravity;
    private bool dashRotationLockApplied;
    private GameObject activeDashPathVfx;

    public bool IsDashing => state == DashState.Dashing;
    public Vector3 DashDirection => dashDirection;
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
        if (damageSystem == null) damageSystem = GetComponent<DamageSystem>();
        if (damageSystem == null) damageSystem = GetComponentInParent<DamageSystem>();
        if (damageSystem == null) damageSystem = FindObjectOfType<DamageSystem>();
        if (hitSequence == null) hitSequence = GetComponent<HitSequenceController>();
        if (moveController == null) moveController = GetComponent<PlayerMoveController>();
        if (moveController == null) moveController = GetComponentInParent<PlayerMoveController>();
        if (chainCombat == null) chainCombat = GetComponent<ChainCombatController>();
        if (chainCombat == null) chainCombat = GetComponentInParent<ChainCombatController>();
        if (chainCombat == null) chainCombat = FindObjectOfType<ChainCombatController>();
        if (targetingSystem == null) targetingSystem = GetComponent<TargetingSystem>();
        if (targetingSystem == null) targetingSystem = GetComponentInParent<TargetingSystem>();
        if (targetingSystem == null) targetingSystem = FindObjectOfType<TargetingSystem>();
        if (dashPathVfxAnchor == null) dashPathVfxAnchor = transform;
        SetupDashTrailVfx();
    }

    private enum DashState
    {
        Idle,
        Dashing
    }

    private void OnDisable()
    {
        ForceStop();
    }

    private void SetupDashTrailVfx()
    {
        if (!useDashTrailVfx)
        {
            if (dashTrail != null)
            {
                dashTrail.Clear();
                dashTrail.emitting = false;
            }
            return;
        }

        if (dashTrail == null)
        {
            if (!autoCreateDashTrail) return;
            var trailObject = new GameObject("DashTrailVfx");
            trailObject.transform.SetParent(transform, false);
            dashTrail = trailObject.AddComponent<TrailRenderer>();
        }

        dashTrail.time = Mathf.Max(0.01f, dashTrailTime);
        dashTrail.startWidth = Mathf.Max(0.005f, dashTrailWidth);
        dashTrail.endWidth = 0f;
        dashTrail.minVertexDistance = 0.02f;
        dashTrail.alignment = LineAlignment.View;
        dashTrail.textureMode = LineTextureMode.Stretch;
        dashTrail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        dashTrail.receiveShadows = false;
        dashTrail.startColor = dashTrailStartColor;
        dashTrail.endColor = dashTrailEndColor;

        if (dashTrailMaterial != null)
        {
            dashTrail.material = dashTrailMaterial;
        }
        else if (dashTrail.sharedMaterial == null)
        {
            var shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                dashTrail.material = new Material(shader);
            }
        }

        dashTrail.Clear();
        dashTrail.emitting = !dashTrailOnlyDuringDash;
    }

    private void SetDashTrailState(bool isDashing)
    {
        if (!useDashTrailVfx || dashTrail == null) return;

        if (isDashing)
        {
            dashTrail.Clear();
            dashTrail.emitting = true;
            return;
        }

        if (dashTrailOnlyDuringDash)
        {
            dashTrail.emitting = false;
        }
    }

    private void SpawnDashPathVfx()
    {
        if (dashPathVfxPrefab == null) return;
        StopDashPathVfx(false);

        var anchor = dashPathVfxAnchor != null ? dashPathVfxAnchor : transform;
        activeDashPathVfx = Instantiate(dashPathVfxPrefab, anchor.position, anchor.rotation, anchor);
    }

    private void StopDashPathVfx(bool keepResidual)
    {
        if (activeDashPathVfx == null) return;

        var instance = activeDashPathVfx;
        activeDashPathVfx = null;

        if (keepResidual)
        {
            instance.transform.SetParent(null, true);
        }

        var trails = instance.GetComponentsInChildren<TrailRenderer>(true);
        for (int i = 0; i < trails.Length; i++)
        {
            trails[i].emitting = false;
        }

        var particleSystems = instance.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particleSystems.Length; i++)
        {
            particleSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (dashPathVfxDestroyDelay > 0f)
        {
            Destroy(instance, dashPathVfxDestroyDelay);
            return;
        }

        Destroy(instance);
    }
}
