using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public partial class ChainVisualController
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    private readonly List<EnvironmentDarkenEntry> environmentDarkenEntries = new List<EnvironmentDarkenEntry>(128);
    private MaterialPropertyBlock environmentDarkenBlock;

    private Tween environmentDarkenTween;
    private float environmentDarkenLerp;

    private sealed class EnvironmentDarkenEntry
    {
        public Renderer Renderer;
        public int MaterialIndex;
        public int ColorPropertyId;
        public Color BaseColor;
    }

    private bool ShouldUseEnvironmentDarken(bool isActive)
    {
        if (!useEnvironmentDarken) return false;
        if (isActive)
        {
            RebuildEnvironmentDarkenEntries();
        }

        return environmentDarkenEntries.Count > 0;
    }

    private void PlayEnvironmentDarken(bool isActive)
    {
        KillEnvironmentTween();
        if (environmentDarkenEntries.Count <= 0)
        {
            environmentDarkenLerp = 0f;
            return;
        }

        var target = isActive ? 1f : 0f;
        if (darkenFadeTime <= 0f)
        {
            SetEnvironmentDarkenLerp(target);
            return;
        }

        environmentDarkenTween = DOTween
            .To(() => environmentDarkenLerp, SetEnvironmentDarkenLerp, target, darkenFadeTime)
            .SetEase(darkenFadeEase)
            .SetUpdate(useUnscaledTime);
    }

    private void ResetEnvironmentDarkenImmediate()
    {
        KillEnvironmentTween();
        SetEnvironmentDarkenLerp(0f);
    }

    private void KillEnvironmentTween()
    {
        if (environmentDarkenTween != null) environmentDarkenTween.Kill();
        environmentDarkenTween = null;
    }

    private void SetEnvironmentDarkenLerp(float value)
    {
        environmentDarkenLerp = Mathf.Clamp01(value);
        ApplyEnvironmentDarken(environmentDarkenLerp);
    }

    private void ApplyEnvironmentDarken(float value)
    {
        if (environmentDarkenEntries.Count <= 0) return;
        EnsureEnvironmentDarkenBlock();

        var tint = Mathf.Clamp01(1f - environmentDarkenStrength * value);
        for (int i = 0; i < environmentDarkenEntries.Count; i++)
        {
            var entry = environmentDarkenEntries[i];
            if (entry.Renderer == null) continue;

            var color = entry.BaseColor;
            color.r *= tint;
            color.g *= tint;
            color.b *= tint;

            entry.Renderer.GetPropertyBlock(environmentDarkenBlock, entry.MaterialIndex);
            environmentDarkenBlock.SetColor(entry.ColorPropertyId, color);
            entry.Renderer.SetPropertyBlock(environmentDarkenBlock, entry.MaterialIndex);
        }
    }

    private void RebuildEnvironmentDarkenEntries()
    {
        environmentDarkenEntries.Clear();

        var renderers = FindObjectsOfType<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            var renderer = renderers[i];
            if (!IsEnvironmentDarkenTarget(renderer)) continue;

            var sharedMaterials = renderer.sharedMaterials;
            if (sharedMaterials == null || sharedMaterials.Length == 0) continue;

            for (int materialIndex = 0; materialIndex < sharedMaterials.Length; materialIndex++)
            {
                var material = sharedMaterials[materialIndex];
                if (!TryResolveColorProperty(material, out var colorPropertyId, out var baseColor)) continue;

                environmentDarkenEntries.Add(new EnvironmentDarkenEntry
                {
                    Renderer = renderer,
                    MaterialIndex = materialIndex,
                    ColorPropertyId = colorPropertyId,
                    BaseColor = baseColor
                });
            }
        }
    }

    private bool IsEnvironmentDarkenTarget(Renderer renderer)
    {
        if (renderer == null) return false;
        if (!renderer.enabled) return false;
        if (renderer == darkenSprite) return false;
        if (!renderer.gameObject.activeInHierarchy) return false;
        if (((1 << renderer.gameObject.layer) & environmentLayerMask.value) == 0) return false;
        if (environmentOnlyStaticRenderers && !renderer.gameObject.isStatic) return false;

        if (renderer is LineRenderer) return false;
        if (renderer is TrailRenderer) return false;
        if (renderer is ParticleSystemRenderer) return false;

        if (renderer.transform.IsChildOf(transform)) return false;
        if (renderer.GetComponentInParent<Canvas>(true) != null) return false;
        if (renderer.GetComponentInParent<PlayerMoveController>(true) != null) return false;
        if (renderer.GetComponentInParent<ChainCombatController>(true) != null) return false;
        if (renderer.GetComponentInParent<DamageSystem>(true) != null) return false;
        if (renderer.GetComponentInParent<TargetingRegistrant>(true) != null) return false;
        return true;
    }

    private bool TryResolveColorProperty(Material material, out int colorPropertyId, out Color baseColor)
    {
        colorPropertyId = 0;
        baseColor = Color.white;
        if (material == null) return false;

        if (material.HasProperty(BaseColorId))
        {
            colorPropertyId = BaseColorId;
            baseColor = material.GetColor(BaseColorId);
            return true;
        }

        if (material.HasProperty(ColorId))
        {
            colorPropertyId = ColorId;
            baseColor = material.GetColor(ColorId);
            return true;
        }

        return false;
    }

    private void EnsureEnvironmentDarkenBlock()
    {
        if (environmentDarkenBlock != null) return;
        environmentDarkenBlock = new MaterialPropertyBlock();
    }
}
