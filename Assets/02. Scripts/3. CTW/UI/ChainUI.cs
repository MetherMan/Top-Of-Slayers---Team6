using TMPro;
using UnityEngine;

public class ChainUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI chainText;

    public void OnChainChanged(int chain)
    {
        chainText.text = $"X{chain}";
    }

    public void OnChainEnded(int finalChain)
    {
        chainText.text = "";
        //사라지는 애니메이션
    }
}
