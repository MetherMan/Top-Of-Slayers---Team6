using System;
using UnityEngine;

public class DamageSystem : MonoBehaviour
{
    public event Action<DamageResult> OnDamageApplied;

    public void ApplyDamage(Transform target, int amount)
    {
        if (target == null) return;
        if (amount <= 0) return;

        var damageable = ResolveDamageable(target);
        if (damageable == null) return;

        damageable.ApplyDamage(amount);

        var result = new DamageResult(target, amount, damageable.IsDead);
        OnDamageApplied?.Invoke(result);
    }

    private IDamageable ResolveDamageable(Transform target)
    {
        var direct = target.GetComponent<IDamageable>();
        if (direct != null) return direct;

        var parent = target.GetComponentInParent<IDamageable>();
        if (parent != null) return parent;

        var root = target.root;
        if (root == null) return null;

        var components = root.GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] is IDamageable damageable)
            {
                return damageable;
            }
        }

        return null;
    }

    public struct DamageResult
    {
        public readonly Transform Target;
        public readonly int Amount;
        public readonly bool IsDead;

        public DamageResult(Transform target, int amount, bool isDead)
        {
            Target = target;
            Amount = amount;
            IsDead = isDead;
        }
    }

    public interface IDamageable
    {
        void ApplyDamage(int amount);
        bool IsDead { get; }
    }
}
