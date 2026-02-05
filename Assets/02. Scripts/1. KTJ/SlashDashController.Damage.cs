using System.Collections.Generic;
using UnityEngine;

public partial class SlashDashController
{
    private int CalculateDamage(TimingGrade grade, AttackSpecSO spec)
    {
        if (spec == null) return 0;
        switch (grade)
        {
            case TimingGrade.Perfect:
                return Mathf.RoundToInt(spec.baseDamage * spec.criticalMultiplier);
            case TimingGrade.Good:
                return spec.baseDamage;
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

        if (pendingPierceTargets.Count > 0)
        {
            for (int i = 0; i < pendingPierceTargets.Count; i++)
            {
                var target = pendingPierceTargets[i];
                if (target == null) continue;
                damageSystem.ApplyDamage(target, pendingDamage);
            }
        }
        else if (pendingTarget != null)
        {
            damageSystem.ApplyDamage(pendingTarget, pendingDamage);
        }

        ClearPendingDamage();
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
}
