using DG.Tweening;
using TMPro;
using UnityEngine;

public partial class ChainVisualController
{
    private const string ChainKillParticleTextTypeName = "CartoonFX.CFXR_ParticleText";
    private const string ChainKillText = "KILL!";
    private const float DamageTextMinScale = 0.03f;
    private const float DamageTextMaxScale = 0.5f;
    private const float DamageTextMinFontSize = 2f;
    private const float DamageTextMaxFontSize = 16f;
    private const float DamageTextMinDuration = 0.12f;
    private const float DamageTextMaxDuration = 1.1f;
    private const float DamageTextMaxRiseDistance = 2f;
    private const float DamageTextMaxHorizontalJitter = 0.6f;
    private const float DamageTextMaxDriftDistance = 0.5f;

    private void TrySpawnChainKillPrefab(Transform target)
    {
        if (chainKillPrefab == null) return;
        if (target == null) return;

        var position = GetChainKillSpawnPosition(target);
        var instance = Instantiate(chainKillPrefab, position, chainKillPrefab.transform.rotation);
        if (instance == null) return;

        ConfigureEffectTiming(instance);
        UpdateChainKillText(instance);

        if (chainKillPrefabAutoDestroyTime <= 0f) return;

        StartCoroutine(DestroyEffectAfterDelay(instance, chainKillPrefabAutoDestroyTime));
    }

    private Vector3 GetChainKillSpawnPosition(Transform target)
    {
        var position = GetTargetTopPoint(target);
        position.y += chainKillPrefabHeightOffset;
        return position;
    }

    private Vector3 GetTargetTopPoint(Transform target)
    {
        if (target == null) return transform.position;
        if (TryGetTopPointFromColliders(target, out var topPoint)) return topPoint;
        if (TryGetTopPointFromRenderers(target, out topPoint)) return topPoint;
        return target.position;
    }

    private bool TryGetTopPointFromColliders(Transform target, out Vector3 topPoint)
    {
        topPoint = target.position;
        var colliders = target.GetComponentsInChildren<Collider>(true);
        var hasCollider = false;
        var highestY = float.MinValue;
        var highestCenter = target.position;

        for (int i = 0; i < colliders.Length; i++)
        {
            var collider = colliders[i];
            if (collider == null) continue;
            if (!collider.enabled) continue;
            if (!collider.gameObject.activeInHierarchy) continue;

            var bounds = collider.bounds;
            if (bounds.size.sqrMagnitude <= 0f) continue;
            if (hasCollider && bounds.max.y <= highestY) continue;

            hasCollider = true;
            highestY = bounds.max.y;
            highestCenter = bounds.center;
        }

        if (!hasCollider) return false;

        topPoint = new Vector3(highestCenter.x, highestY, highestCenter.z);
        return true;
    }

    private bool TryGetTopPointFromRenderers(Transform target, out Vector3 topPoint)
    {
        topPoint = target.position;
        var targetRenderers = target.GetComponentsInChildren<Renderer>(true);
        var hasRenderer = false;
        var highestY = float.MinValue;
        var highestCenter = target.position;

        for (int i = 0; i < targetRenderers.Length; i++)
        {
            var renderer = targetRenderers[i];
            if (renderer == null) continue;
            if (!renderer.enabled) continue;
            if (!renderer.gameObject.activeInHierarchy) continue;

            var rendererBounds = renderer.bounds;
            if (rendererBounds.size.sqrMagnitude <= 0f) continue;

            if (hasRenderer && rendererBounds.max.y <= highestY) continue;
            hasRenderer = true;
            highestY = rendererBounds.max.y;
            highestCenter = rendererBounds.center;
        }

        if (!hasRenderer) return false;
        topPoint = new Vector3(highestCenter.x, highestY, highestCenter.z);
        return true;
    }

    private void UpdateChainKillText(GameObject instance)
    {
        if (instance == null) return;

        var particleText = instance.GetComponent(ChainKillParticleTextTypeName);
        if (particleText == null) return;

        var particleTextType = particleText.GetType();
        var dynamicField = particleTextType.GetField("isDynamic");
        if (dynamicField != null)
        {
            dynamicField.SetValue(particleText, true);
        }

        var updateTextMethod = particleTextType.GetMethod("UpdateText");
        if (updateTextMethod == null) return;

        try
        {
            updateTextMethod.Invoke(particleText, new object[] { ChainKillText, null, null, null, null, null });
        }
        catch (System.Exception)
        {
        }
    }

    private void TrySpawnDamageText(DamageSystem.DamageResult result)
    {
        if (!useDamageText) return;
        if (result.Amount <= 0) return;
        if (result.Target == null) return;

        float amountNormalized = Mathf.Clamp01(
            Mathf.InverseLerp(
                1f,
                Mathf.Max(2f, damageTextBigHitThreshold * 2.5f),
                result.Amount));
        float amountCurve = Mathf.SmoothStep(0f, 1f, amountNormalized);
        bool isBigHit = result.Amount >= damageTextBigHitThreshold;

        float scale = Mathf.Clamp(damageTextScale, DamageTextMinScale, DamageTextMaxScale);
        scale *= 1f + amountCurve * Mathf.Clamp01(damageTextAmountScaleWeight);
        if (isBigHit)
        {
            scale += Mathf.Max(0f, damageTextBigHitExtraScale) * Mathf.Lerp(0.65f, 1f, amountCurve);
        }
        if (result.IsDead)
        {
            scale *= 1.32f;
        }
        scale = Mathf.Clamp(scale, DamageTextMinScale, DamageTextMaxScale);

        float fontSize = Mathf.Clamp(damageTextFontSize, DamageTextMinFontSize, DamageTextMaxFontSize);
        fontSize *= Mathf.Lerp(1f, 1.24f, amountCurve);
        if (isBigHit) fontSize *= Mathf.Lerp(1.04f, 1.1f, amountCurve);
        if (result.IsDead) fontSize *= 1.22f;
        fontSize = Mathf.Clamp(fontSize, DamageTextMinFontSize, DamageTextMaxFontSize);

        float duration = Mathf.Clamp(damageTextDuration, DamageTextMinDuration, DamageTextMaxDuration);
        float riseDistance = Mathf.Clamp(damageTextRiseDistance, 0f, DamageTextMaxRiseDistance);
        if (result.IsDead)
        {
            duration = Mathf.Clamp(duration * 1.08f, DamageTextMinDuration, DamageTextMaxDuration);
            riseDistance = Mathf.Clamp(riseDistance * 1.16f, 0f, DamageTextMaxRiseDistance);
        }
        float popDuration = Mathf.Clamp(damageTextPopDuration, 0f, duration * 0.5f);
        float popScaleMultiplier = Mathf.Clamp(damageTextPopScaleMultiplier, 1f, 3f);
        float driftDistance = Mathf.Clamp(damageTextDriftDistance, 0f, DamageTextMaxDriftDistance);
        if (result.IsDead) driftDistance *= 0.55f;

        var spawnPosition = GetDamageTextPosition(result.Target);
        var textObject = new GameObject($"DamageText_{result.Amount}", typeof(TextMeshPro));
        textObject.transform.position = spawnPosition;
        textObject.transform.localScale = Vector3.one * (scale * 0.55f);

        var tmp = textObject.GetComponent<TextMeshPro>();
        var fontAsset = GetDamageTextFontAsset();
        if (fontAsset != null)
        {
            tmp.font = fontAsset;
        }
        tmp.text = result.Amount.ToString();
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.enableWordWrapping = false;
        tmp.alignment = TextAlignmentOptions.Center;
        var targetColor = GetDamageTextColor(result.IsDead, isBigHit, amountCurve);
        tmp.color = Color.white;
        tmp.outlineWidth = 0.32f;
        tmp.outlineColor = new Color(0f, 0f, 0f, 0.9f);
        tmp.alpha = 1f;
        tmp.enableVertexGradient = true;
        tmp.colorGradient = BuildDamageTextGradient(targetColor);

        var textRenderer = tmp.renderer;
        if (textRenderer != null)
        {
            textRenderer.sortingOrder = 500;
            textRenderer.receiveShadows = false;
            textRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        float tilt = result.IsDead ? Random.Range(-20f, 20f) : Random.Range(-12f, 12f);
        AlignDamageTextToCamera(textObject.transform, tilt);

        if (isBigHit) popScaleMultiplier += 0.18f;
        if (result.IsDead) popScaleMultiplier += 0.36f;
        var popScale = Vector3.one * (scale * popScaleMultiplier);
        var targetScale = Vector3.one * (result.IsDead ? scale * 1.2f : scale);
        var drift = new Vector3(
            Random.Range(-driftDistance, driftDistance),
            0f,
            Random.Range(-driftDistance, driftDistance));
        var targetPosition = spawnPosition + drift + Vector3.up * riseDistance;

        var sequence = DOTween.Sequence();
        sequence.SetUpdate(useUnscaledTime);
        if (popDuration > 0f)
        {
            sequence.Append(textObject.transform.DOScale(popScale, popDuration).SetEase(Ease.OutBack));
        }
        sequence.Join(DOTween.To(() => tmp.color, x => tmp.color = x, targetColor, Mathf.Max(0.05f, popDuration)).SetEase(Ease.OutQuad));
        sequence.Append(textObject.transform.DOScale(targetScale, duration).SetEase(Ease.OutQuad));
        sequence.Join(textObject.transform.DOMove(targetPosition, duration).SetEase(Ease.OutCubic));
        sequence.Join(tmp.DOFade(0f, duration * 0.85f).SetDelay(duration * 0.15f).SetEase(Ease.InQuad));
        sequence.Join(DOTween.To(() => tilt, x => tilt = x, 0f, duration).SetEase(Ease.OutCubic));
        sequence.OnUpdate(() => AlignDamageTextToCamera(textObject.transform, tilt));
        sequence.OnComplete(() =>
        {
            if (textObject != null)
            {
                Destroy(textObject);
            }
        });
    }

    private Vector3 GetDamageTextPosition(Transform target)
    {
        var position = GetTargetTopPoint(target);
        position += damageTextOffset;
        float horizontalJitter = Mathf.Clamp(damageTextRandomHorizontal, 0f, DamageTextMaxHorizontalJitter);
        if (horizontalJitter > 0f)
        {
            position.x += Random.Range(-horizontalJitter, horizontalJitter);
            position.z += Random.Range(-horizontalJitter, horizontalJitter);
        }

        return position;
    }

    private void AlignDamageTextToCamera(Transform textTransform)
    {
        AlignDamageTextToCamera(textTransform, 0f);
    }

    private void AlignDamageTextToCamera(Transform textTransform, float tilt)
    {
        if (textTransform == null) return;

        var camera = GetDamageTextCamera();
        if (camera == null) return;

        textTransform.rotation = Quaternion.LookRotation(camera.transform.forward, camera.transform.up)
            * Quaternion.Euler(0f, 0f, tilt);
    }

    private Camera GetDamageTextCamera()
    {
        if (damageTextCamera != null) return damageTextCamera;

        damageTextCamera = Camera.main;
        if (damageTextCamera != null) return damageTextCamera;

        damageTextCamera = FindObjectOfType<Camera>();
        return damageTextCamera;
    }

    private Color GetDamageTextColor(bool isDead, bool isBigHit, float amountNormalized)
    {
        if (isDead) return killDamageTextColor;
        var amount01 = Mathf.Clamp01(amountNormalized);
        if (!isBigHit)
        {
            return Color.Lerp(damageTextColor, damageTextBigHitColor, amount01 * 0.35f);
        }

        return Color.Lerp(damageTextColor, damageTextBigHitColor, Mathf.Lerp(0.45f, 1f, amount01));
    }

    private VertexGradient BuildDamageTextGradient(Color baseColor)
    {
        var topColor = Color.Lerp(baseColor, Color.white, 0.55f);
        return new VertexGradient(topColor, topColor, baseColor, baseColor);
    }

    private TMP_FontAsset GetDamageTextFontAsset()
    {
        if (damageTextFontAsset != null) return damageTextFontAsset;
        if (chainText != null && chainText.font != null) return chainText.font;
        return TMP_Settings.defaultFontAsset;
    }

    private void ConfigureEffectTiming(GameObject instance)
    {
        if (instance == null) return;
        if (!useUnscaledTime) return;

        var particleSystems = instance.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particleSystems.Length; i++)
        {
            var particleSystem = particleSystems[i];
            if (particleSystem == null) continue;

            var main = particleSystem.main;
            main.useUnscaledTime = true;
        }

        var animators = instance.GetComponentsInChildren<Animator>(true);
        for (int i = 0; i < animators.Length; i++)
        {
            var effectAnimator = animators[i];
            if (effectAnimator == null) continue;
            effectAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }
    }

    private System.Collections.IEnumerator DestroyEffectAfterDelay(GameObject instance, float delay)
    {
        if (instance == null) yield break;
        if (delay <= 0f)
        {
            Destroy(instance);
            yield break;
        }

        yield return new WaitForSecondsRealtime(delay);
        if (instance != null)
        {
            Destroy(instance);
        }
    }
}
