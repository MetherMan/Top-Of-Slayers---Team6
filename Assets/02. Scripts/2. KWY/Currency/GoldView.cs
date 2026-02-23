using TMPro;
using UnityEngine;

public class GoldView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI goldText;

    private void Start()
    {
        CurrencyManager.Instance.OnGoldChanged += Refresh;
        Refresh();
    }

    private void OnDestroy()
    {
        if (CurrencyManager.HasInstance)
            CurrencyManager.Instance.OnGoldChanged -= Refresh;
    }

    public void Refresh()
    {
        goldText.text = CurrencyManager.Instance.GetGold().ToString();
    }
}