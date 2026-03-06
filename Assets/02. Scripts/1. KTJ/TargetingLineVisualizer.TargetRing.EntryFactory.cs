using UnityEngine;

public partial class TargetingLineVisualizer
{
    private bool EnsureMonsterRingEntry(Transform target, out MonsterRingEntry entry)
    {
        entry = null;
        if (target == null) return false;

        if (monsterRingEntries.TryGetValue(target, out entry))
        {
            if (entry != null && entry.RingRenderer != null)
            {
                EnsureLineRingRenderer(entry);
                return true;
            }

            monsterRingEntries.Remove(target);
        }

        if (!CreateMonsterRingEntry(target, out entry))
        {
            return false;
        }

        monsterRingEntries[target] = entry;
        return true;
    }

    private void EnsureLineRingRenderer(MonsterRingEntry entry)
    {
        if (entry == null || entry.RingRenderer == null) return;
        if (entry.RingRenderer is LineRenderer) return;

        var lineRing = CreateLineRingRenderer();
        if (lineRing == null) return;

        if (entry.HiddenOriginalRenderer == null)
        {
            entry.HiddenOriginalRenderer = entry.RingRenderer;
            entry.HiddenOriginalRendererWasEnabled = entry.RingRenderer.enabled;
        }

        entry.RingRenderer.enabled = false;
        entry.RingRenderer = lineRing;
        entry.RingTransform = lineRing.transform;
        entry.SpawnedByVisualizer = true;
        entry.BaseScale = ClampMonsterRingLocalScale(entry.BaseScale);
        entry.RingTransform.localScale = entry.BaseScale;
        UpdateMonsterRingTransform(entry);
        ApplyMonsterRingColor(entry, entry.IdleColor);
    }

    private bool CreateMonsterRingEntry(Transform target, out MonsterRingEntry entry)
    {
        entry = null;

        var searchRoot = target.root != null ? target.root : target;
        var ringRenderer = FindMonsterRingRenderer(searchRoot);
        var spawnedByVisualizer = false;
        Renderer hiddenOriginalRenderer = null;
        var hiddenOriginalRendererWasEnabled = false;

        if (ringRenderer == null)
        {
            ringRenderer = SpawnMonsterRingPrefab();
            spawnedByVisualizer = ringRenderer != null;
        }

        if (ringRenderer == null || !(ringRenderer is LineRenderer))
        {
            if (ringRenderer != null && !spawnedByVisualizer)
            {
                hiddenOriginalRenderer = ringRenderer;
                hiddenOriginalRendererWasEnabled = ringRenderer.enabled;
                ringRenderer.enabled = false;
            }
            else if (ringRenderer != null)
            {
                Destroy(ringRenderer.gameObject);
            }

            ringRenderer = CreateLineRingRenderer();
            if (ringRenderer == null) return false;
            spawnedByVisualizer = true;
        }

        var ringTransform = ringRenderer.transform;
        EnsureMonsterRingMarker(ringTransform);
        var targetCollider = target.GetComponentInChildren<Collider>(true);
        var targetBodyRenderer = FindTargetBodyRenderer(target, ringRenderer);

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

    private Renderer SpawnMonsterRingPrefab()
    {
        if (monsterRingPrefab == null) return null;

        var ringObject = Instantiate(monsterRingPrefab);
        ringObject.name = "MonsterTargetRing";

        var ringRenderer = ringObject.GetComponentInChildren<Renderer>(true);
        if (ringRenderer != null)
        {
            return ringRenderer;
        }

        Destroy(ringObject);
        return null;
    }

    private void EnsureMonsterRingMarker(Transform ringTransform)
    {
        if (ringTransform == null) return;
        if (ringTransform.GetComponent<MonsterTargetRingMarker>() != null) return;
        ringTransform.gameObject.AddComponent<MonsterTargetRingMarker>();
    }

    private Renderer CreateLineRingRenderer()
    {
        const int segmentCount = 40;
        var ringObject = new GameObject("MonsterTargetRing_Line");
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

    private Renderer FindTargetBodyRenderer(Transform target, Renderer ignoreRenderer)
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
