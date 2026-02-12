using UnityEngine;

public partial class TargetingLineVisualizer
{
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

    private void ApplyLineStyle(Color startColor, Color endColor, float length, float worldWidth)
    {
        if (line == null) return;

        var gradient = BuildLinearGradient(startColor, endColor);
        if (!useDottedLine)
        {
            ApplySolidLineMaterial();
            line.colorGradient = gradient;
            return;
        }

        ApplyDottedLineMaterial(length, worldWidth);
        line.colorGradient = gradient;
    }

    private void OnDisable()
    {
        ReleaseMonsterExistingRing(true);
        lastVisualTarget = null;
        targetColorFillTimer = 0f;
        lastRawConfirmTarget = null;
        lastRawConfirmProgress = 0f;
        postConfirmHoldTarget = null;
        postConfirmHoldTimer = 0f;
        wasChainForcedRingColor = false;
    }

    private float GetVisualTargetFillProgress(Transform target)
    {
        if (target == null)
        {
            lastVisualTarget = null;
            targetColorFillTimer = 0f;
            return 0f;
        }

        if (lastVisualTarget != target)
        {
            lastVisualTarget = target;
            targetColorFillTimer = 0f;
        }

        var delta = Time.unscaledDeltaTime > 0f ? Time.unscaledDeltaTime : Time.deltaTime;
        if (targetColorFillDuration <= 0f) return 1f;

        var safeDelta = Mathf.Max(0f, delta);
        targetColorFillTimer = Mathf.MoveTowards(targetColorFillTimer, targetColorFillDuration, safeDelta);
        return Mathf.Clamp01(targetColorFillTimer / targetColorFillDuration);
    }

    private void UpdateColorCycleReset(Transform target, float rawConfirmProgress)
    {
        if (target == null)
        {
            lastRawConfirmTarget = null;
            lastRawConfirmProgress = 0f;
            postConfirmHoldTarget = null;
            postConfirmHoldTimer = 0f;
            return;
        }

        if (lastRawConfirmTarget != target)
        {
            lastRawConfirmTarget = target;
            lastRawConfirmProgress = rawConfirmProgress;
            postConfirmHoldTarget = null;
            postConfirmHoldTimer = 0f;
            return;
        }

        // 공격 확정 직후 진행도가 급락하면 짧게 빨강 상태를 유지해 점멸을 줄인다.
        var wasReady = lastRawConfirmProgress >= 0.85f;
        var droppedToStart = rawConfirmProgress <= 0.1f;
        if (wasReady && droppedToStart)
        {
            if (postConfirmColorHoldDuration > 0f)
            {
                postConfirmHoldTarget = target;
                postConfirmHoldTimer = postConfirmColorHoldDuration;
            }
            else
            {
                confirmProgressVisual = 0f;
                targetColorFillTimer = 0f;
            }
        }

        lastRawConfirmProgress = rawConfirmProgress;
    }

    private float ApplyPostConfirmColorHold(Transform target, float rawConfirmProgress)
    {
        if (target == null) return rawConfirmProgress;
        if (postConfirmHoldTimer <= 0f) return rawConfirmProgress;
        if (postConfirmHoldTarget != target) return rawConfirmProgress;
        if (IsChainRingForcedColorActive()) return rawConfirmProgress;

        var delta = Time.unscaledDeltaTime > 0f ? Time.unscaledDeltaTime : Time.deltaTime;
        postConfirmHoldTimer = Mathf.Max(0f, postConfirmHoldTimer - Mathf.Max(0f, delta));
        if (postConfirmHoldTimer <= 0f)
        {
            postConfirmHoldTarget = null;
            return rawConfirmProgress;
        }

        if (rawConfirmProgress > 0.1f)
        {
            postConfirmHoldTimer = 0f;
            postConfirmHoldTarget = null;
            return rawConfirmProgress;
        }

        return 1f;
    }

    private float ApplyColorLeadProgress(float progress)
    {
        var clamped = Mathf.Clamp01(progress);
        if (!useTargetConfirmFx) return clamped;

        var earlyWindow = Mathf.Clamp(confirmColorEarlyRedWindow, 0f, 0.5f);
        if (earlyWindow > 0f)
        {
            var maxBeforeAttack = Mathf.Max(0.001f, 1f - earlyWindow);
            clamped = Mathf.Clamp01(clamped / maxBeforeAttack);
        }

        var lead = Mathf.Clamp01(confirmColorLeadProgress);
        if (lead <= 0f) return clamped;

        // 진행도가 높을수록 선행 비율을 줄여 과도한 즉시 확정을 방지한다.
        return Mathf.Clamp01(clamped + ((1f - clamped) * lead));
    }
}
