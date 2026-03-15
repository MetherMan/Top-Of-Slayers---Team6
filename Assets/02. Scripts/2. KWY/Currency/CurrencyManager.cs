using UnityEngine;
using System;

public class CurrencyManager : Singleton<CurrencyManager>
{
    [SerializeField] int gold = 10000;

    public event Action OnGoldChanged;
    protected override void Awake()
    {
        base.Awake();

        if (Instance == this)
        {
            DontDestroyOnLoad(gameObject);
        }
    }
    //현재 보유 골드 반환
    public int GetGold()
    {
        return gold;
    }
    //구매 가능여부 확인
    public bool HasEnough(int amount)
    {
        return gold >= amount;
    }
    //골드 차감
    public void Spend(int amount)
    {
        if (gold < amount) return;

        gold -= amount;
        OnGoldChanged?.Invoke();
    }
    // 골드 증가
    public void Add(int amount)
    {
        gold += amount;
        OnGoldChanged?.Invoke();
    }
}
