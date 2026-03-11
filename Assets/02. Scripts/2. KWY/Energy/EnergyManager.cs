using System;
using UnityEngine;

public class EnergyManager : MonoBehaviour
{
    [SerializeField] private int currentEnergy = 60;
    [SerializeField] private int maxEnergy = 60;
    [SerializeField] private int addEnergyMinutes = 5;
    public Action<int, int> OnEnergyChanged;

    private void OnEnable()
    {
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
    }

    public bool UseEnergy(int amount)
    {
        if(currentEnergy < amount)
        {
            //에너지 부족
            return false;
        }
        currentEnergy -= amount;

        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        return true;
    }
    public void AddEnergy(int amount)
    {
        //몇분마다 에너지 증가

        currentEnergy += amount;

        if(currentEnergy > maxEnergy)
        {
            currentEnergy = maxEnergy;
        }

        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
    }
}
