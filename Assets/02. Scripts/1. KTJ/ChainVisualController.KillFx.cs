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

        var position = ResolveChainKillPrefabPosition(target);
        var instance = Instantiate(chainKillPrefab, position, chainKillPrefab.transform.rotation);
        if (instance == null) return;

        TryUpdateChainKillText(instance);

        if (chainKillPrefabAutoDestroyTime <= 0f) return;

        Destroy(instance, chainKillPrefabAutoDestroyTime);
    }

    private Vector3 ResolveChainKillPrefabPosition(Transform target)
    {
        var position = target.position;
        if (TryGetTargetTopPoint(target, out var topPoint))
        {
            position = topPoint;
        }

        position.y += chainKillPrefabHeightOffset;
        return position;
    }

    private bool TryGetTargetTopPoint(Transform target, out Vector3 topPoint)
    {
        topPoint = target.position;

        var colliders = target.GetComponentsInChildren<Collider>(true);
        var hasCollider = false;
        var highestColliderY = float.MinValue;
        var highestColliderCenter = target.position;
        for (int i = 0; i < colliders.Length; i++)
        {
            var collider = colliders[i];
            if (collider == null) continue;
            if (!collider.enabled) continue;
            if (!collider.gameObject.activeInHierarchy) continue;

            var colliderBounds = collider.bounds;
            if (colliderBounds.size.sqrMagnitude <= 0f) continue;
            if (hasCollider && colliderBounds.max.y <= highestColliderY) continue;

            hasCollider = true;
            highestColliderY = colliderBounds.max.y;
            highestColliderCenter = colliderBounds.center;
        }

        if (hasCollider)
        {
            topPoint = new Vector3(highestColliderCenter.x, highestColliderY, highestColliderCenter.z);
            return true;
        }

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

    private void TryUpdateChainKillText(GameObject instance)
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
        catch
        {
        }
    }

    private void TrySpawnDamageText(DamageSystem.DamageResult result)
    {
        if (!useDamageText) return;
        if (result.Amount <= 0) return;
        if (result.Target == null) return;

        float amountNormalized = Mathf.Clamp01(Mathf.Log10(result.Amount + 1f) * 0.5f);
        bool isBigHit = result.Amount >= damageTextBigHitThreshold;

        float safeScale = Mathf.Clamp(damageTextScale, DamageTextMinScale, DamageTextMaxScale);
        safeScale *= 1f + amountNormalized * Mathf.Clamp01(damageTextAmountScaleWeight);
        if (isBigHit)
        {
            safeScale += Mathf.Max(0f, damageTextBigHitExtraScale);
        }
        if (result.IsDead)
        {
            safeScale *= 1.32f;
        }
        safeScale = Mathf.Clamp(safeScale, DamageTextMinScale, DamageTextMaxScale);

        float safeFontSize = Mathf.Clamp(damageTextFontSize, DamageTextMinFontSize, DamageTextMaxFontSize);
        safeFontSize *= Mathf.Lerp(1f, 1.18f, amountNormalized);
        if (isBigHit) safeFontSize *= 1.06f;
        if (result.IsDead) safeFontSize *= 1.22f;
        safeFontSize = Mathf.Clamp(safeFontSize, DamageTextMinFontSize, DamageTextMaxFontSize);

        float safeDuration = Mathf.Clamp(damageTextDuration, DamageTextMinDuration, DamageTextMaxDuration);
        float safeRiseDistance = Mathf.Clamp(damageTextRiseDistance, 0f, DamageTextMaxRiseDistance);
        if (result.IsDead)
        {
            safeDuration = Mathf.Clamp(safeDuration * 1.08f, DamageTextMinDuration, DamageTextMaxDuration);
            safeRiseDistance = Mathf.Clamp(safeRiseDistance * 1.16f, 0f, DamageTextMaxRiseDistance);
        }
        float safePopDuration = Mathf.Clamp(damageTextPopDuration, 0f, safeDuration * 0.5f);
        float safePopScaleMultiplier = Mathf.Clamp(damageTextPopScaleMultiplier, 1f, 3f);
        float safeDriftDistance = Mathf.Clamp(damageTextDriftDistance, 0f, DamageTextMaxDriftDistance);
        if (result.IsDead) safeDriftDistance *= 0.55f;

        var spawnPosition = ResolveDamageTextPosition(result.Target);
        var textObject = new GameObject($"DamageText_{result.Amount}", typeof(TextMeshPro));
        textObject.transform.position = spawnPosition;
        textObject.transform.localScale = Vector3.one * (safeScale * 0.55f);

        var tmp = textObject.GetComponent<TextMeshPro>();
        var fontAsset = ResolveDamageTextFontAsset();
        if (fontAsset != null)
        {
            tmp.font = fontAsset;
        }
        tmp.text = result.Amount.ToString();
        tmp.fontSize = safeFontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.enableWordWrapping = false;
        tmp.alignment = TextAlignmentOptions.Center;
        var targetColor = ResolveDamageTextColor(result.IsDead, isBigHit);
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

        float popScaleMultiplier = safePopScaleMultiplier;
        if (isBigHit) popScaleMultiplier += 0.18f;
        if (result.IsDead) popScaleMultiplier += 0.36f;
        var popScale = Vector3.one * (safeScale * popScaleMultiplier);
        var targetScale = Vector3.one * (result.IsDead ? safeScale * 1.2f : safeScale);
        var drift = new Vector3(
            Random.Range(-safeDriftDistance, safeDriftDistance),
            0f,
            Random.Range(-safeDriftDistance, safeDriftDistance));
        var targetPosition = spawnPosition + drift + Vector3.up * safeRiseDistance;

        var sequence = DOTween.Sequence();
        sequence.SetUpdate(useUnscaledTime);
        if (safePopDuration > 0f)
        {
            sequence.Append(textObject.transform.DOScale(popScale, safePopDuration).SetEase(Ease.OutBack));
        }
        sequence.Join(DOTween.To(() => tmp.color, x => tmp.color = x, targetColor, Mathf.Max(0.05f, safePopDuration)).SetEase(Ease.OutQuad));
        sequence.Append(textObject.transform.DOScale(targetScale, safeDuration).SetEase(Ease.OutQuad));
        sequence.Join(textObject.transform.DOMove(targetPosition, safeDuration).SetEase(Ease.OutCubic));
        sequence.Join(tmp.DOFade(0f, safeDuration * 0.85f).SetDelay(safeDuration * 0.15f).SetEase(Ease.InQuad));
        sequence.Join(DOTween.To(() => tilt, x => tilt = x, 0f, safeDuration).SetEase(Ease.OutCubic));
        sequence.OnUpdate(() => AlignDamageTextToCamera(textObject.transform, tilt));
        sequence.OnComplete(() =>
        {
            if (textObject != null)
            {
                Destroy(textObject);
            }
        });
    }

    private Vector3 ResolveDamageTextPosition(Transform target)
    {
        var position = target.position;
        if (TryGetTargetTopPoint(target, out var topPoint))
        {
            position = topPoint;
        }

        position += damageTextOffset;
        float safeHorizontal = Mathf.Clamp(damageTextRandomHorizontal, 0f, DamageTextMaxHorizontalJitter);
        if (safeHorizontal > 0f)
        {
            position.x += Random.Range(-safeHorizontal, safeHorizontal);
            position.z += Random.Range(-safeHorizontal, safeHorizontal);
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

        var camera = ResolveDamageTextCamera();
        if (camera == null) return;

        textTransform.rotation = Quaternion.LookRotation(camera.transform.forward, camera.transform.up)
            * Quaternion.Euler(0f, 0f, tilt);
    }

    private Camera ResolveDamageTextCamera()
    {
        if (damageTextCamera != null) return damageTextCamera;

        damageTextCamera = Camera.main;
        if (damageTextCamera != null) return damageTextCamera;

        damageTextCamera = FindObjectOfType<Camera>();
        return damageTextCamera;
    }

    private Color ResolveDamageTextColor(bool isDead, bool isBigHit)
    {
        if (isDead) return killDamageTextColor;
        if (isBigHit) return damageTextBigHitColor;
        return damageTextColor;
    }

    private VertexGradient BuildDamageTextGradient(Color baseColor)
    {
        var topColor = Color.Lerp(baseColor, Color.white, 0.55f);
        return new VertexGradient(topColor, topColor, baseColor, baseColor);
    }

    private TMP_FontAsset ResolveDamageTextFontAsset()
    {
        if (damageTextFontAsset != null) return damageTextFontAsset;
        if (chainText != null && chainText.font != null) return chainText.font;
        return TMP_Settings.defaultFontAsset;
    }
}
