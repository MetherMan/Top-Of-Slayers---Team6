using System;
using UnityEngine;

public partial class TargetingLineVisualizer
{
    private Renderer FindMonsterRingRenderer(Transform root)
    {
        if (root == null) return null;

        var renderers = root.GetComponentsInChildren<Renderer>(true);

        var namedRing = FindNamedMonsterRing(renderers);
        if (namedRing != null)
        {
            return namedRing;
        }

        return FindGroundMonsterRing(renderers, root.position.y);
    }

    private Renderer FindNamedMonsterRing(Renderer[] renderers)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            var renderer = renderers[i];
            if (!IsUsableMonsterRingRenderer(renderer)) continue;
            if (!HasRingNameHint(renderer.name)) continue;
            return renderer;
        }

        return null;
    }

    private Renderer FindGroundMonsterRing(Renderer[] renderers, float referenceY)
    {
        Renderer closest = null;
        var bestHeightGap = float.PositiveInfinity;
        var bestArea = 0f;

        for (int i = 0; i < renderers.Length; i++)
        {
            var renderer = renderers[i];
            if (!IsUsableMonsterRingRenderer(renderer)) continue;

            var bounds = renderer.bounds;
            var heightGap = Mathf.Abs(bounds.center.y - referenceY);
            var area = bounds.size.x * bounds.size.z;

            if (closest == null)
            {
                closest = renderer;
                bestHeightGap = heightGap;
                bestArea = area;
                continue;
            }

            var clearlyCloserToFeet = heightGap < bestHeightGap - 0.05f;
            var sameHeightBand = Mathf.Abs(heightGap - bestHeightGap) <= 0.05f;
            if (!clearlyCloserToFeet && !(sameHeightBand && area > bestArea))
            {
                continue;
            }

            closest = renderer;
            bestHeightGap = heightGap;
            bestArea = area;
        }

        return closest;
    }

    private bool IsUsableMonsterRingRenderer(Renderer renderer)
    {
        if (renderer == null) return false;
        if (renderer is ParticleSystemRenderer) return false;
        if (renderer is LineRenderer) return false;
        if (renderer is TrailRenderer) return false;
        if (renderer is SkinnedMeshRenderer) return false;

        var bounds = renderer.bounds;
        var size = bounds.size;
        if (size.x < 0.05f || size.z < 0.05f) return false;

        var maxHorizontal = Mathf.Max(size.x, size.z);
        if (maxHorizontal <= 0f) return false;

        var flatness = size.y / maxHorizontal;
        return flatness <= 0.45f;
    }

    private bool HasRingNameHint(string nameText)
    {
        if (string.IsNullOrEmpty(nameText)) return false;

        for (int i = 0; i < MonsterRingNameHints.Length; i++)
        {
            var hint = MonsterRingNameHints[i];
            if (string.IsNullOrEmpty(hint)) continue;
            if (nameText.IndexOf(hint, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }

    private Color ResolveMonsterRingIdleColor(Renderer renderer)
    {
        var fallback = monsterRingIdleFallbackColor;
        var alpha = fallback.a;

        if (renderer != null)
        {
            var material = renderer.sharedMaterial;
            if (material != null)
            {
                if (material.HasProperty(BaseColorPropertyId))
                {
                    alpha = Mathf.Clamp01(material.GetColor(BaseColorPropertyId).a);
                }
                else if (material.HasProperty(ColorPropertyId))
                {
                    alpha = Mathf.Clamp01(material.GetColor(ColorPropertyId).a);
                }
            }
        }

        if (alpha <= 0f) alpha = 1f;
        return new Color(fallback.r, fallback.g, fallback.b, alpha);
    }
}
