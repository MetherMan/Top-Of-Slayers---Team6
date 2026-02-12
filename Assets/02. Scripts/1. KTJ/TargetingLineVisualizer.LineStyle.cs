using UnityEngine;
public partial class TargetingLineVisualizer
{
    private Gradient BuildLinearGradient(Color startColor, Color endColor)
    {
        var gradient = new Gradient();
        colorKeys.Clear();
        colorKeys.Add(new GradientColorKey(startColor, 0f));
        colorKeys.Add(new GradientColorKey(endColor, 1f));
        alphaKeys.Clear();
        alphaKeys.Add(new GradientAlphaKey(startColor.a, 0f));
        alphaKeys.Add(new GradientAlphaKey(endColor.a, 1f));
        gradient.SetKeys(colorKeys.ToArray(), alphaKeys.ToArray());
        return gradient;
    }
    private void EnsureDefaultLineMaterial(LineRenderer renderer)
    {
        if (renderer == null) return;
        if (renderer.sharedMaterial != null) return;
        var shader = Shader.Find("Sprites/Default");
        if (shader == null) return;
        renderer.material = new Material(shader);
    }
    private void ApplySolidLineMaterial()
    {
        if (line == null) return;
        if (solidLineMaterial == null)
        {
            EnsureDefaultLineMaterial(line);
            solidLineMaterial = line.sharedMaterial;
        }
        if (solidLineMaterial != null && line.sharedMaterial != solidLineMaterial)
        {
            line.sharedMaterial = solidLineMaterial;
        }
        line.textureMode = LineTextureMode.Stretch;
    }
    private void ApplyDottedLineMaterial(float length, float worldWidth)
    {
        if (line == null) return;
        EnsureDottedLineMaterial();
        if (dottedLineMaterial == null)
        {
            ApplySolidLineMaterial();
            return;
        }
        if (line.sharedMaterial != dottedLineMaterial)
        {
            line.sharedMaterial = dottedLineMaterial;
        }
        // 월드 두께 기준으로 타일 길이를 제한해 점이 길쭉해지는 현상을 방지한다.
        var safeWidth = Mathf.Max(minLineWidth, worldWidth);
        var minAllowed = safeWidth * 0.85f;
        var maxAllowed = safeWidth * 1.15f;
        var density = Mathf.Clamp(dotRepeatPerMeter, 0.8f, 1.2f);
        var tileLength = safeWidth * Mathf.Clamp(dotTileSizeMultiplier / density, 0.9f, 1.1f);
        var userMin = Mathf.Clamp(dotMinWorldSize, minAllowed, maxAllowed);
        tileLength = Mathf.Clamp(tileLength, userMin, maxAllowed);
        tileLength = Mathf.Max(0.005f, tileLength);
        var repeat = Mathf.Clamp(length / tileLength, 1f, 240f);
        line.textureMode = LineTextureMode.Tile;
        line.sharedMaterial.mainTextureScale = new Vector2(repeat, 1f);
    }
    private void EnsureDottedLineMaterial()
    {
        if (dottedLineMaterial != null && dottedTexture != null) return;
        var shader = Shader.Find("Sprites/Default");
        if (shader == null) return;
        if (dottedTexture == null)
        {
            dottedTexture = BuildDottedTexture();
        }
        if (dottedLineMaterial == null)
        {
            dottedLineMaterial = new Material(shader);
        }
        dottedLineMaterial.mainTexture = dottedTexture;
    }
    private Texture2D BuildDottedTexture()
    {
        const int size = 64;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Bilinear;
        var radius = Mathf.Clamp(Mathf.RoundToInt(size * dotFillRatio * 0.5f), 1, (size / 2) - 1);
        var feather = Mathf.Max(1f, size * 0.06f);
        var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                var dx = x - center.x;
                var dy = y - center.y;
                var distance = Mathf.Sqrt(dx * dx + dy * dy);
                var alpha = Mathf.Clamp01((radius + feather - distance) / feather);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        texture.Apply(false, true);
        return texture;
    }
    private float GetTargetConfirmProgress(Transform target)
    {
        if (!useTargetConfirmFx || autoSlash == null || target == null) return 0f;
        return Mathf.Clamp01(autoSlash.GetCurrentTargetConfirmProgress(target));
    }
    private float SmoothConfirmProgress(float current, float next, bool hasTarget)
    {
        if (confirmSmoothSpeed <= 0f) return hasTarget ? next : 0f;
        var delta = Time.unscaledDeltaTime > 0f ? Time.unscaledDeltaTime : Time.deltaTime;
        var step = confirmSmoothSpeed * Mathf.Max(0f, delta);
        if (!hasTarget)
        {
            return Mathf.MoveTowards(current, 0f, step);
        }
        return Mathf.MoveTowards(current, next, step);
    }
    private int EvaluateConfirmStage(float progress)
    {
        var clamped = Mathf.Clamp01(progress);
        var caution = Mathf.Clamp(cautionThreshold, 0.05f, 0.85f);
        var warning = Mathf.Clamp(warningThreshold, caution + 0.05f, 0.95f);
        var danger = Mathf.Clamp(dangerThreshold, warning + 0.02f, 1f);
        if (clamped < caution) return 0;
        if (clamped < warning) return 1;
        if (clamped < danger) return 2;
        return 3;
    }
    private float UpdateConfirmStageFx(bool hasTarget, float progress)
    {
        if (!useTargetConfirmFx || !hasTarget)
        {
            confirmStage = -1;
            confirmStageKickTimer = 0f;
            return 0f;
        }
        var delta = Time.unscaledDeltaTime > 0f ? Time.unscaledDeltaTime : Time.deltaTime;
        var stage = EvaluateConfirmStage(progress);
        if (stage != confirmStage)
        {
            confirmStage = stage;
            confirmStageKickTimer = stageTransitionKickDuration;
        }
        if (confirmStageKickTimer > 0f)
        {
            confirmStageKickTimer = Mathf.Max(0f, confirmStageKickTimer - delta);
        }
        var stageSpeed = stagePulseSpeed + (stage * stagePulseSpeedByStage);
        var wave = (Mathf.Sin(Time.unscaledTime * Mathf.Max(0f, stageSpeed)) + 1f) * 0.5f;
        var pulseFx = wave * stagePulseAmount * Mathf.Clamp01(progress + (stage * 0.08f));
        var kickFx = 0f;
        if (stageTransitionKickDuration > 0f)
        {
            kickFx = (confirmStageKickTimer / stageTransitionKickDuration) * stageTransitionKickAmount;
        }
        return Mathf.Clamp01(pulseFx + kickFx);
    }
    private Color EvaluateConfirmRampColor(float progress)
    {
        var clamped = Mathf.Clamp01(progress);
        var caution = Mathf.Clamp(cautionThreshold, 0.05f, 0.85f);
        var warning = Mathf.Clamp(warningThreshold, caution + 0.05f, 0.95f);
        if (clamped <= caution)
        {
            var t = caution > 0f ? clamped / caution : 1f;
            return Color.Lerp(targetReadyColor, targetCautionColor, t);
        }
        if (clamped <= warning)
        {
            var t = (clamped - caution) / Mathf.Max(0.001f, warning - caution);
            return Color.Lerp(targetCautionColor, targetWarningColor, t);
        }
        var confirmT = (clamped - warning) / Mathf.Max(0.001f, 1f - warning);
        return Color.Lerp(targetWarningColor, targetConfirmColor, confirmT);
    }
    private void ResolveTargetColors(bool hasTarget, float confirmProgress, float stageFx, out Color lineStartColor, out Color lineEndColor, out Color ringColor)
    {
        if (!hasTarget)
        {
            lineStartColor = idleLineColor;
            lineEndColor = idleLineColor;
            ringColor = idleLineColor;
            return;
        }
        if (!useTargetConfirmFx)
        {
            lineStartColor = detectedLineColor;
            lineEndColor = detectedLineColor;
            ringColor = detectedLineColor;
            return;
        }
        var rampColor = EvaluateConfirmRampColor(confirmProgress);
        var wave = 0f;
        if (confirmPulseAmount > 0f && confirmPulseSpeed > 0f)
        {
            wave = (Mathf.Sin(Time.unscaledTime * confirmPulseSpeed) + 1f) * 0.5f;
        }
        var pulseFx = wave * confirmPulseAmount * confirmProgress;
        var flashAmount = Mathf.Clamp01((pulseFx + stageFx) * stageFlashToWhite);
        var flashColor = Color.Lerp(rampColor, Color.white, flashAmount);
        lineStartColor = Color.Lerp(detectedLineColor, rampColor, lineColorBlend);
        lineEndColor = Color.Lerp(rampColor, flashColor, 0.75f);
        ringColor = rampColor;
    }
    private void OnDestroy()
    {
        ReleaseMonsterExistingRing(true);

        if (dottedLineMaterial != null)
        {
            Destroy(dottedLineMaterial);
        }
        if (dottedTexture != null)
        {
            Destroy(dottedTexture);
        }
    }
}
