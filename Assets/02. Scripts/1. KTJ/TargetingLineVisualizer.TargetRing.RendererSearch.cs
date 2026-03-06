using System;
using UnityEngine;

public partial class TargetingLineVisualizer
{
    private Renderer FindMonsterRingRenderer(Transform root)
    {
        if (root == null) return null;

        var candidates = root.GetComponentsInChildren<Renderer>(true);
        Renderer best = null;
        var bestScore = float.NegativeInfinity;
        var referenceY = root.position.y;

        for (int i = 0; i < candidates.Length; i++)
        {
            var renderer = candidates[i];
            if (!IsValidMonsterRingRenderer(renderer)) continue;

            var score = EvaluateMonsterRingScore(renderer, referenceY);
            if (score <= bestScore) continue;

            best = renderer;
            bestScore = score;
        }

        return best;
    }

    private bool IsValidMonsterRingRenderer(Renderer renderer)
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

    private float EvaluateMonsterRingScore(Renderer renderer, float referenceY)
    {
        var bounds = renderer.bounds;
        var size = bounds.size;

        var maxHorizontal = Mathf.Max(0.001f, Mathf.Max(size.x, size.z));
        var flatness = size.y / maxHorizontal;
        var area = size.x * size.z;
        var heightPenalty = Mathf.Abs(bounds.center.y - referenceY);

        var score = area * 0.4f;
        score += (1f - Mathf.Clamp01(flatness)) * 2f;
        score -= Mathf.Clamp(heightPenalty, 0f, 3f);
        if (HasRingNameHint(renderer.name))
        {
            score += 3f;
        }

        return score;
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
