using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

public class GachaController : MonoBehaviour
{
    [SerializeField] GachaManager manager;
    [SerializeField] GachaResultUI resultUI;
    [SerializeField] GameObject failPanel;

    [SerializeField] TextMeshProUGUI ticketCountText;
    [SerializeField] ItemSO ticketItem;
    [SerializeField] GachaChestUI chestUI;

    ItemSO cachedItem;
    List<ItemSO> cachedItems;


    private void OnEnable()
    {
        RefreshTicket();
    }

    public void OnClickOne()
    {
        var item = manager.RollOne();
        if (item == null) 
        {
            failPanel.SetActive(true);
            return;
        } 
        cachedItem = item;
        RefreshTicket();

        chestUI.PlayChest(OnChestOpenedOne);


    }

    public void OnClickTen()
    {
        var items = manager.RollTen();
        if (items == null) 
        {
            failPanel.SetActive(true);
            return;

        }

        cachedItems = items;

        RefreshTicket();

        chestUI.PlayChest(OnChestOpenedTen);
    }

    private void RefreshTicket()
    {
        int count = InventoryManager.Instance.GetItemCount(ticketItem);
        ticketCountText.text = count.ToString();
    }

    private void OnChestOpenedOne()
    {
        resultUI.ShowOne(cachedItem);
    }

    private void OnChestOpenedTen()
    {
        resultUI.ShowTen(cachedItems);
    }
}
