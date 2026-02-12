using UnityEngine;

public partial class TargetingLineVisualizer
{
    private const float LockOnIconMaxRuntimeScale = 2f;

    private void EnsureMonsterLockOnIcon(MonsterRingEntry entry)
    {
        if (!useLockOnIcon) return;
        if (entry == null || entry.Target == null) return;
        if (entry.LockOnTransform != null) return;

        // 사각형 아티팩트를 피하기 위해 락온 아이콘은 안전한 스프라이트 경로만 사용한다.
        var iconObject = new GameObject("MonsterLockOnIcon");
        var spriteRenderer = iconObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetFallbackLockOnSprite();
        iconObject.AddComponent<MonsterTargetRingMarker>();

        if (iconObject == null) return;

        iconObject.name = "MonsterLockOnIcon";
        entry.SpawnedLockOnByVisualizer = true;
        entry.LockOnTransform = iconObject.transform;
        entry.LockOnSpriteRenderer = iconObject.GetComponentInChildren<SpriteRenderer>(true);
        if (entry.LockOnSpriteRenderer != null)
        {
            entry.LockOnSpriteRenderer.sortingOrder = 30000;
            entry.LockOnSpriteRenderer.color = lockOnIconColor;
        }

        iconObject.SetActive(false);
        UpdateMonsterLockOnTransform(entry);
    }

    private Sprite GetFallbackLockOnSprite()
    {
        if (fallbackLockOnSprite != null) return fallbackLockOnSprite;

        const int size = 128;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false, true)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        var center = (size - 1) * 0.5f;
        var radius = size * 0.34f;
        var ringThickness = 3.5f;
        var tickInner = radius + 3f;
        var tickOuter = radius + 12f;
        var clear = new Color(0f, 0f, 0f, 0f);
        var solid = Color.white;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                var dx = x - center;
                var dy = y - center;
                var dist = Mathf.Sqrt((dx * dx) + (dy * dy));
                var absDx = Mathf.Abs(dx);
                var absDy = Mathf.Abs(dy);

                var onRing = Mathf.Abs(dist - radius) <= ringThickness;
                var onHorizontalTick = absDy <= 1.5f && dist >= tickInner && dist <= tickOuter;
                var onVerticalTick = absDx <= 1.5f && dist >= tickInner && dist <= tickOuter;
                var pixel = (onRing || onHorizontalTick || onVerticalTick) ? solid : clear;
                texture.SetPixel(x, y, pixel);
            }
        }

        texture.Apply(false, false);
        fallbackLockOnTexture = texture;
        fallbackLockOnSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            100f,
            0u,
            SpriteMeshType.Tight
        );

        return fallbackLockOnSprite;
    }

    private void UpdateMonsterLockOnTransform(MonsterRingEntry entry)
    {
        if (entry == null || entry.Target == null) return;
        if (entry.LockOnTransform == null) return;

        entry.LockOnTransform.position = ResolveMonsterLockOnPosition(entry) + lockOnIconOffset;

        if (lockOnIconUseFlatPlane)
        {
            entry.LockOnTransform.rotation = Quaternion.Euler(lockOnIconFlatPlanePitch, entry.LockOnSpinAngle, 0f);
            return;
        }

        if (lockOnIconBillboard)
        {
            var camera = Camera.main;
            if (camera != null)
            {
                var direction = camera.transform.position - entry.LockOnTransform.position;
                if (direction.sqrMagnitude > 0f)
                {
                    entry.LockOnTransform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
                }
            }
        }

        if (lockOnIconSpinSpeed > 0f)
        {
            entry.LockOnTransform.rotation *= Quaternion.AngleAxis(entry.LockOnSpinAngle, Vector3.forward);
        }
    }

    private Vector3 ResolveMonsterLockOnPosition(MonsterRingEntry entry)
    {
        var groundPosition = ResolveMonsterRingGroundPosition(entry);
        return groundPosition + (Vector3.up * monsterRingHeightOffset);
    }

    private void ApplyMonsterLockOnIcon(MonsterRingEntry entry, bool visible, Color color, float confirmProgress)
    {
        if (entry == null) return;
        if (!useLockOnIcon)
        {
            if (entry.LockOnTransform != null && entry.LockOnTransform.gameObject.activeSelf)
            {
                entry.LockOnTransform.gameObject.SetActive(false);
            }

            entry.LockOnVisibleLerp = 0f;
            return;
        }

        EnsureMonsterLockOnIcon(entry);
        if (entry.LockOnTransform == null) return;

        var delta = Time.unscaledDeltaTime > 0f ? Time.unscaledDeltaTime : Time.deltaTime;
        var safeDelta = Mathf.Max(0f, delta);
        var targetVisible = visible ? 1f : 0f;
        var appearSpeed = Mathf.Max(0.01f, lockOnIconAppearSpeed);
        entry.LockOnVisibleLerp = Mathf.MoveTowards(entry.LockOnVisibleLerp, targetVisible, safeDelta * appearSpeed);
        var active = entry.LockOnVisibleLerp > 0.001f;

        if (entry.LockOnTransform.gameObject.activeSelf != active)
        {
            entry.LockOnTransform.gameObject.SetActive(active);
        }

        if (!active) return;

        if (lockOnIconSpinSpeed > 0f)
        {
            entry.LockOnSpinAngle = Mathf.Repeat(entry.LockOnSpinAngle + (lockOnIconSpinSpeed * safeDelta), 360f);
        }

        var clampedConfirm = Mathf.Clamp01(confirmProgress);
        var pulseWave = 1f;
        if (lockOnIconPulseAmount > 0f && lockOnIconPulseSpeed > 0f)
        {
            var t = (Mathf.Sin((Time.unscaledTime + entry.LockOnPulseSeed) * lockOnIconPulseSpeed) + 1f) * 0.5f;
            pulseWave = 1f + (t * lockOnIconPulseAmount * entry.LockOnVisibleLerp);
        }

        var scaleBoost = 1f + (entry.LockOnVisibleLerp * 0.18f) + (clampedConfirm * lockOnIconConfirmScaleBoost);
        var rawScale = Mathf.Max(0.05f, lockOnIconScale * scaleBoost * pulseWave);
        var clampedScale = Mathf.Min(LockOnIconMaxRuntimeScale, rawScale);
        entry.LockOnTransform.localScale = Vector3.one * clampedScale;
        UpdateMonsterLockOnTransform(entry);

        if (entry.LockOnSpriteRenderer == null) return;

        var mixed = Color.Lerp(lockOnIconColor, color, Mathf.Clamp01(lockOnIconColorBlend));
        var alphaRamp = Mathf.Lerp(lockOnIconMinAlpha, 1f, entry.LockOnVisibleLerp);
        mixed.a = lockOnIconColor.a * alphaRamp;
        entry.LockOnSpriteRenderer.color = mixed;
    }
}
