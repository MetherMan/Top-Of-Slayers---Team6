using System.Collections.Generic;
using UnityEngine;

public class GachaResultUI : MonoBehaviour
{
    [SerializeField] GameObject onePanel;
    [SerializeField] GameObject tenPanel;

    [SerializeField] Transform oneIconParent;
    [SerializeField] Transform tenIconParent;
    [SerializeField] GameObject oneIconPrefab;
    [SerializeField] GameObject tenIconPrefab;

    public void ShowOne(ItemSO item)
    {
        Clear();

        onePanel.SetActive(true);

        var icon = Instantiate(oneIconPrefab, oneIconParent).GetComponent<GachaResultIcon>();

        icon.Set(item);
    }

    public void ShowTen(List<ItemSO> items)
    {
        Clear();
        tenPanel.SetActive(true);

        foreach (var item in items)
        {
            var icon =  Instantiate(tenIconPrefab, tenIconParent)
                .GetComponent<GachaResultIcon>();

            icon.Set(item);
        }
    }

    private void Clear()
    {
        onePanel.SetActive(false);
        tenPanel.SetActive(false);

        foreach (Transform child in oneIconParent)
            Destroy(child.gameObject);

        foreach (Transform child in tenIconParent)
            Destroy(child.gameObject);
    }
}
