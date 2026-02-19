using UnityEngine;

public class ShopView : MonoBehaviour
{
    [SerializeField] ShopManager shopManager;
    [SerializeField] Transform slotRoot;

    ShopItemSlot[] slots;
    //슬롯 찾고 UI갱신
    private void OnEnable()
    {
        slots = slotRoot.GetComponentsInChildren<ShopItemSlot>();
        RefreshUI();
    }
    //shopManager 데이터 기반 슬롯 채우기
    void RefreshUI()
    {
        var shopItems = shopManager.GetShopItems();

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < shopItems.Count)
            {
                slots[i].SetItem(shopItems[i]);
            }
            else
            {
                slots[i].ClearItem();
            }
        }
    }
}
