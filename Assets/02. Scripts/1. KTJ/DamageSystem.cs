using System;
using UnityEngine;

using System.Collections.Generic;

public class DamageSystem : MonoBehaviour
{
    [SerializeField] private TargetingSystem targetingSystem;

    private readonly List<MonoBehaviour> damageableSearchBuffer = new List<MonoBehaviour>(16);

    public event Action<DamageResult> OnDamageApplied;

    private void Awake()
    {
        if (targetingSystem == null) targetingSystem = GetComponent<TargetingSystem>();
        if (targetingSystem == null) targetingSystem = GetComponentInParent<TargetingSystem>();
    }

    public void ApplyDamage(Transform target, int amount)
    {
        if (target == null) return;
        if (amount <= 0) return;

        if (!TryResolveDamageable(target, out var damageable, out var targetIdentity)) return;
        if (damageable.IsDead) return;

        damageable.ApplyDamage(amount);

        var result = new DamageResult(target, targetIdentity, amount, damageable.IsDead);
        OnDamageApplied?.Invoke(result);
    }

    private bool TryResolveDamageable(Transform target, out IDamageable damageable, out Transform identity)
    {
        if (targetingSystem != null && targetingSystem.TryResolveDamageable(target, out damageable))
        {
            identity = targetingSystem.ResolveTargetIdentity(target);
            return true;
        }

        return CombatTargetResolver.TryResolve(
            target,
            damageableSearchBuffer,
            out damageable,
            out identity);
    }

    public struct DamageResult
    {
        public readonly Transform Target;
        public readonly Transform TargetIdentity;
        public readonly int Amount;
        public readonly bool IsDead;

        public DamageResult(Transform target, int amount, bool isDead)
            : this(target, target, amount, isDead)
        {
        }

        public DamageResult(Transform target, Transform targetIdentity, int amount, bool isDead)
        {
            Target = target;
            TargetIdentity = targetIdentity != null ? targetIdentity : target;
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
