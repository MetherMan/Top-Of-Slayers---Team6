using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class TargetingSystemTests
{
    private readonly List<GameObject> createdObjects = new List<GameObject>();

    [TearDown]
    public void TearDown()
    {
        for (int i = createdObjects.Count - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(createdObjects[i]);
        }

        createdObjects.Clear();
    }

    [Test]
    public void RegisterTarget_CachesDamageableIdentityFromHierarchy()
    {
        var system = CreateObject("TargetingSystem").AddComponent<TargetingSystem>();
        var enemy = CreateObject("Enemy");
        enemy.AddComponent<TargetingTestDamageable>();
        var hitPoint = CreateObject("HitPoint");
        hitPoint.transform.SetParent(enemy.transform);

        system.RegisterTarget(hitPoint.transform);

        Assert.That(system.TryResolveDamageable(hitPoint.transform, out var damageable), Is.True);
        Assert.That(damageable, Is.SameAs(enemy.GetComponent<TargetingTestDamageable>()));
        Assert.That(system.ResolveTargetIdentity(hitPoint.transform), Is.SameAs(enemy.transform));
    }

    [Test]
    public void DeadTarget_IsRemovedDuringNextQuery()
    {
        var system = CreateObject("TargetingSystem").AddComponent<TargetingSystem>();
        var enemy = CreateObject("Enemy");
        var damageable = enemy.AddComponent<TargetingTestDamageable>();
        system.RegisterTarget(enemy.transform);

        damageable.ApplyDamage(1);

        Assert.That(system.GetActiveTargetCount(), Is.Zero);
    }

    [Test]
    public void GetTargetsInLineNonAlloc_ReusesCallerBufferAndFiltersGeometry()
    {
        var system = CreateObject("TargetingSystem").AddComponent<TargetingSystem>();
        var inLine = CreateDamageableTarget("InLine", new Vector3(0f, 0f, 3f));
        var outsideLine = CreateDamageableTarget("OutsideLine", new Vector3(3f, 0f, 3f));
        system.RegisterTarget(inLine.transform);
        system.RegisterTarget(outsideLine.transform);

        var buffer = new List<Transform>(8) { outsideLine.transform };
        system.GetTargetsInLineNonAlloc(Vector3.zero, Vector3.forward, 8f, null, buffer);

        Assert.That(buffer, Has.Count.EqualTo(1));
        Assert.That(buffer[0], Is.SameAs(inLine.transform));
    }

    private GameObject CreateDamageableTarget(string name, Vector3 position)
    {
        var target = CreateObject(name);
        target.transform.position = position;
        target.AddComponent<TargetingTestDamageable>();
        return target;
    }

    private GameObject CreateObject(string name)
    {
        var instance = new GameObject(name);
        createdObjects.Add(instance);
        return instance;
    }

}

public sealed class TargetingTestDamageable : MonoBehaviour, DamageSystem.IDamageable
{
    public bool IsDead { get; private set; }

    public void ApplyDamage(int amount)
    {
        if (amount > 0)
        {
            IsDead = true;
        }
    }
}
