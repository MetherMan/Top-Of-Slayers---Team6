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

    //아이템 추가
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
    //소비아이템 사용(가챠권 사용때 사용 예정)
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
    //특정 아이템 보유 수량 조회
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

    //제작/분해 아이템 제거
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
