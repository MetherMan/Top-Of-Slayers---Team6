using UnityEngine;
using System.Collections.Generic;
using System;

public class InventoryManager : Singleton<InventoryManager>
{
    public List<InventoryItem> inventory = new List<InventoryItem>();
    public event Action OnInventoryChanged;
    protected override void Awake()
    {
        base.Awake();
    }
    public void AddItem(ItemSO item, int amount)
    {

        if (item == null) return;

        bool found = false;
        foreach (var slot in inventory)
        {
            if (slot.item == item)
            {
                slot.count += amount;
                found = true;
                break;
            }
        }
        if (!found)
        {
            inventory.Add(new InventoryItem { item = item, count = amount });

        }
        OnInventoryChanged?.Invoke();
    }
    public bool UseItem(ItemSO item, int amount = 1)
    {
        for(int i = 0; i < inventory.Count; i++)
        {
            if(inventory[i].item == item)
            {
                inventory[i].count -= amount;
                if(inventory[i].count <= 0)
                {
                    inventory.RemoveAt(i);
                }
                OnInventoryChanged?.Invoke();
                return true;
            }
        }
        return false;
    }
    public int GetItemCount(ItemSO item)
    {
        foreach (var slot in inventory)
        {
            if(slot.item == item)
            {
                return slot.count;
            }
        }
        return 0;
    }
    public bool HasEnoughItem(ItemSO item, int amount)
    {
        return GetItemCount(item) >= amount;
    }

    public void RemoveItem(ItemSO item, int amount)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].item == item)
            {
                inventory[i].count -= amount;

                if (inventory[i].count <= 0)
                {
                    inventory.RemoveAt(i);
                }

                OnInventoryChanged?.Invoke();
                return;
            }
        }
    }

}
