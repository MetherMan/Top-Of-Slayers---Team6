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

        var length = GetLineLength();
        var width = Mathf.Max(0f, targetingSystem.LineWidth * 2f);
        width = AdjustWidthByScale(width);

        line.startWidth = width;
        line.endWidth = width;
        line.SetPosition(0, origin);
        line.SetPosition(1, origin + direction.normalized * length);
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
            var attackRange = autoSlash.AttackRange;
            if (attackRange > 0f) return attackRange;
        }

        if (useDashRange && dashController != null)
        {
            var dashRange = dashController.DefaultDashDistance;
            if (dashRange > 0f) return dashRange;
        }

        return Mathf.Max(0f, targetingSystem.MaxRange);
    }
}
