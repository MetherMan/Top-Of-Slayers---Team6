using System.Collections.Generic;
using UnityEngine;

public class TargetingLineVisualizer : MonoBehaviour
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
    [SerializeField, Min(1f)] private float dotRepeatPerMeter = 2.2f;
    [SerializeField] private Color idleLineColor = new Color(1f, 1f, 1f, 0.7f);
    [SerializeField] private Color detectedLineColor = new Color(0.55f, 1f, 0.75f, 0.95f);

    [Header("타겟 원형 마커")]
    [SerializeField] private bool useTargetRing = true;
    [SerializeField] private LineRenderer targetRing;
    [SerializeField, Min(0.05f)] private float targetRingRadius = 0.35f;
    [SerializeField, Range(8, 64)] private int targetRingSegments = 24;
    [SerializeField, Min(0f)] private float targetRingHeight = 0.03f;
    [SerializeField, Range(0.2f, 2f)] private float targetRingWidthMultiplier = 1f;

    private readonly List<GradientAlphaKey> alphaKeys = new List<GradientAlphaKey>(128);
    private readonly List<GradientColorKey> colorKeys = new List<GradientColorKey>(2);

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
        var width = Mathf.Max(0f, targetingSystem.LineWidth * 2f);
        width = AdjustWidthByScale(width);

        var target = targetingSystem.GetTarget(origin, direction, length, null);
        var hasTarget = target != null;

        line.startWidth = width;
        line.endWidth = width;
        ApplyLineStyle(hasTarget, length);

        var end = origin + direction * length;
        if (hasTarget)
        {
            end = target.position;
            end.y = origin.y;
        }

        line.SetPosition(0, origin);
        line.SetPosition(1, end);
        UpdateTargetRing(target, width, hasTarget ? detectedLineColor : idleLineColor);
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

    private void ApplyLineStyle(bool hasTarget, float length)
    {
        if (line == null) return;

        var color = hasTarget ? detectedLineColor : idleLineColor;
        if (!useDottedLine)
        {
            line.colorGradient = BuildSolidGradient(color);
            return;
        }

        line.colorGradient = BuildDottedGradient(color, length);
    }

    private Gradient BuildSolidGradient(Color color)
    {
        var gradient = new Gradient();
        colorKeys.Clear();
        colorKeys.Add(new GradientColorKey(color, 0f));
        colorKeys.Add(new GradientColorKey(color, 1f));
        alphaKeys.Clear();
        alphaKeys.Add(new GradientAlphaKey(color.a, 0f));
        alphaKeys.Add(new GradientAlphaKey(color.a, 1f));
        gradient.SetKeys(colorKeys.ToArray(), alphaKeys.ToArray());
        return gradient;
    }

    private Gradient BuildDottedGradient(Color color, float length)
    {
        var gradient = new Gradient();
        var safeLength = Mathf.Max(0.01f, length);
        var transitionCount = Mathf.Clamp(Mathf.RoundToInt(safeLength * dotRepeatPerMeter), 2, 7);
        const int maxAlphaKeys = 8;

        colorKeys.Clear();
        colorKeys.Add(new GradientColorKey(color, 0f));
        colorKeys.Add(new GradientColorKey(color, 1f));

        alphaKeys.Clear();
        for (int i = 0; i < maxAlphaKeys; i++)
        {
            var t = i / (maxAlphaKeys - 1f);
            var phase = Mathf.FloorToInt(t * transitionCount);
            var alpha = phase % 2 == 0 ? color.a : 0f;
            alphaKeys.Add(new GradientAlphaKey(alpha, t));
        }

        gradient.SetKeys(colorKeys.ToArray(), alphaKeys.ToArray());
        return gradient;
    }

    private void EnsureTargetRing()
    {
        if (!useTargetRing) return;
        if (targetRing != null)
        {
            targetRing.loop = true;
            targetRing.useWorldSpace = true;
            targetRing.positionCount = Mathf.Max(8, targetRingSegments);
            targetRing.enabled = false;
            return;
        }

        var ringObject = new GameObject("TargetRing");
        ringObject.transform.SetParent(transform, false);
        targetRing = ringObject.AddComponent<LineRenderer>();
        targetRing.loop = true;
        targetRing.useWorldSpace = true;
        targetRing.positionCount = Mathf.Max(8, targetRingSegments);
        targetRing.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        targetRing.receiveShadows = false;
        targetRing.enabled = false;

        if (line != null)
        {
            targetRing.material = line.material;
            targetRing.textureMode = line.textureMode;
            targetRing.alignment = line.alignment;
            targetRing.numCapVertices = line.numCapVertices;
            targetRing.numCornerVertices = line.numCornerVertices;
        }
    }

    private void UpdateTargetRing(Transform target, float baseWidth, Color color)
    {
        if (!useTargetRing || targetRing == null) return;
        if (target == null)
        {
            targetRing.enabled = false;
            return;
        }

        var segmentCount = Mathf.Max(8, targetRingSegments);
        if (targetRing.positionCount != segmentCount)
        {
            targetRing.positionCount = segmentCount;
        }

        var center = target.position + Vector3.up * targetRingHeight;
        var step = 360f / segmentCount;
        for (int i = 0; i < segmentCount; i++)
        {
            var rad = Mathf.Deg2Rad * (i * step);
            var point = center + new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * targetRingRadius;
            targetRing.SetPosition(i, point);
        }

        var ringWidth = Mathf.Max(0.005f, baseWidth * targetRingWidthMultiplier);
        targetRing.startWidth = ringWidth;
        targetRing.endWidth = ringWidth;
        targetRing.colorGradient = BuildSolidGradient(color);
        targetRing.enabled = true;
    }
}
