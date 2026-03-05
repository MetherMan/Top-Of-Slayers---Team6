using UnityEngine;

public partial class TargetingLineVisualizer
{
    private readonly RaycastHit[] monsterRingGroundHits = new RaycastHit[12];

    private void SetMonsterRingVisible(MonsterRingEntry entry, bool visible)
    {
        if (entry == null || entry.RingRenderer == null) return;
        if (entry.RingRenderer.enabled == visible) return;
        entry.RingRenderer.enabled = visible;
    }

    private void UpdateMonsterRingTransform(MonsterRingEntry entry)
    {
        if (entry == null || entry.Target == null) return;

        if (entry.SpawnedByVisualizer && entry.RingTransform != null)
        {
            var position = ResolveMonsterRingGroundPosition(entry);
            var ringYOffset = Mathf.Max(monsterRingHeightOffset, monsterRingGroundClearance);
            entry.RingTransform.position = position + Vector3.up * ringYOffset;
        }

        UpdateMonsterLockOnTransform(entry);
    }

    private Vector3 ResolveMonsterRingGroundPosition(MonsterRingEntry entry)
    {
        var position = entry.Target.position;
        var targetBottomY = position.y;

        if (entry.TargetCollider != null)
        {
            var bounds = entry.TargetCollider.bounds;
            position = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            targetBottomY = bounds.min.y;
        }
        else if (entry.TargetBodyRenderer != null)
        {
            var bounds = entry.TargetBodyRenderer.bounds;
            position = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            targetBottomY = bounds.min.y;
        }

        if (monsterRingUseGroundRaycast && TryResolveMonsterRingGroundHit(entry, position, out var groundPoint))
        {
            position = groundPoint;
            position.y = Mathf.Max(position.y, targetBottomY);
        }

        return position;
    }

    private bool TryResolveMonsterRingGroundHit(MonsterRingEntry entry, Vector3 basePosition, out Vector3 groundPoint)
    {
        groundPoint = basePosition;

        if (entry == null || entry.Target == null)
        {
            return false;
        }

        var rayOrigin = basePosition + Vector3.up * monsterRingRaycastHeight;
        var hitCount = Physics.RaycastNonAlloc(
            rayOrigin,
            Vector3.down,
            monsterRingGroundHits,
            monsterRingRaycastDistance,
            monsterRingRaycastMask,
            QueryTriggerInteraction.Ignore);

        if (hitCount <= 0)
        {
            return false;
        }

        var found = false;
        var bestDistance = float.PositiveInfinity;
        var bestPoint = basePosition;
        for (int i = 0; i < hitCount; i++)
        {
            var hit = monsterRingGroundHits[i];
            if (!IsValidMonsterRingGroundHit(entry, hit)) continue;

            if (hit.distance >= bestDistance) continue;
            bestDistance = hit.distance;
            bestPoint = hit.point;
            found = true;
        }

        if (!found)
        {
            return false;
        }

        groundPoint = bestPoint;
        return true;
    }

    private bool IsValidMonsterRingGroundHit(MonsterRingEntry entry, RaycastHit hit)
    {
        if (hit.collider == null) return false;

        var targetRoot = entry.Target.root != null ? entry.Target.root : entry.Target;
        if (hit.collider.transform.IsChildOf(targetRoot)) return false;
        if (hit.collider.GetComponentInParent<MonsterTargetRingMarker>(true) != null) return false;
        return true;
    }

    private void ApplyMonsterRingIdleVisual(MonsterRingEntry entry)
    {
        if (entry == null) return;

        SetMonsterRingVisible(entry, true);
        UpdateMonsterRingTransform(entry);
        ApplyMonsterRingColor(entry, entry.IdleColor);
        ApplyMonsterLockOnIcon(entry, false, entry.IdleColor, 0f);
        entry.LastConfirmStage = -1;

        ApplyMonsterRingScale(entry, monsterRingBaseScaleMultiplier * (1f + entry.RingTweenScale));
    }

    private void ApplyMonsterRingHighlight(MonsterRingEntry entry, Color color, float confirmProgress, float stageFx)
    {
        if (entry == null) return;

        UpdateMonsterRingTransform(entry);

        var wave = 0f;
        if (monsterRingPulseAmount > 0f && monsterRingPulseSpeed > 0f)
        {
            wave = (Mathf.Sin(Time.unscaledTime * monsterRingPulseSpeed) + 1f) * 0.5f;
        }

        var pulse = wave * monsterRingPulseAmount * Mathf.Clamp01(confirmProgress + 0.2f);
        var boost = Mathf.Clamp01(confirmProgress + (stageFx * 0.5f));
        var stage = EvaluateConfirmStage(confirmProgress);
        if (stage > entry.LastConfirmStage && entry.LastConfirmStage >= 0)
        {
            var stageDelta = stage - entry.LastConfirmStage;
            var stagePunch = monsterRingStagePunchScale * Mathf.Clamp(stageDelta, 1, 3);
            PlayMonsterRingTween(entry, stagePunch, monsterRingStagePunchDuration);
        }

        entry.LastConfirmStage = stage;
        var rampColor = Color.Lerp(entry.IdleColor, color, monsterRingColorBlend);

        var shouldHideBaseRing = hideMonsterRingWhenLocked && useLockOnIcon;
        if (shouldHideBaseRing)
        {
            SetMonsterRingVisible(entry, false);
        }
        else
        {
            SetMonsterRingVisible(entry, true);
            ApplyMonsterRingColor(entry, rampColor);
        }

        ApplyMonsterLockOnIcon(entry, true, rampColor, confirmProgress);

        var pulseScale = 1f + pulse;
        var confirmScale = Mathf.Lerp(1f, monsterRingConfirmScaleMultiplier, boost);
        var stageScale = Mathf.Lerp(1f, monsterRingStageScaleMultiplier, stageFx);
        var dynamicScale = 1f + entry.RingTweenScale;
        var totalScale = pulseScale * confirmScale * stageScale * dynamicScale;
        ApplyMonsterRingScale(entry, monsterRingBaseScaleMultiplier * totalScale);
    }

    private void ApplyMonsterRingPreviewVisual(MonsterRingEntry entry, Color focusedColor)
    {
        if (entry == null) return;

        UpdateMonsterRingTransform(entry);
        SetMonsterRingVisible(entry, true);

        var previewColor = Color.Lerp(entry.IdleColor, nextTargetPreviewColor, Mathf.Clamp01(nextTargetPreviewColorBlend));
        previewColor = Color.Lerp(previewColor, focusedColor, 0.12f);
        ApplyMonsterRingColor(entry, previewColor);
        ApplyMonsterLockOnIcon(entry, false, previewColor, 0f);
        entry.LastConfirmStage = -1;

        var pulse = 0f;
        if (nextTargetPreviewPulseAmount > 0f && nextTargetPreviewPulseSpeed > 0f)
        {
            var wave = (Mathf.Sin((Time.unscaledTime + entry.LockOnPulseSeed) * nextTargetPreviewPulseSpeed) + 1f) * 0.5f;
            pulse = wave * nextTargetPreviewPulseAmount;
        }

        var previewScale = Mathf.Max(0.1f, nextTargetPreviewScaleMultiplier);
        var totalScale = (1f + pulse) * previewScale * (1f + entry.RingTweenScale);
        ApplyMonsterRingScale(entry, monsterRingBaseScaleMultiplier * totalScale);
    }

    private void ApplyMonsterRingColor(MonsterRingEntry entry, Color color)
    {
        if (entry == null || entry.RingRenderer == null) return;

        if (entry.RingRenderer is LineRenderer lineRenderer)
        {
            lineRenderer.colorGradient = BuildLinearGradient(color, color);
        }

        if (entry.PropertyBlock == null)
        {
            entry.PropertyBlock = new MaterialPropertyBlock();
        }

        var appliedColor = color;
        // 메쉬 링은 원본 알파를 유지해 사각형 면이 드러나는 현상을 줄인다.
        if (!(entry.RingRenderer is LineRenderer))
        {
            var idleAlpha = Mathf.Clamp01(entry.IdleColor.a);
            if (idleAlpha > 0f)
            {
                appliedColor.a = Mathf.Min(appliedColor.a, idleAlpha);
            }
        }

        entry.RingRenderer.GetPropertyBlock(entry.PropertyBlock);
        entry.PropertyBlock.SetColor(BaseColorPropertyId, appliedColor);
        entry.PropertyBlock.SetColor(ColorPropertyId, appliedColor);

        if (monsterRingEmission > 0f)
        {
            entry.PropertyBlock.SetColor(EmissionColorPropertyId, appliedColor * monsterRingEmission);
        }

        entry.RingRenderer.SetPropertyBlock(entry.PropertyBlock);
    }
}
