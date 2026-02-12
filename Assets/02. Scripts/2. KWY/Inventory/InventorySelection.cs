using System;
using UnityEngine;

public class InventorySelection : MonoBehaviour
{
    public static InventorySelection Instance;

    Action<ItemSO> onItemSelected;

    private void Awake()
    {
        Instance = this;
    }

    public void EnableSelectMode(Action<ItemSO> callback)
    {
        onItemSelected = callback;
    }

    public void DisableSelectMode()
    {
        onItemSelected = null;
    }

    public void NotifyItemClicked(ItemSO item) 
    {
        if (onItemSelected == null) return;

        onItemSelected.Invoke(item);
        DisableSelectMode();
    }
}
