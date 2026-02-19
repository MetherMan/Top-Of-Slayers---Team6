using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class GachaSystem : MonoBehaviour
{
    [SerializeField] private GachaDataSO gachaData;

    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Image resultImage;

    [SerializeField] private Transform iconParent;
    [SerializeField] private GameObject iconPrefab;

    public void OnClickOne()
    {
        ItemSO result = Gacha();
        ClearPanel();

        string message = "뽑기 결과\n";
        message += $"{result.itemName}\n";

        resultImage.gameObject.SetActive(true);
        resultImage.sprite = result.sprite;

        ResultUI(message);
    }
    public ItemSO Gacha()
    {
        if (gachaData == null || gachaData.items.Count == 0)
        {
            Debug.LogWarning("가차 테이블이 비어있음");
            return null;
        }
    
        float totalChance = gachaData.TotalChance();
        float randomPoint = Random.Range(0, totalChance);
        float current = 0f;

        foreach (var item in gachaData.items)
        {
            current += item.chance;
            if (randomPoint <= current)
            {
                float percent = (item.chance / totalChance) * 100f;

                Debug.Log($"결과: {item.item.name}({percent}%)");
                return item.item;
            }
        }
    
        return null;
    }

    public List<ItemSO> TenGacha()
    {
        List<ItemSO> results = new List<ItemSO>();
        for (int i = 0; i < 10; i++)
        {
            ItemSO result = Gacha();
            results.Add(result);
            Debug.Log($"{i + 1}번쨰 {result.itemName}");
        }
        return results;
    }

    public void OnClickTen()
    {
        List<ItemSO> results = TenGacha();

        ClearPanel();

        string message = "10연 뽑기 결과\n";
        foreach(var item in results)
        {
            GameObject iconObj = Instantiate(iconPrefab, iconParent);
            iconObj.GetComponent<Image>().sprite = item.sprite;
        }
        ResultUI(message);

        resultImage.gameObject.SetActive(false);
    }

    private void ResultUI(string message)
    {
        resultPanel.SetActive(true);
        resultText.text = message;
    }

    private void ClearPanel()
    {
        foreach(Transform child in iconParent)
        {
            Destroy(child.gameObject);
        }
    }
}
