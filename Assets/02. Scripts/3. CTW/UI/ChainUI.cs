using TMPro;
using UnityEngine;

public class ChainUI : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private ChainComboSystem chainComboSystem;

    [Header("Settings")]
    [SerializeField] private GameObject chainPanel;
    [SerializeField] private TextMeshProUGUI chainText;

    public bool IsReady => chainPanel != null && chainText != null;
    public bool IsVisible => chainPanel != null && chainPanel.activeSelf;

    public void UpdateChainUI(int chain)
    {
        if (!IsReady) return;

        if (!chainPanel.activeSelf)
        {
            chainPanel.SetActive(true);
        }

        chainText.text = $"X{chain}";
    }

    public void HideChainUI(int finalChain)
    {
        if (chainPanel == null) return;
        chainPanel.SetActive(false);
    }
}