using UnityEngine;
using System;
using Cysharp.Threading.Tasks;

public class CurrencyManager : Singleton<CurrencyManager>
{
    private int gold;
    [SerializeField] public bool isCompleted = false;

    public event Action OnGoldChanged;
    protected override void Awake()
    {
        base.Awake();

        if (Instance == this)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private async void Start()
    {
        try
        {
            await WithUntilDataloaded();
            Init();
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"CurrencyManager Start중 Error : {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void Init()
    {
        gold = FirebaseManager.Instance.RefreshGold();
        isCompleted = true;
    }

    private async UniTask WithUntilDataloaded()
    {
        while (!FirebaseManager.Instance.IsDataLoaded)
        {
            await UniTask.Delay(500);
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
        FirebaseManager.Instance.SaveGold(gold);
    }
    // 골드 증가
    public void Add(int amount)
    {
        gold += amount;
        OnGoldChanged?.Invoke();
        FirebaseManager.Instance.SaveGold(gold);
    }
}
