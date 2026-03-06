using UnityEngine;

public partial class AutoSlashController
{
    public int CurrentAttackCost
    {
        get
        {
            EnsureCombatResource();
            return combatResource != null ? combatResource.CurrentAttackCost : 0;
        }
    }

    public int MaxAttackCost
    {
        get
        {
            EnsureCombatResource();
            return combatResource != null ? combatResource.MaxAttackCost : 0;
        }
    }

    private void EnsureCombatResource()
    {
        if (combatResource != null) return;
        combatResource = GetComponent<PlayerCombatResource>();
        if (combatResource == null) combatResource = GetComponentInParent<PlayerCombatResource>();
        if (combatResource == null) combatResource = FindObjectOfType<PlayerCombatResource>();
    }

    private int GetAttackCostPerUse()
    {
        if (useSpecAttackCost && spec != null)
        {
            return Mathf.Max(0, spec.attackCost);
        }

        return Mathf.Max(0, manualAttackCost);
    }

    private bool CanStartAttackByCost()
    {
        EnsureCombatResource();
        if (combatResource == null) return true;
        return combatResource.CanStartAttack(IsChainActive(), GetAttackCostPerUse());
    }

    private void ConsumeAttackCost()
    {
        EnsureCombatResource();
        if (combatResource == null) return;
        combatResource.ConsumeAttackCost(IsChainActive(), GetAttackCostPerUse());
    }
}
