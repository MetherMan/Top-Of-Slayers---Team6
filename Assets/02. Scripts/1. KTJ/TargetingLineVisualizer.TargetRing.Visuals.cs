using UnityEngine;

public partial class TargetingLineVisualizer
{
    private void UpdateMonsterRingTransform(MonsterRingEntry entry)
    {
        if (entry == null || entry.Target == null) return;

        if (entry.SpawnedByVisualizer && entry.RingTransform != null)
        {
            var position = ResolveMonsterRingGroundPosition(entry);
            entry.RingTransform.position = position + Vector3.up * monsterRingHeightOffset;
        }

        UpdateMonsterLockOnTransform(entry);
    }

    private Vector3 ResolveMonsterRingGroundPosition(MonsterRingEntry entry)
    {
        var position = entry.Target.position;

        if (entry.TargetCollider != null)
        {
            var bounds = entry.TargetCollider.bounds;
            position = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        }
        else if (entry.TargetBodyRenderer != null)
        {
            var bounds = entry.TargetBodyRenderer.bounds;
            position = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        }

        if (monsterRingUseGroundRaycast)
        {
            var rayOrigin = position + Vector3.up * monsterRingRaycastHeight;
            if (Physics.Raycast(rayOrigin, Vector3.down, out var hit, monsterRingRaycastDistance, monsterRingRaycastMask, QueryTriggerInteraction.Ignore))
            {
                position = hit.point;
            }
        }

        return position;
    }

    private void ApplyMonsterRingIdleVisual(MonsterRingEntry entry)
    {
        if (entry == null) return;

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
        ApplyMonsterRingColor(entry, rampColor);
        ApplyMonsterLockOnIcon(entry, true, rampColor, confirmProgress);

        var pulseScale = 1f + pulse;
        var confirmScale = Mathf.Lerp(1f, monsterRingConfirmScaleMultiplier, boost);
        var stageScale = Mathf.Lerp(1f, monsterRingStageScaleMultiplier, stageFx);
        var dynamicScale = 1f + entry.RingTweenScale;
        var totalScale = pulseScale * confirmScale * stageScale * dynamicScale;
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
