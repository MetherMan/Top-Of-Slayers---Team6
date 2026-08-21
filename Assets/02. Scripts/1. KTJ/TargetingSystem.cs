using System.Collections.Generic;
using UnityEngine;

public enum TargetingStrategyType
{
    Nearest,
    ForwardCone,
    Line
}

public partial class TargetingSystem : MonoBehaviour
{
    private const float TargetCoincidentSqrThreshold = 0.0001f;
    private const float TargetDirectionScoreEpsilon = 0.00001f;

    [Header("타겟 설정")]
    [SerializeField] private float maxRange = 8f;
    [SerializeField] private float coneAngle = 60f;
    [SerializeField, Min(0f)] private float lineWidth = 1.5f;
    [SerializeField, Min(0f)] private float lineEndPadding = 0.1f;

    [Header("전략")]
    [SerializeField] private TargetingStrategyType strategyType = TargetingStrategyType.Line;

    private readonly List<TargetEntry> targets = new List<TargetEntry>(64);
    private readonly Dictionary<int, TargetEntry> targetLookup = new Dictionary<int, TargetEntry>(64);
    private readonly List<MonoBehaviour> damageableSearchBuffer = new List<MonoBehaviour>(16);
    private ITargetingStrategy strategy;

    public float MaxRange => maxRange;
    public float LineWidth => lineWidth;
    public float LineEndPadding => lineEndPadding;
    public TargetingStrategyType StrategyType => strategyType;

    private void Awake()
    {
        strategy = CreateStrategy(strategyType);
    }

    public void RegisterTarget(Transform target)
    {
        if (target == null) return;

        var instanceId = target.GetInstanceID();
        if (targetLookup.TryGetValue(instanceId, out var registeredEntry))
        {
            if (registeredEntry.Target == target) return;

            targetLookup.Remove(instanceId);
            targets.Remove(registeredEntry);
        }

        CombatTargetResolver.TryResolve(
            target,
            damageableSearchBuffer,
            out var damageable,
            out var identity);

        var entry = new TargetEntry(instanceId, target, damageable, identity);
        targetLookup.Add(instanceId, entry);
        targets.Add(entry);
    }

    public void UnregisterTarget(Transform target)
    {
        if (target == null) return;

        var instanceId = target.GetInstanceID();
        if (!targetLookup.TryGetValue(instanceId, out var entry)) return;

        targetLookup.Remove(instanceId);
        targets.Remove(entry);
    }

    public int GetActiveTargetCount()
    {
        CleanupTargets();
        return targets.Count;
    }

    public void GetTargetsSnapshot(List<Transform> buffer)
    {
        if (buffer == null) return;

        CleanupTargets();
        buffer.Clear();

        for (int i = 0; i < targets.Count; i++)
        {
            var target = targets[i].Target;
            if (target != null)
            {
                buffer.Add(target);
            }
        }
    }

    public bool TryResolveDamageable(Transform target, out DamageSystem.IDamageable damageable)
    {
        damageable = null;
        if (target == null) return false;

        if (targetLookup.TryGetValue(target.GetInstanceID(), out var entry))
        {
            damageable = entry.Damageable;
            return entry.HasDamageable && CombatTargetResolver.IsAlive(damageable);
        }

        return CombatTargetResolver.TryResolve(
            target,
            damageableSearchBuffer,
            out damageable,
            out _);
    }

    public Transform ResolveTargetIdentity(Transform target)
    {
        if (target == null) return null;

        if (targetLookup.TryGetValue(target.GetInstanceID(), out var entry))
        {
            return entry.Identity != null ? entry.Identity : target;
        }

        CombatTargetResolver.TryResolve(
            target,
            damageableSearchBuffer,
            out _,
            out var identity);
        return identity != null ? identity : target;
    }

    public void SetStrategy(TargetingStrategyType type)
    {
        strategyType = type;
        strategy = CreateStrategy(type);
    }

    private void CleanupTargets()
    {
        for (int i = targets.Count - 1; i >= 0; i--)
        {
            var entry = targets[i];
            if (entry.IsSelectable()) continue;

            targetLookup.Remove(entry.InstanceId);
            targets.RemoveAt(i);
        }
    }

    private ITargetingStrategy CreateStrategy(TargetingStrategyType type)
    {
        switch (type)
        {
            case TargetingStrategyType.ForwardCone:
                return new ForwardConeTargetStrategy();
            case TargetingStrategyType.Line:
                return new LineTargetStrategy();
            default:
                return new NearestTargetStrategy();
        }
    }

    private sealed class TargetEntry
    {
        public TargetEntry(
            int instanceId,
            Transform target,
            DamageSystem.IDamageable damageable,
            Transform identity)
        {
            InstanceId = instanceId;
            Target = target;
            Damageable = damageable;
            Identity = identity != null ? identity : target;
            HasDamageable = CombatTargetResolver.IsAlive(damageable);
        }

        public int InstanceId { get; }
        public Transform Target { get; }
        public DamageSystem.IDamageable Damageable { get; }
        public Transform Identity { get; }
        public bool HasDamageable { get; }

        public bool IsSelectable()
        {
            if (Target == null) return false;
            if (!Target.gameObject.activeInHierarchy) return false;
            if (!HasDamageable) return true;
            if (!CombatTargetResolver.IsAlive(Damageable)) return false;

            return !Damageable.IsDead;
        }
    }
}
