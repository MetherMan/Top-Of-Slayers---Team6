using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentEnhancementUI : MonoBehaviour
{
    [SerializeField] Image beforeImage;
    [SerializeField] Image afterImage;

    [SerializeField] Sprite defaultBeforeSprite;
    [SerializeField] Sprite defaultAfterSprite;

    [SerializeField] TextMeshProUGUI beforeStatText;
    [SerializeField] TextMeshProUGUI beforeLevelText;
    [SerializeField] TextMeshProUGUI afterStatText;
    [SerializeField] TextMeshProUGUI afterLevelText;
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

        beforeStatText.text = equip.GetStatText(currentLevel);
        beforeLevelText.text = $"+{currentLevel}";
        

        if (enhancementSystem.IsMaxLevel(currentLevel))
        {
            afterStatText.text = equip.GetStatText(currentLevel);
            costText.text = "";
            return;
        }

        int nextLevel = currentLevel + 1;

        afterStatText.text = equip.GetStatText(nextLevel);
        afterLevelText.text = $"+{nextLevel}";

        int cost = enhancementSystem.GetCost(currentLevel);
        costText.text = cost.ToString();

        //firebase
        //CheckPlus(equip.equipSlot, afterLevelText.text);
    }


    private void ResetUI()
    {
        selectedItem = null;

        beforeImage.sprite = defaultBeforeSprite;
        afterImage.sprite = defaultAfterSprite;

        beforeStatText.text = "";
        beforeLevelText.text = "";
        afterStatText.text = "";
        afterLevelText.text = "";
        costText.text = "";
    }

    //firebase
    private void CheckPlus(EquipSlot slot, string plus)
    {
        switch (slot)
        {
            //생성 파괴이지만 쓰고 버리는거라 메모리상 유의미?
            //필드에 변수를 두고 사용하면 생성 파괴로 인한 연산부하는 없음
            //그런데, level 변수를 굳이 필드에 만들어 둘 필요가?
            case EquipSlot.Weapon:
                {
                    int level = System.Convert.ToInt32(plus);
                    EquipmentManager.Instance.weapon.enhancementLevel = level;
                }
                break;
            case EquipSlot.Shoes:
                {
                    int level = System.Convert.ToInt32(plus);
                    EquipmentManager.Instance.shoes.enhancementLevel = level;
                }
                break;
            case EquipSlot.Gloves:
                {
                    int level = System.Convert.ToInt32(plus);
                    EquipmentManager.Instance.gloves.enhancementLevel = level;
                }
                break;
            case EquipSlot.Armor:
                {
                    int level = System.Convert.ToInt32(plus);
                    EquipmentManager.Instance.armor.enhancementLevel = level;
                }
                break;
            case EquipSlot.Ring:
                {
                    int level = System.Convert.ToInt32(plus);
                    EquipmentManager.Instance.emblem.enhancementLevel = level;
                }
                break;
        }
    }
}
