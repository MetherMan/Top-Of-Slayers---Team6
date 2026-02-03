using System;
using UnityEngine;

public class DamageSystem : MonoBehaviour
{
    public event Action<DamageResult> OnDamageApplied;

    public void ApplyDamage(Transform target, int amount)
    {
        if (target == null) return;
        if (amount <= 0) return;

        var damageable = target.GetComponent<IDamageable>();
        if (damageable == null) return;

        damageable.ApplyDamage(amount);

        var result = new DamageResult(target, amount, damageable.IsDead);
        OnDamageApplied?.Invoke(result);
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
