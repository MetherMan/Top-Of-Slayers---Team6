using UnityEngine;

public partial class TargetingLineVisualizer
{
    private bool EnsureMonsterRingEntry(Transform target, out MonsterRingEntry entry)
    {
        if (monsterRingEntries.TryGetValue(target, out entry))
        {
            if (entry != null && entry.RingRenderer != null)
            {
                EnsureEntryUsesSafeRingRenderer(entry);
                return true;
            }

            monsterRingEntries.Remove(target);
        }

        if (!TryCreateMonsterRingEntry(target, out entry))
        {
            return false;
        }

        monsterRingEntries[target] = entry;
        return true;
    }

    private void EnsureEntryUsesSafeRingRenderer(MonsterRingEntry entry)
    {
        if (entry == null || entry.RingRenderer == null) return;
        if (entry.RingRenderer is LineRenderer) return;

        var fallbackRingRenderer = CreateFallbackRingRenderer();
        if (fallbackRingRenderer == null) return;

        if (entry.HiddenOriginalRenderer == null)
        {
            entry.HiddenOriginalRenderer = entry.RingRenderer;
            entry.HiddenOriginalRendererWasEnabled = entry.RingRenderer.enabled;
        }

        entry.RingRenderer.enabled = false;
        entry.RingRenderer = fallbackRingRenderer;
        entry.RingTransform = fallbackRingRenderer.transform;
        entry.SpawnedByVisualizer = true;
        entry.BaseScale = ClampMonsterRingLocalScale(entry.BaseScale);
        entry.RingTransform.localScale = entry.BaseScale;
        UpdateMonsterRingTransform(entry);
        ApplyMonsterRingColor(entry, entry.IdleColor);
    }

    private bool TryCreateMonsterRingEntry(Transform target, out MonsterRingEntry entry)
    {
        entry = null;
        if (target == null) return false;

        var searchRoot = target.root != null ? target.root : target;
        var ringRenderer = FindMonsterRingRenderer(searchRoot);
        var hasExistingRingRenderer = ringRenderer != null;
        var spawnedByVisualizer = false;

        if (ringRenderer == null && monsterRingPrefab != null)
        {
            var ringObject = Instantiate(monsterRingPrefab);
            ringObject.name = "MonsterTargetRing";
            ringRenderer = ringObject.GetComponentInChildren<Renderer>(true);
            if (ringRenderer == null)
            {
                Destroy(ringObject);
                return false;
            }

            spawnedByVisualizer = true;
        }

        if (ringRenderer == null)
        {
            ringRenderer = CreateFallbackRingRenderer();
            if (ringRenderer == null) return false;
            spawnedByVisualizer = true;
        }

        Renderer hiddenOriginalRenderer = null;
        var hiddenOriginalRendererWasEnabled = false;

        // 메쉬 기반 링은 체인 컬러링 시 사각형 아티팩트가 잦아 라인 링으로 대체한다.
        if (!(ringRenderer is LineRenderer))
        {
            var fallbackRingRenderer = CreateFallbackRingRenderer();
            if (fallbackRingRenderer != null)
            {
                if (!spawnedByVisualizer && hasExistingRingRenderer)
                {
                    hiddenOriginalRenderer = ringRenderer;
                    hiddenOriginalRendererWasEnabled = ringRenderer.enabled;
                    ringRenderer.enabled = false;
                }
                else if (spawnedByVisualizer && ringRenderer != null)
                {
                    Destroy(ringRenderer.gameObject);
                }

                ringRenderer = fallbackRingRenderer;
                spawnedByVisualizer = true;
            }
        }

        var ringTransform = ringRenderer.transform;
        EnsureMonsterRingMarker(ringTransform);
        var targetCollider = target.GetComponentInChildren<Collider>(true);
        var targetBodyRenderer = ResolveTargetBodyRenderer(target, ringRenderer);

        entry = new MonsterRingEntry
        {
            Target = target,
            RingTransform = ringTransform,
            RingRenderer = ringRenderer,
            HiddenOriginalRenderer = hiddenOriginalRenderer,
            HiddenOriginalRendererWasEnabled = hiddenOriginalRendererWasEnabled,
            PropertyBlock = new MaterialPropertyBlock(),
            TargetCollider = targetCollider,
            TargetBodyRenderer = targetBodyRenderer,
            SpawnedByVisualizer = spawnedByVisualizer,
            IdleColor = ResolveMonsterRingIdleColor(ringRenderer),
            BaseScale = ClampMonsterRingLocalScale(ringTransform.localScale),
            LockOnVisibleLerp = 0f,
            LockOnPulseSeed = Random.Range(0f, 10f),
            LockOnSpinAngle = Random.Range(0f, 360f),
            RingTweenScale = 0f,
            LastConfirmStage = -1,
            RingTween = null
        };

        ringTransform.localScale = entry.BaseScale;

        if (spawnedByVisualizer)
        {
            var scale = CalculateSpawnedRingScale(targetCollider, targetBodyRenderer);
            entry.BaseScale = Vector3.Scale(monsterRingBaseScale, new Vector3(scale, scale, scale));
            ringTransform.localScale = ClampMonsterRingLocalScale(entry.BaseScale);
            UpdateMonsterRingTransform(entry);
        }

        EnsureMonsterLockOnIcon(entry);
        ApplyMonsterRingColor(entry, entry.IdleColor);
        return true;
    }

    private void EnsureMonsterRingMarker(Transform ringTransform)
    {
        if (ringTransform == null) return;
        if (ringTransform.GetComponent<MonsterTargetRingMarker>() != null) return;
        ringTransform.gameObject.AddComponent<MonsterTargetRingMarker>();
    }

    private Renderer CreateFallbackRingRenderer()
    {
        const int segmentCount = 40;
        var ringObject = new GameObject("MonsterTargetRing_Fallback");
        var lineRenderer = ringObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.loop = true;
        lineRenderer.positionCount = segmentCount;
        lineRenderer.startWidth = 0.08f;
        lineRenderer.endWidth = 0.08f;
        lineRenderer.textureMode = LineTextureMode.Stretch;

        var shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            lineRenderer.material = new Material(shader);
        }

        var step = 360f / segmentCount;
        for (int i = 0; i < segmentCount; i++)
        {
            var rad = Mathf.Deg2Rad * (i * step);
            var point = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad));
            lineRenderer.SetPosition(i, point);
        }

        return lineRenderer;
    }

    private Renderer ResolveTargetBodyRenderer(Transform target, Renderer ignoreRenderer)
    {
        var renderers = target.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            var renderer = renderers[i];
            if (renderer == null) continue;
            if (renderer == ignoreRenderer) continue;
            if (renderer is ParticleSystemRenderer) continue;
            if (renderer is LineRenderer) continue;
            if (renderer is TrailRenderer) continue;
            return renderer;
        }

        return null;
    }

    private float CalculateSpawnedRingScale(Collider targetCollider, Renderer targetBodyRenderer)
    {
        var radius = 0.6f;

        if (targetCollider != null)
        {
            var extents = targetCollider.bounds.extents;
            radius = Mathf.Max(extents.x, extents.z);
        }
        else if (targetBodyRenderer != null)
        {
            var extents = targetBodyRenderer.bounds.extents;
            radius = Mathf.Max(extents.x, extents.z);
        }

        radius = Mathf.Max(0.2f, radius);
        return radius * Mathf.Max(0f, monsterRingScaleByTargetRadius);
    }

}
