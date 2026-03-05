using System.Collections.Generic;
using UnityEngine;

public partial class SlashDashController
{
    private int CalculateDamage(TimingGrade grade, AttackSpecSO spec)
    {
        if (spec == null) return 0;
        var baseDamage = Mathf.Max(0, spec.baseDamage + equipmentAttackBonus);
        switch (grade)
        {
            case TimingGrade.Perfect:
                return Mathf.RoundToInt(baseDamage * spec.criticalMultiplier);
            case TimingGrade.Good:
                if (equipmentCriticalChanceBonus > 0f)
                {
                    var criticalChance = Mathf.Clamp01(equipmentCriticalChanceBonus * 0.01f);
                    if (Random.value <= criticalChance)
                    {
                        return Mathf.RoundToInt(baseDamage * spec.criticalMultiplier);
                    }
                }

                return baseDamage;
            default:
                return 0;
        }
    }

    private void ApplyPendingDamage()
    {
        if (damageSystem == null || pendingDamage <= 0)
        {
            ClearPendingDamage();
            return;
        }

        var hitCount = 0;
        if (pendingPierceTargets.Count > 0)
        {
            for (int i = 0; i < pendingPierceTargets.Count; i++)
            {
                var target = pendingPierceTargets[i];
                if (target == null) continue;
                damageSystem.ApplyDamage(target, pendingDamage);
                hitCount++;
            }
        }
        else if (pendingTarget != null)
        {
            damageSystem.ApplyDamage(pendingTarget, pendingDamage);
            hitCount++;
        }

        ApplyEquipmentHeal(hitCount);
        ClearPendingDamage();
    }

    private void ApplyPendingDamageSafely()
    {
        try
        {
            ApplyPendingDamage();
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning($"대미지 적용 중 예외가 발생해 공격 후처리를 정리합니다.\n{exception}");
            ClearPendingDamage();
        }
    }

    private void ClearPendingDamage()
    {
        pendingTarget = null;
        pendingDamage = 0;
        contactStopTriggered = false;
        impactTriggered = false;
        pendingPierceTargets.Clear();
    }

    public bool TryStartAutoSlash(Transform target, Vector3 aimDirection, float aimDistance, TimingGrade grade, float damageMultiplier)
    {
        if (target == null) return false;
        if (!TryStartDash(target, aimDirection, aimDistance, spec)) return false;
        if (grade == TimingGrade.Miss) return true;
        if (spec == null) return true;

        pendingPierceTargets.Clear();
        pendingTarget = target;
        var multiplier = Mathf.Max(0f, damageMultiplier);
        pendingDamage = Mathf.RoundToInt(CalculateDamage(grade, spec) * multiplier);
        return true;
    }

    public bool TryStartAutoSlashPierce(Transform target, Vector3 aimDirection, float aimDistance, TimingGrade grade, float damageMultiplier, List<Transform> pierceTargets)
    {
        if (target == null) return false;
        if (!TryStartDash(target, aimDirection, aimDistance, spec)) return false;
        if (grade == TimingGrade.Miss) return true;
        if (spec == null) return true;

        pendingPierceTargets.Clear();
        pendingTarget = target;
        var multiplier = Mathf.Max(0f, damageMultiplier);
        pendingDamage = Mathf.RoundToInt(CalculateDamage(grade, spec) * multiplier);

        if (pierceTargets != null)
        {
            for (int i = 0; i < pierceTargets.Count; i++)
            {
                var candidate = pierceTargets[i];
                if (candidate == null) continue;
                if (!pendingPierceTargets.Contains(candidate))
                {
                    pendingPierceTargets.Add(candidate);
                }
            }
        }

        if (pendingPierceTargets.Count == 0 && target != null)
        {
            pendingPierceTargets.Add(target);
        }

        return true;
    }

    private void ApplyEquipmentHeal(int hitCount)
    {
        if (equipmentHealOnHit <= 0 || hitCount <= 0) return;

        var playerHp = ResolvePlayerHp();
        if (playerHp == null) return;

        var maxHp = Mathf.Max(1, playerHp.maxHP);
        var currentHp = Mathf.Clamp(playerHp.currentHP, 0, maxHp);
        var healAmount = equipmentHealOnHit * hitCount;
        var nextHp = Mathf.Clamp(currentHp + healAmount, 0, maxHp);
        if (nextHp == currentHp) return;

        playerHp.currentHP = nextHp;
        playerHp.TakeDamage(0);
    }

    private PlayerHP ResolvePlayerHp()
    {
        if (cachedPlayerHp != null && cachedPlayerHp.gameObject.activeInHierarchy)
        {
            return cachedPlayerHp;
        }

        cachedPlayerHp = GetComponent<PlayerHP>();
        if (cachedPlayerHp == null) cachedPlayerHp = GetComponentInParent<PlayerHP>();
        if (cachedPlayerHp == null) cachedPlayerHp = FindObjectOfType<PlayerHP>();
        return cachedPlayerHp;
    }
}
