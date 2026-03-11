using TMPro;
using UnityEngine;

public class EnergyUI : MonoBehaviour
{
    [SerializeField] private EnergyManager energyManager;
    [SerializeField] private TextMeshProUGUI energyText;

    private void OnEnable()
    {
        energyManager.OnEnergyChanged += UpdateUI;
    }
    private void OnDisable()
    {
        energyManager.OnEnergyChanged -= UpdateUI;
    }
    private void UpdateUI(int current, int max)
    {
        energyText.text = current + " / " + max;
    }
}
