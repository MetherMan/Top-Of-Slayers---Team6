using TMPro;
using UnityEngine;

public class ChainUI : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private ChainComboSystem chainComboSystem;

    [Header("세팅")]
    [SerializeField] private GameObject chainPanel;
    [SerializeField] private TextMeshProUGUI chainText;

    public void UpdateChainUI(int chain)
    {
        if (chainPanel == null || chainText == null) return;

        if (!chainPanel.activeSelf)
        {
            chainPanel.SetActive(true);
        }
        chainText.text = $"X{chain}";
    }

    public void HideChainUI(int finalChain)
    {
        chainPanel.SetActive(false);
        //사라지는 애니메이션
    }
}
