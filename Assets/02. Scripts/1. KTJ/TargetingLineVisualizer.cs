using System.Collections.Generic;
using UnityEngine;

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

    [Header("타겟 원형 마커")]
    [SerializeField] private bool useTargetRing = true;
    [SerializeField] private LineRenderer targetRing;
    [SerializeField, Min(0.05f)] private float targetRingRadius = 0.75f;
    [SerializeField, Range(8, 64)] private int targetRingSegments = 24;
    [SerializeField, Min(0f)] private float targetRingHeight = 0.03f;
    [SerializeField, Range(0.2f, 2f)] private float targetRingWidthMultiplier = 1f;

    private readonly List<GradientAlphaKey> alphaKeys = new List<GradientAlphaKey>(128);
    private readonly List<GradientColorKey> colorKeys = new List<GradientColorKey>(2);
    private Material solidLineMaterial;
    private Material dottedLineMaterial;
    private Texture2D dottedTexture;

    private void Awake()
    {
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

        EnsureTargetRing();
    }

    private void LateUpdate()
    {
        if (line == null || targetingSystem == null) return;
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
        var localWidth = AdjustWidthByScale(renderWorldWidth);
        var ringLocalWidth = AdjustWidthByScale(baseWorldWidth);

        var target = targetingSystem.GetTarget(origin, direction, length, null);
        var hasTarget = target != null;

        line.startWidth = localWidth;
        line.endWidth = localWidth;
        ApplyLineStyle(hasTarget, length, renderWorldWidth);

        var end = origin + direction * length;
        if (hasTarget)
        {
            end = target.position;
            end.y = origin.y;
        }

        line.SetPosition(0, origin);
        line.SetPosition(1, end);
        UpdateTargetRing(target, ringLocalWidth, hasTarget ? detectedLineColor : idleLineColor);
    }

    private float AdjustWidthByScale(float width)
    {
        if (line == null) return width;
        var scale = line.transform.lossyScale;
        var maxScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
        if (maxScale <= 0f) return width;
        return width / maxScale;
    }

    private float GetLineLength()
    {
        if (autoSlash != null)
        {
            var attackRange = autoSlash.GetPreviewRange();
            if (attackRange > 0f) return attackRange;
        }

        if (useDashRange && dashController != null)
        {
            var dashRange = dashController.DefaultDashDistance;
            if (dashRange > 0f) return dashRange;
        }

        return Mathf.Max(0f, targetingSystem.MaxRange);
    }

    private void ApplyLineStyle(bool hasTarget, float length, float worldWidth)
    {
        if (line == null) return;

        var color = hasTarget ? detectedLineColor : idleLineColor;
        if (!useDottedLine)
        {
            ApplySolidLineMaterial();
            line.colorGradient = BuildSolidGradient(color);
            return;
        }

        ApplyDottedLineMaterial(length, worldWidth);
        line.colorGradient = BuildSolidGradient(color);
    }
}
