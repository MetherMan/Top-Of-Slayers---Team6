using UnityEngine;

public partial class ChainVisualController
{
    private const string ChainKillParticleTextTypeName = "CartoonFX.CFXR_ParticleText";
    private const string ChainKillText = "KILL!";

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
}
