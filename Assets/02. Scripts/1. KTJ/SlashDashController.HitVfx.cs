using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class SlashDashController
{
    [Header("Hit VFX Preset")]
    [SerializeField] private bool useHitImpactVfx = true;
    [SerializeField] private GameObject weakHitImpactVfxPrefab;
    [SerializeField] private GameObject mediumHitImpactVfxPrefab;
    [SerializeField] private GameObject strongHitImpactVfxPrefab;
    [SerializeField, Min(1)] private int mediumHitChainThreshold = 3;
    [SerializeField, Min(1)] private int strongHitChainThreshold = 6;
    [SerializeField, Min(0f)] private float hitImpactHeightOffset = 0.2f;
    [SerializeField, Min(0f)] private float hitImpactAutoDestroyTime = 1.2f;

#if UNITY_EDITOR
    private const string DefaultWeakHitVfxPath = "Assets/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Impacts/CFXR Hit A (Red).prefab";
    private const string DefaultMediumHitVfxPath = "Assets/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Impacts/CFXR Hit D 3D (Yellow).prefab";
    private const string DefaultStrongHitVfxPath = "Assets/JMO Assets/Cartoon FX Remaster/CFXR Prefabs/Sword Trails/Plain/CFXR4 Sword Hit PLAIN (Cross).prefab";
#endif

    private void OnValidate()
    {
        ValidateHitImpactThresholds();
#if UNITY_EDITOR
        TryAutoAssignHitImpactPrefabsInEditor();
#endif
    }

    private void ValidateHitImpactThresholds()
    {
        mediumHitChainThreshold = Mathf.Max(1, mediumHitChainThreshold);
        strongHitChainThreshold = Mathf.Max(mediumHitChainThreshold + 1, strongHitChainThreshold);
    }

    private void TrySpawnHitImpactVfx(Transform impactTarget)
    {
        if (!useHitImpactVfx) return;
        if (pendingDamage <= 0) return;

        var vfxPrefab = SelectHitImpactVfxPrefab(impactTarget);
        if (vfxPrefab == null) return;

        var spawnPosition = ResolveHitImpactVfxPosition(impactTarget);
        var instance = Instantiate(vfxPrefab, spawnPosition, vfxPrefab.transform.rotation);
        if (instance == null) return;

        if (hitImpactAutoDestroyTime <= 0f) return;
        Destroy(instance, hitImpactAutoDestroyTime);
    }

    private GameObject SelectHitImpactVfxPrefab(Transform impactTarget)
    {
        var chainStep = PredictNextChainStep(impactTarget);
        if (chainStep >= strongHitChainThreshold)
        {
            return strongHitImpactVfxPrefab != null
                ? strongHitImpactVfxPrefab
                : (mediumHitImpactVfxPrefab != null ? mediumHitImpactVfxPrefab : weakHitImpactVfxPrefab);
        }

        if (chainStep >= mediumHitChainThreshold)
        {
            return mediumHitImpactVfxPrefab != null
                ? mediumHitImpactVfxPrefab
                : (weakHitImpactVfxPrefab != null ? weakHitImpactVfxPrefab : strongHitImpactVfxPrefab);
        }

        return weakHitImpactVfxPrefab != null
            ? weakHitImpactVfxPrefab
            : (mediumHitImpactVfxPrefab != null ? mediumHitImpactVfxPrefab : strongHitImpactVfxPrefab);
    }

    private int PredictNextChainStep(Transform impactTarget)
    {
        if (chainCombat == null) return 1;

        var currentChain = Mathf.Max(0, chainCombat.CurrentChain);
        if (currentChain <= 0) return 1;

        var lastTarget = chainCombat.LastTarget;
        if (impactTarget != null && lastTarget != null && impactTarget != lastTarget)
        {
            return currentChain + 1;
        }

        return currentChain;
    }

    private Vector3 ResolveHitImpactVfxPosition(Transform impactTarget)
    {
        if (impactTarget == null) return transform.position;

        if (TryGetTargetCenterPoint(impactTarget, out var centerPoint))
        {
            centerPoint.y += hitImpactHeightOffset;
            return centerPoint;
        }

        var fallback = impactTarget.position;
        fallback.y += hitImpactHeightOffset;
        return fallback;
    }

    private bool TryGetTargetCenterPoint(Transform target, out Vector3 centerPoint)
    {
        centerPoint = target.position;

        var colliders = target.GetComponentsInChildren<Collider>(true);
        for (var i = 0; i < colliders.Length; i++)
        {
            var collider = colliders[i];
            if (collider == null || !collider.enabled || !collider.gameObject.activeInHierarchy) continue;

            var bounds = collider.bounds;
            if (bounds.size.sqrMagnitude <= 0f) continue;
            centerPoint = bounds.center;
            return true;
        }

        var renderers = target.GetComponentsInChildren<Renderer>(true);
        for (var i = 0; i < renderers.Length; i++)
        {
            var renderer = renderers[i];
            if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy) continue;

            var bounds = renderer.bounds;
            if (bounds.size.sqrMagnitude <= 0f) continue;
            centerPoint = bounds.center;
            return true;
        }

        return false;
    }

#if UNITY_EDITOR
    private void TryAutoAssignHitImpactPrefabsInEditor()
    {
        if (Application.isPlaying) return;

        if (weakHitImpactVfxPrefab == null)
        {
            weakHitImpactVfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultWeakHitVfxPath);
        }

        if (mediumHitImpactVfxPrefab == null)
        {
            mediumHitImpactVfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultMediumHitVfxPath);
        }

        if (strongHitImpactVfxPrefab == null)
        {
            strongHitImpactVfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DefaultStrongHitVfxPath);
        }
    }
#endif
}
