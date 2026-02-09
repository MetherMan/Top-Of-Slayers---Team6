using UnityEngine;

public partial class PlayerMoveController
{
    public bool IsMovementLocked => movementLocked;
    public bool IsRotationLocked => rotationLocked || rotationLockCount > 0;

    private void HandleChainSlowStateChanged(bool isActive)
    {
        ApplyChainLock(isActive);
    }

    private void ApplyChainLock(bool isActive)
    {
        if (isActive)
        {
            if (!lockMovementDuringChain) return;
            if (chainLockApplied) return;
            chainLockApplied = true;
            AddMovementLock();
            return;
        }

        if (!chainLockApplied) return;
        chainLockApplied = false;
        RemoveMovementLock();
    }

    private void ResolveChainCombat()
    {
        if (chainCombat != null) return;
        chainCombat = GetComponent<ChainCombatController>();
        if (chainCombat == null) chainCombat = GetComponentInParent<ChainCombatController>();
        if (chainCombat == null) chainCombat = FindObjectOfType<ChainCombatController>();
    }

    public void SetMovementLocked(bool locked)
    {
        if (locked)
        {
            AddMovementLock();
        }
        else
        {
            RemoveMovementLock();
        }
    }

    public void SetRotationLocked(bool locked)
    {
        rotationLocked = locked;
    }

    public void AddRotationLock()
    {
        rotationLockCount++;
    }

    public void RemoveRotationLock()
    {
        rotationLockCount = Mathf.Max(0, rotationLockCount - 1);
    }

    public void AddMovementLock()
    {
        movementLockCount++;
        movementLocked = movementLockCount > 0;
    }

    public void RemoveMovementLock()
    {
        movementLockCount = Mathf.Max(0, movementLockCount - 1);
        movementLocked = movementLockCount > 0;
    }
}
