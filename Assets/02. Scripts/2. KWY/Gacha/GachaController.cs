using TMPro;
using UnityEngine;

public class GachaController : MonoBehaviour
{
    [SerializeField] GachaManager manager;
    [SerializeField] GachaResultUI resultUI;
    [SerializeField] GameObject failPanel;

    [SerializeField] TextMeshProUGUI ticketCountText;
    [SerializeField] ItemSO ticketItem;


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

        RefreshTicket();
        resultUI.ShowOne(item);
    }

    public void OnClickTen()
    {
        var items = manager.RollTen();
        if (items == null) 
        {
            failPanel.SetActive(true);
            return;

        }

        RefreshTicket();
        resultUI.ShowTen(items);
    }

    private void RefreshTicket()
    {
        int count = InventoryManager.Instance.GetItemCount(ticketItem);
        ticketCountText.text = count.ToString();
    }
}
