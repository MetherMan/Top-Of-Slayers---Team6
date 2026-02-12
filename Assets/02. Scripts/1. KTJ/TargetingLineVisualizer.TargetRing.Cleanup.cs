using DG.Tweening;
using UnityEngine;

public partial class TargetingLineVisualizer
{
    private void ReleaseMonsterExistingRing(bool forceIdleRestore)
    {
        ClearMonsterRingEntries(forceIdleRestore);
    }

    private void ClearMonsterRingEntries(bool forceIdleRestore)
    {
        foreach (var pair in monsterRingEntries)
        {
            CleanupMonsterRingEntry(pair.Value, forceIdleRestore);
        }

        monsterRingEntries.Clear();
        monsterRingTargets.Clear();
        monsterRingCleanupTargets.Clear();
        lastFocusedRingTarget = null;
    }

    private void CleanupMonsterRingEntry(MonsterRingEntry entry, bool forceIdleRestore)
    {
        if (entry == null) return;

        if (entry.RingTween != null)
        {
            entry.RingTween.Kill();
            entry.RingTween = null;
        }

        entry.RingTweenScale = 0f;
        entry.LastConfirmStage = -1;

        if (entry.RingRenderer != null && (restoreMonsterRingOnRelease || forceIdleRestore))
        {
            ApplyMonsterRingColor(entry, entry.IdleColor);
        }

        if (entry.RingTransform != null)
        {
            ApplyMonsterRingScale(entry, monsterRingBaseScaleMultiplier);
            if (entry.SpawnedByVisualizer)
            {
                Destroy(entry.RingTransform.gameObject);
            }
        }

        if (entry.LockOnTransform != null)
        {
            if (entry.SpawnedLockOnByVisualizer)
            {
                Destroy(entry.LockOnTransform.gameObject);
            }
            else
            {
                entry.LockOnTransform.gameObject.SetActive(false);
            }
        }

        if (entry.HiddenOriginalRenderer != null)
        {
            entry.HiddenOriginalRenderer.enabled = entry.HiddenOriginalRendererWasEnabled;
        }
    }
}

