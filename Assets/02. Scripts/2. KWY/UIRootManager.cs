using UnityEngine;

public class UIRootManager : MonoBehaviour
{
    [Header("Main Panels")]
    [SerializeField] GameObject inventoryPanel;
    [SerializeField] GameObject shopPanel;
    [SerializeField] GameObject gachaPanel;
    [SerializeField] GameObject dailyRewardPanel;

    [Header("Inventory Sub Panels")]
    [SerializeField] GameObject inventoryScrollPanel;
    [SerializeField] GameObject craftingScrollPanel;
    [SerializeField] GameObject craftingUIPanel;
    [SerializeField] GameObject itemDisassemblyScrollPanel;
    [SerializeField] GameObject EquipmentEnhancementPanel;

    [Header("Crafting Panels")]
    [SerializeField] GameObject succesPanel;
    [SerializeField] GameObject failPanel;

    [SerializeField] BottomNavController bottomNav;

    public void OpenInventory()
    {
        inventoryPanel.SetActive(true);
        shopPanel.SetActive(false);
        gachaPanel.SetActive(false);
        dailyRewardPanel.SetActive(false);

        inventoryScrollPanel.SetActive(true);
        craftingScrollPanel.SetActive(false);
        craftingUIPanel.SetActive(false);
        itemDisassemblyScrollPanel.SetActive(false);
        EquipmentEnhancementPanel.SetActive(false);

        bottomNav.Select(0);

    }

    public void OpenShop()
    {
        inventoryPanel.SetActive(false);
        shopPanel.SetActive(true);
        gachaPanel.SetActive(false);
        dailyRewardPanel.SetActive(false);
    }
    public void OpenGacha()
    {
        inventoryPanel.SetActive(false);
        shopPanel.SetActive(false);
        gachaPanel.SetActive(true);
        dailyRewardPanel.SetActive(false);
    }

    public void OpenDailyReward()
    {
        inventoryPanel.SetActive(false);
        shopPanel.SetActive(false);
        gachaPanel.SetActive(false);
        dailyRewardPanel.SetActive(true);
    }
    public void OpenInventoryScroll()
    {
        inventoryScrollPanel.SetActive(true);
        craftingScrollPanel.SetActive(false);
        craftingUIPanel.SetActive(false);
        itemDisassemblyScrollPanel.SetActive(false);
        EquipmentEnhancementPanel.SetActive(false);
    }

    public void OpenCraftList()
    {
        inventoryScrollPanel.SetActive(false);
        craftingScrollPanel.SetActive(true);
        craftingUIPanel.SetActive(false);
        itemDisassemblyScrollPanel.SetActive(false);
        EquipmentEnhancementPanel.SetActive(false);
    }
    //public void OpenCraftingUI()
    //{
    //    inventoryScrollPanel.SetActive(false);
    //    craftingScrollPanel.SetActive(false);
    //    craftingUIPanel.SetActive(true);
    //    itemDisassemblyScrollPanel.SetActive(false);
    //    EquipmentEnhancementPanel.SetActive(false);
    //}
    public void OpenItemDisassemblyScroll()
    {
        inventoryScrollPanel.SetActive(false);
        craftingScrollPanel.SetActive(false);
        craftingUIPanel.SetActive(false);
        itemDisassemblyScrollPanel.SetActive(true);
        EquipmentEnhancementPanel.SetActive(false);
    }

    public void OpenEquipmentEnhancement()
    {
        inventoryScrollPanel.SetActive(false);
        craftingScrollPanel.SetActive(false);
        craftingUIPanel.SetActive(false);
        itemDisassemblyScrollPanel.SetActive(false);
        EquipmentEnhancementPanel.SetActive(true);
    }

    public void OpenCraftingSuccesPanel()
    {
        succesPanel.SetActive(true);
    }
    public void OpenCraftingFailPanel()
    {
        failPanel.SetActive(true);
    }
    public void OpenPanel(GameObject target)
    {
        target.SetActive(true);
    }

    public void CloseCurrent(GameObject target)
    {
        target.SetActive(false);
    }
}
