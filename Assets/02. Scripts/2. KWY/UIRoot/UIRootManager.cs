using UnityEngine;

public class UIRootManager : MonoBehaviour
{
    [Header("Main Panels")]
    [SerializeField] GameObject inventoryPanel;
    [SerializeField] GameObject shopPanel;
    [SerializeField] GameObject gachaPanel;
    [SerializeField] GameObject dailyRewardPanel;
    [SerializeField] GameObject settingPanel;

    [Header("Inventory Sub Panels")]
    [SerializeField] GameObject inventoryScrollPanel;
    [SerializeField] GameObject craftingScrollPanel;
    [SerializeField] GameObject craftingUIPanel;
    [SerializeField] GameObject itemDisassemblyScrollPanel;
    [SerializeField] GameObject EquipmentEnhancementPanel;
    [SerializeField] GameObject gachaResultOne;
    [SerializeField] GameObject gachaResultTen;


    [Header("Crafting Panels")]
    [SerializeField] GameObject succesPanel;
    [SerializeField] GameObject failPanel;

    [SerializeField] BottomNavController bottomNav;

    private void Awake()
    {
        ApplyBuildScreenSetup();
    }

    private void Start()
    {
        ApplyBuildScreenSetup();
    }

    private void ApplyBuildScreenSetup()
    {
#if UNITY_EDITOR
        return;
#else
#if UNITY_ANDROID || UNITY_IOS
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.orientation = ScreenOrientation.Portrait;
#endif
#if UNITY_STANDALONE || UNITY_WEBGL
        if (Screen.width > Screen.height)
        {
            Screen.SetResolution(Screen.height, Screen.width, false);
        }
#endif
#endif
    }

    public void OpenInventory()
    {
        inventoryPanel.SetActive(true);
        shopPanel.SetActive(false);
        gachaPanel.SetActive(false);
        dailyRewardPanel.SetActive(false);
        settingPanel.SetActive(false);

        inventoryScrollPanel.SetActive(true);
        craftingScrollPanel.SetActive(false);
        craftingUIPanel.SetActive(false);
        itemDisassemblyScrollPanel.SetActive(false);
        EquipmentEnhancementPanel.SetActive(false);
        gachaResultOne.SetActive(false);
        gachaResultTen.SetActive(false);

        bottomNav.Select(0);

    }

    public void OpenShop()
    {
        inventoryPanel.SetActive(false);
        shopPanel.SetActive(true);
        gachaPanel.SetActive(false);
        dailyRewardPanel.SetActive(false);
        settingPanel.SetActive(false);

        gachaResultOne.SetActive(false);
        gachaResultTen.SetActive(false);
    }
    public void OpenGacha()
    {
        inventoryPanel.SetActive(false);
        shopPanel.SetActive(false);
        gachaPanel.SetActive(true);
        dailyRewardPanel.SetActive(false);
        settingPanel.SetActive(false);

        gachaResultOne.SetActive(false);
        gachaResultTen.SetActive(false);

    }

    public void OpenSetting()
    {
        inventoryPanel.SetActive(false);
        shopPanel.SetActive(false);
        gachaPanel.SetActive(false);
        dailyRewardPanel.SetActive(false);
        settingPanel.SetActive(true);

        gachaResultOne.SetActive(false);
        gachaResultTen.SetActive(false);
    }

    public void OpenDailyReward()
    {
        inventoryPanel.SetActive(false);
        shopPanel.SetActive(false);
        gachaPanel.SetActive(false);
        dailyRewardPanel.SetActive(true);
        settingPanel.SetActive(false);

        gachaResultOne.SetActive(false);
        gachaResultTen.SetActive(false);
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
