using System.Collections.Generic;
using UnityEngine;

public class GachaManager : MonoBehaviour
{
    [SerializeField] GachaDataSO gachaData;
    [SerializeField] ItemSO ticketItem;


    public ItemSO TicketItem => ticketItem;

    public ItemSO RollOne()
    {
        if (!InventoryManager.Instance.HasEnoughItem(ticketItem, 1))
        {
            return null;
        }

        InventoryManager.Instance.RemoveItem(ticketItem, 1);

        ItemSO result = GachaCalculator.Roll(gachaData);
        InventoryManager.Instance.AddItem(result, 1);

        return result;
    }

    public List<ItemSO> RollTen()
    {
        if (!InventoryManager.Instance.HasEnoughItem(ticketItem, 10))
        {
            return null;
        }

        InventoryManager.Instance.RemoveItem(ticketItem, 10);

        List<ItemSO> results = new List<ItemSO>();

        for (int i = 0; i < 10; i++) 
        {
            ItemSO item = GachaCalculator.Roll(gachaData);
            results.Add(item);
            InventoryManager.Instance.AddItem(item, 1);
        }
        return results;
    }
}
