using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Resolves the damage receiver and stable combat identity for a target without
/// allocating component arrays. Callers own the reusable search buffer.
/// </summary>
public static class CombatTargetResolver
{
    public static bool TryResolve(
        Transform target,
        List<MonoBehaviour> searchBuffer,
        out DamageSystem.IDamageable damageable,
        out Transform identity)
    {
        damageable = null;
        identity = target;

        if (target == null) return false;

        damageable = target.GetComponent<DamageSystem.IDamageable>();
        if (IsAlive(damageable))
        {
            identity = GetIdentity(damageable, target);
            return true;
        }

        damageable = target.GetComponentInParent<DamageSystem.IDamageable>();
        if (IsAlive(damageable))
        {
            identity = GetIdentity(damageable, target);
            return true;
        }

        var root = target.root;
        if (root == null || searchBuffer == null) return false;

        searchBuffer.Clear();
        root.GetComponentsInChildren(true, searchBuffer);

        for (int i = 0; i < searchBuffer.Count; i++)
        {
            if (!(searchBuffer[i] is DamageSystem.IDamageable candidate)) continue;
            if (!IsAlive(candidate)) continue;

            damageable = candidate;
            identity = GetIdentity(candidate, target);
            searchBuffer.Clear();
            return true;
        }

        searchBuffer.Clear();
        return false;
    }

    public static bool IsAlive(DamageSystem.IDamageable damageable)
    {
        if (damageable == null) return false;

        if (damageable is Object unityObject)
        {
            return unityObject != null;
        }

        return true;
    }

    private static Transform GetIdentity(DamageSystem.IDamageable damageable, Transform fallback)
    {
        return damageable is Component component && component != null
            ? component.transform
            : fallback;
    }
}
