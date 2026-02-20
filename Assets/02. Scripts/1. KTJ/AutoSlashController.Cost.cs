using UnityEngine;

public partial class AutoSlashController
{
    public int CurrentAttackCost => combatResource != null ? combatResource.CurrentAttackCost : 0;
    public int MaxAttackCost => combatResource != null ? combatResource.MaxAttackCost : 0;

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
        if (combatResource == null) return true;
        return combatResource.CanStartAttack(IsChainActive(), GetAttackCostPerUse());
    }

    private void ConsumeAttackCost()
    {
        if (combatResource == null) return;
        combatResource.ConsumeAttackCost(IsChainActive(), GetAttackCostPerUse());
    }
}
