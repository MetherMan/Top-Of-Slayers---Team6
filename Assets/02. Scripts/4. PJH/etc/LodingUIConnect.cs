using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LodingUIConnect : MonoBehaviour
{
    #region field
    [SerializeField] public GameObject lodingUI;
    [SerializeField] public Slider loadingbar;
    [SerializeField] public TextMeshProUGUI percent;
    #endregion

    void Start()
    {
        AddressableManager.Instance.TakeObject(loadingbar, percent);
    }
}
