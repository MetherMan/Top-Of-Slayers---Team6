using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public partial class TargetingLineVisualizer : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private TargetingSystem targetingSystem;
    [SerializeField] private LineRenderer line;
    [SerializeField] private Transform followTarget;
    [SerializeField] private SlashDashController dashController;
    [SerializeField] private AutoSlashController autoSlash;
    [SerializeField] private PlayerMoveController moveController;

    [Header("표시")]
    [SerializeField] private Vector3 originOffset = new Vector3(0f, 0.1f, 0f);
    [SerializeField] private bool alwaysVisible = true;
    [SerializeField] private bool useDashRange = true;
    [SerializeField] private bool useAutoSlashAimPreview = true;
    [SerializeField] private bool useAutoSlashAimOrigin = true;
    [SerializeField] private bool useDottedLine = true;
    [SerializeField, Min(0.3f)] private float dotRepeatPerMeter = 1f;
    [SerializeField, Range(0.1f, 0.9f)] private float dotFillRatio = 0.4f;
    [SerializeField, Min(0.005f)] private float dotMinWorldSize = 0.01f;
    [SerializeField, Range(0.6f, 1.4f)] private float dotTileSizeMultiplier = 1f;
    [SerializeField, Min(0.01f)] private float dottedMinWorldWidth = 0.08f;
    [SerializeField] private Color idleLineColor = new Color(1f, 1f, 1f, 0.7f);
    [SerializeField] private Color detectedLineColor = new Color(0.55f, 1f, 0.75f, 0.95f);
    [SerializeField, Range(0.05f, 1f)] private float lineWidthMultiplier = 0.18f;
    [SerializeField, Min(0.005f)] private float minLineWidth = 0.04f;

    [Header("타겟 확정 연출")]
    [SerializeField] private bool useTargetConfirmFx = true;
    [SerializeField] private Color targetReadyColor = new Color(0.3f, 1f, 0.45f, 0.95f);
    [SerializeField] private Color targetCautionColor = new Color(1f, 0.92f, 0.25f, 0.98f);
    [SerializeField] private Color targetWarningColor = new Color(1f, 0.58f, 0.18f, 1f);
    [SerializeField] private Color targetConfirmColor = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField, Range(0f, 1f)] private float cautionThreshold = 0.35f;
    [SerializeField, Range(0f, 1f)] private float warningThreshold = 0.7f;
    [SerializeField, Range(0f, 1f)] private float dangerThreshold = 0.9f;
    [SerializeField, Range(0f, 1f)] private float lineColorBlend = 0.6f;
    [SerializeField, Range(0f, 1f)] private float stageFlashToWhite = 0.35f;
    [SerializeField, Range(0f, 0.5f)] private float stagePulseAmount = 0.08f;
    [SerializeField, Min(0f)] private float stagePulseSpeed = 7f;
    [SerializeField, Min(0f)] private float stagePulseSpeedByStage = 2.2f;
    [SerializeField, Range(0f, 1f)] private float stageTransitionKickAmount = 0.3f;
    [SerializeField, Min(0f)] private float stageTransitionKickDuration = 0.15f;
    [SerializeField, Min(0f)] private float confirmSmoothSpeed = 14f;
    [SerializeField, Range(0f, 1f)] private float confirmPulseAmount = 0.18f;
    [SerializeField, Min(0f)] private float confirmPulseSpeed = 10f;
    [SerializeField, Min(1f)] private float confirmLineWidthBoost = 1.15f;

    [Header("기존 몬스터 링 연출")]
    [FormerlySerializedAs("useGroundTargetMarker")]
    [SerializeField] private bool useMonsterExistingRingFx = true;
    [SerializeField] private Color monsterRingIdleFallbackColor = new Color(1f, 1f, 1f, 1f);
    [FormerlySerializedAs("groundMarkerColorBlend")]
    [SerializeField, Range(0f, 1f)] private float monsterRingColorBlend = 1f;
    [SerializeField, Range(0f, 0.5f)] private float monsterRingPulseAmount = 0.1f;
    [SerializeField, Min(0f)] private float monsterRingPulseSpeed = 10f;
    [SerializeField, Range(0.1f, 2f)] private float monsterRingBaseScaleMultiplier = 0.4f;
    [SerializeField, Min(1f)] private float monsterRingConfirmScaleMultiplier = 1.12f;
    [SerializeField, Min(1f)] private float monsterRingStageScaleMultiplier = 1.05f;
    [FormerlySerializedAs("groundMarkerEmission")]
    [SerializeField, Min(0f)] private float monsterRingEmission = 0.5f;
    [SerializeField] private bool restoreMonsterRingOnRelease = true;
    [SerializeField] private bool showMonsterRingForAllTargets = true;
    [FormerlySerializedAs("groundTargetMarkerPrefab")]
    [SerializeField] private GameObject monsterRingPrefab;
    [FormerlySerializedAs("groundMarkerBaseScale")]
    [SerializeField] private Vector3 monsterRingBaseScale = Vector3.one;
    [FormerlySerializedAs("groundMarkerScaleByRadius")]
    [SerializeField, Min(0f)] private float monsterRingScaleByTargetRadius = 1.4f;
    [FormerlySerializedAs("groundMarkerHeightOffset")]
    [SerializeField, Min(0f)] private float monsterRingHeightOffset = 0.02f;
    [FormerlySerializedAs("groundMarkerUseGroundRaycast")]
    [SerializeField] private bool monsterRingUseGroundRaycast = true;
    [FormerlySerializedAs("groundMarkerRaycastHeight")]
    [SerializeField, Min(0.1f)] private float monsterRingRaycastHeight = 2f;
    [FormerlySerializedAs("groundMarkerRaycastDistance")]
    [SerializeField, Min(0.1f)] private float monsterRingRaycastDistance = 6f;
    [FormerlySerializedAs("groundMarkerRaycastMask")]
    [SerializeField] private LayerMask monsterRingRaycastMask = ~0;
    [SerializeField, Min(0.05f)] private float targetColorFillDuration = 0.25f;
    [SerializeField, Range(0f, 0.5f)] private float confirmColorEarlyRedWindow = 0.22f;
    [SerializeField, Range(0f, 0.7f)] private float confirmColorLeadProgress = 0.32f;
    [SerializeField, Min(0f)] private float postConfirmColorHoldDuration = 0.12f;
    [SerializeField] private bool forceRedRingDuringChain = true;
    [SerializeField] private Color chainForcedRingColor = new Color(1f, 0f, 0f, 1f);
    [SerializeField] private bool useMonsterRingDynamicTween = true;
    [SerializeField, Min(0f)] private float monsterRingLockOnPunchScale = 0.45f;
    [SerializeField, Min(0.01f)] private float monsterRingLockOnPunchDuration = 0.24f;
    [SerializeField, Min(0f)] private float monsterRingStagePunchScale = 0.24f;
    [SerializeField, Min(0.01f)] private float monsterRingStagePunchDuration = 0.18f;
    [SerializeField] private bool useLockOnIcon = true;
    [SerializeField] private GameObject lockOnIconPrefab;
    [SerializeField] private Sprite lockOnIconSprite;
    [SerializeField] private Color lockOnIconColor = new Color(1f, 0.16f, 0.16f, 1f);
    [SerializeField] private Vector3 lockOnIconOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField, Min(0.05f)] private float lockOnIconScale = 1.8f;
    [SerializeField] private bool lockOnIconUseFlatPlane = true;
    [SerializeField, Range(-180f, 180f)] private float lockOnIconFlatPlanePitch = 90f;
    [SerializeField] private bool lockOnIconBillboard = false;
    [SerializeField, Min(0.01f)] private float lockOnIconAppearSpeed = 10f;
    [SerializeField, Range(0f, 1f)] private float lockOnIconMinAlpha = 0.85f;
    [SerializeField, Range(0f, 1f)] private float lockOnIconColorBlend = 0.25f;
    [SerializeField, Min(0f)] private float lockOnIconPulseAmount = 0.03f;
    [SerializeField, Min(0f)] private float lockOnIconPulseSpeed = 10f;
    [SerializeField, Min(0f)] private float lockOnIconSpinSpeed = 120f;
    [SerializeField, Range(0f, 1.5f)] private float lockOnIconConfirmScaleBoost = 0.05f;

    private readonly List<GradientAlphaKey> alphaKeys = new List<GradientAlphaKey>(128);
    private readonly List<GradientColorKey> colorKeys = new List<GradientColorKey>(2);
    private static readonly int BaseColorPropertyId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");
    private static readonly int EmissionColorPropertyId = Shader.PropertyToID("_EmissionColor");
    private Material solidLineMaterial;
    private Material dottedLineMaterial;
    private Texture2D dottedTexture;
    private float confirmProgressVisual;
    private int confirmStage = -1;
    private float confirmStageKickTimer;
    private Transform lastVisualTarget;
    private float targetColorFillTimer;
    private Transform lastRawConfirmTarget;
    private float lastRawConfirmProgress;
    private Transform postConfirmHoldTarget;
    private float postConfirmHoldTimer;
    private bool wasChainForcedRingColor;
    private readonly List<Transform> monsterRingTargets = new List<Transform>(64);
    private readonly List<Transform> monsterRingCleanupTargets = new List<Transform>(64);
    private readonly Dictionary<Transform, MonsterRingEntry> monsterRingEntries = new Dictionary<Transform, MonsterRingEntry>(64);

    private void Awake()
    {
        monsterRingIdleFallbackColor = Color.white;

        if (targetingSystem == null) targetingSystem = GetComponent<TargetingSystem>();
        if (targetingSystem == null) targetingSystem = GetComponentInParent<TargetingSystem>();
        if (followTarget == null)
        {
            followTarget = targetingSystem != null ? targetingSystem.transform : transform;
        }

        if (dashController == null) dashController = GetComponent<SlashDashController>();
        if (dashController == null) dashController = GetComponentInParent<SlashDashController>();
        if (autoSlash == null) autoSlash = GetComponent<AutoSlashController>();
        if (autoSlash == null) autoSlash = GetComponentInParent<AutoSlashController>();
        if (moveController == null) moveController = GetComponent<PlayerMoveController>();
        if (moveController == null) moveController = GetComponentInParent<PlayerMoveController>();

        if (line != null)
        {
            line.positionCount = 2;
            line.useWorldSpace = true;
            line.enabled = alwaysVisible;
            EnsureDefaultLineMaterial(line);
            solidLineMaterial = line.sharedMaterial;
        }
    }

    private void LateUpdate()
    {
        if (line == null || targetingSystem == null)
        {
            ReleaseMonsterExistingRing(false);
            return;
        }

        if (!alwaysVisible && line.enabled) line.enabled = false;

        var source = followTarget != null ? followTarget : transform;
        var origin = source.position + originOffset;
        var direction = source.forward;

        if (useAutoSlashAimPreview && autoSlash != null && autoSlash.TryGetAimPreview(out var previewOrigin, out var previewDirection))
        {
            direction = previewDirection;
            if (useAutoSlashAimOrigin)
            {
                origin = previewOrigin + originOffset;
            }
        }
        else if (moveController != null)
        {
            direction = moveController.GetAimDirection();
        }

        direction.y = 0f;
        if (direction.sqrMagnitude <= 0f)
        {
            direction = Vector3.forward;
        }
        direction = direction.normalized;

        var length = GetLineLength();
        var baseWorldWidth = Mathf.Max(minLineWidth, targetingSystem.LineWidth * lineWidthMultiplier);
        var renderWorldWidth = useDottedLine ? Mathf.Max(baseWorldWidth, dottedMinWorldWidth) : baseWorldWidth;

        var target = targetingSystem.GetTarget(origin, direction, length, null);
        var hasTarget = target != null;
        var rawConfirmProgress = GetTargetConfirmProgress(target);
        UpdateColorCycleReset(target, rawConfirmProgress);
        rawConfirmProgress = ApplyPostConfirmColorHold(target, rawConfirmProgress);
        confirmProgressVisual = SmoothConfirmProgress(confirmProgressVisual, rawConfirmProgress, hasTarget);
        var visualConfirmProgress = Mathf.Max(confirmProgressVisual, GetVisualTargetFillProgress(target));
        var colorProgress = ApplyColorLeadProgress(visualConfirmProgress);
        var stageFx = UpdateConfirmStageFx(hasTarget, colorProgress);
        ResolveTargetColors(hasTarget, colorProgress, stageFx, out var lineStartColor, out var lineEndColor, out var ringColor);

        var lineBoostProgress = hasTarget && useTargetConfirmFx ? Mathf.Clamp01(colorProgress + (stageFx * 0.35f)) : 0f;
        var lineWidthBoost = hasTarget && useTargetConfirmFx ? Mathf.Lerp(1f, confirmLineWidthBoost, lineBoostProgress) : 1f;
        var localWidth = AdjustWidthByScale(renderWorldWidth * lineWidthBoost);

        line.startWidth = localWidth;
        line.endWidth = localWidth;
        ApplyLineStyle(lineStartColor, lineEndColor, length, renderWorldWidth);

        var end = origin + direction * length;
        if (hasTarget)
        {
            end = target.position;
            end.y = origin.y;
        }

        line.SetPosition(0, origin);
        line.SetPosition(1, end);
        UpdateMonsterExistingRing(target, ringColor, colorProgress, stageFx);
    }

}


