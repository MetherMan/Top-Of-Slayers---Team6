using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public sealed class MonsterTargetRingMarker : MonoBehaviour
{
}

public partial class TargetingLineVisualizer
{
    private sealed class MonsterRingEntry
    {
        public Transform Target;
        public Transform RingTransform;
        public Renderer RingRenderer;
        public Renderer HiddenOriginalRenderer;
        public bool HiddenOriginalRendererWasEnabled;
        public MaterialPropertyBlock PropertyBlock;
        public Collider TargetCollider;
        public Renderer TargetBodyRenderer;
        public bool SpawnedByVisualizer;
        public Color IdleColor;
        public Vector3 BaseScale;
        public Transform LockOnTransform;
        public SpriteRenderer LockOnSpriteRenderer;
        public bool SpawnedLockOnByVisualizer;
        public float LockOnVisibleLerp;
        public float LockOnPulseSeed;
        public float LockOnSpinAngle;
        public float RingTweenScale;
        public int LastConfirmStage;
        public Tween RingTween;
    }

    private static readonly string[] MonsterRingNameHints =
    {
        "ring",
        "circle",
        "target",
        "marker",
        "indicator",
        "lock"
    };

    private static Sprite fallbackLockOnSprite;
    private static Texture2D fallbackLockOnTexture;
    private Transform lastFocusedRingTarget;
    private const float MonsterRingMaxLocalScale = 3f;

    private void UpdateMonsterExistingRing(Transform target, Color color, float confirmProgress, float stageFx)
    {
        if (!useMonsterExistingRingFx || targetingSystem == null)
        {
            ClearMonsterRingEntries(false);
            lastRawConfirmTarget = null;
            lastRawConfirmProgress = 0f;
            wasChainForcedRingColor = false;
            lastFocusedRingTarget = null;
            return;
        }

        var useChainForcedColor = IsChainRingForcedColorActive();
        if (useChainForcedColor != wasChainForcedRingColor)
        {
            if (!useChainForcedColor)
            {
                // 체인 종료 직후에는 타겟 컬러링 상태를 초기화해 다음 겨냥 연출을 다시 시작한다.
                lastVisualTarget = null;
                targetColorFillTimer = 0f;
                lastRawConfirmTarget = null;
                lastRawConfirmProgress = 0f;
                confirmProgressVisual = 0f;
                confirmStage = -1;
                confirmStageKickTimer = 0f;
            }

            wasChainForcedRingColor = useChainForcedColor;
        }

        SyncMonsterRingEntries(target, useChainForcedColor);
        UpdateFocusedRingTarget(target);

        if (useChainForcedColor)
        {
            ApplyChainForcedRingVisual(target);
            return;
        }

        if (target == null) return;
        if (!monsterRingEntries.TryGetValue(target, out var entry)) return;
        if (entry == null || entry.RingRenderer == null) return;

        ApplyMonsterRingHighlight(entry, color, confirmProgress, stageFx);
    }

    private void SyncMonsterRingEntries(Transform focusedTarget, bool skipIdleVisual)
    {
        monsterRingTargets.Clear();

        if (showMonsterRingForAllTargets)
        {
            targetingSystem.GetTargetsSnapshot(monsterRingTargets);
        }

        // 스냅샷 누락을 대비해 포커스 타겟은 항상 보강한다.
        if (focusedTarget != null && !monsterRingTargets.Contains(focusedTarget))
        {
            monsterRingTargets.Add(focusedTarget);
        }

        for (int i = 0; i < monsterRingTargets.Count; i++)
        {
            var target = monsterRingTargets[i];
            if (target == null) continue;
            if (!EnsureMonsterRingEntry(target, out var entry)) continue;
            EnsureEntryUsesSafeRingRenderer(entry);

            UpdateMonsterRingTransform(entry);
            if (!skipIdleVisual && target != focusedTarget)
            {
                ApplyMonsterRingIdleVisual(entry);
            }
        }

        foreach (var pair in monsterRingEntries)
        {
            var target = pair.Key;
            if (target == null || !monsterRingTargets.Contains(target))
            {
                monsterRingCleanupTargets.Add(target);
            }
        }

        for (int i = 0; i < monsterRingCleanupTargets.Count; i++)
        {
            var target = monsterRingCleanupTargets[i];
            if (target != null && monsterRingEntries.TryGetValue(target, out var entry))
            {
                CleanupMonsterRingEntry(entry, false);
                monsterRingEntries.Remove(target);
                continue;
            }

            monsterRingEntries.Remove(target);
        }

        monsterRingCleanupTargets.Clear();
    }

    private bool IsChainRingForcedColorActive()
    {
        if (!forceRedRingDuringChain) return false;
        if (autoSlash == null) return false;
        return autoSlash.IsChainSlowActive;
    }

    private void ApplyChainForcedRingVisual(Transform focusedTarget)
    {
        foreach (var pair in monsterRingEntries)
        {
            var target = pair.Key;
            var entry = pair.Value;
            if (entry == null || entry.RingRenderer == null) continue;

            UpdateMonsterRingTransform(entry);
            ApplyMonsterRingColor(entry, chainForcedRingColor);
            ApplyMonsterLockOnIcon(entry, target == focusedTarget, chainForcedRingColor, 1f);
            if (entry.RingTransform != null)
            {
                ApplyMonsterRingScale(entry, monsterRingBaseScaleMultiplier * (1f + entry.RingTweenScale));
            }
        }
    }

    private void UpdateFocusedRingTarget(Transform focusedTarget)
    {
        if (focusedTarget == lastFocusedRingTarget) return;

        if (focusedTarget != null && monsterRingEntries.TryGetValue(focusedTarget, out var entry))
        {
            PlayMonsterRingTween(entry, monsterRingLockOnPunchScale, monsterRingLockOnPunchDuration);
        }

        lastFocusedRingTarget = focusedTarget;
    }

    private void PlayMonsterRingTween(MonsterRingEntry entry, float punchScale, float duration)
    {
        if (!useMonsterRingDynamicTween) return;
        if (entry == null || entry.RingTransform == null) return;
        if (punchScale <= 0f || duration <= 0f) return;

        if (entry.RingTween != null)
        {
            entry.RingTween.Kill();
            entry.RingTween = null;
        }

        entry.RingTweenScale = 0f;
        var upDuration = Mathf.Max(0.01f, duration * 0.45f);
        var downDuration = Mathf.Max(0.01f, duration - upDuration);
        var sequence = DOTween.Sequence().SetUpdate(true);
        sequence.Append(DOTween.To(() => entry.RingTweenScale, value => entry.RingTweenScale = value, punchScale, upDuration).SetEase(Ease.OutBack));
        sequence.Append(DOTween.To(() => entry.RingTweenScale, value => entry.RingTweenScale = value, 0f, downDuration).SetEase(Ease.InSine));
        sequence.OnKill(() =>
        {
            entry.RingTweenScale = 0f;
            entry.RingTween = null;
        });
        entry.RingTween = sequence;
    }

    private void ApplyMonsterRingScale(MonsterRingEntry entry, float scaleMultiplier)
    {
        if (entry == null || entry.RingTransform == null) return;

        var scaled = entry.BaseScale * Mathf.Max(0.01f, scaleMultiplier);
        entry.RingTransform.localScale = ClampMonsterRingLocalScale(scaled);
    }

    private Vector3 ClampMonsterRingLocalScale(Vector3 scale)
    {
        return new Vector3(
            Mathf.Clamp(scale.x, 0.01f, MonsterRingMaxLocalScale),
            Mathf.Clamp(scale.y, 0.01f, MonsterRingMaxLocalScale),
            Mathf.Clamp(scale.z, 0.01f, MonsterRingMaxLocalScale)
        );
    }
}
