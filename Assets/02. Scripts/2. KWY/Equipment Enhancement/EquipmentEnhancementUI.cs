using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentEnhancementUI : MonoBehaviour
{
    [SerializeField] Image beforeImage;
    [SerializeField] Image afterImage;

    [SerializeField] Sprite defaultBeforeSprite;
    [SerializeField] Sprite defaultAfterSprite;

    [SerializeField] TextMeshProUGUI beforeStatText;
    [SerializeField] TextMeshProUGUI afterStatText;
    [SerializeField] TextMeshProUGUI costText;

    [SerializeField] GameObject inventoryPanel;
    [SerializeField] GameObject failPanel;
    [SerializeField] TextMeshProUGUI failText;

    [SerializeField] EquipmentEnhancementSystem enhancementSystem;
    [SerializeField] InventorySelection inventorySelection;

    InventoryItem selectedItem;

    private void OnDisable()
    {
        ResetUI();
    }

    public void OnClickSelectItem()
    {
        inventoryPanel.SetActive(true);
        inventorySelection.EnableSelectMode(OnItemSelected);
    }

    public void OnItemSelected(InventoryItem data)
    {
        if (!(data.item is EquipmentSO equip)) 
        {
            failText.text = "재료아이템은\r\n 분해하지 못합니다.";
            failPanel.SetActive(true );
            inventorySelection.EnableSelectMode(OnItemSelected);
            return;
        }

        selectedItem = data;

        beforeImage.sprite = equip.sprite;
        afterImage.sprite = equip.sprite;

        RefreshUI();

        inventoryPanel.SetActive(false);
    }

    public void OnClickEnhance()
    {
        if (selectedItem == null) return;

        enhancementSystem.TryEnhance(selectedItem);

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (selectedItem == null) return;

        EquipmentSO equip = selectedItem.item as EquipmentSO;

        int currentLevel = selectedItem.enhancementLevel;
        int nextLevel = currentLevel + 1;

        beforeStatText.text = equip.GetStatText(currentLevel);
        afterStatText.text = equip.GetStatText(nextLevel);

        int cost = enhancementSystem.GetCost(currentLevel);
        costText.text = cost.ToString();
    }


    private void ResetUI()
    {
        selectedItem = null;

        beforeImage.sprite = defaultBeforeSprite;
        afterImage.sprite = defaultAfterSprite;

        beforeStatText.text = "";
        afterStatText.text = "";
        costText.text = "";
    }
}
