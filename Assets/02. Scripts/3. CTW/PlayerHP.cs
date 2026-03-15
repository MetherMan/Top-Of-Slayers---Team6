using System;
using UnityEngine;

public class PlayerHP : MonoBehaviour
{
    [SerializeField] public int maxHP = 100;
    public int currentHP;

    public event Action<int, int> OnHPChanged;

    private PlayerCombatResource combatResource;
    private PlayerStateMachine playerStateMachine;
    private bool combatBridgeInitialized;
    private bool isCombatResourceSubscribed;

    private void Awake()
    {
        ResolveRefs();
        maxHP = Mathf.Max(1, maxHP);
        currentHP = ResolveInitialHp();
        InitializeCombatBridge();
    }

    private void OnEnable()
    {
        ResolveRefs();
        SubscribeCombatResource();

        if (combatResource != null && combatBridgeInitialized)
        {
            SyncFromCombatResource(combatResource.CurrentHp, combatResource.MaxHp);
        }
    }

    private void Start()
    {
        if (!combatBridgeInitialized)
        {
            InitializeCombatBridge();
        }

        if (combatResource == null)
        {
            NotifyHpChanged();
            return;
        }

        if (!isCombatResourceSubscribed)
        {
            SubscribeCombatResource();
        }
    }

    private void OnDisable()
    {
        UnsubscribeCombatResource();
    }

    public void TakeDamage(int damage)
    {
        if (combatResource != null)
        {
            if (damage > 0)
            {
                if (combatResource.TakeEnemyHit(damage))
                {
                    EnemyDamageUI.Instance.ShowDamage(transform, damage);
                }
            }
            else
            {
                SyncFromCombatResource(combatResource.CurrentHp, combatResource.MaxHp);
                return;
            }

            if (!isCombatResourceSubscribed)
            {
                SyncFromCombatResource(combatResource.CurrentHp, combatResource.MaxHp);
            }
            return;
        }

        if (damage <= 0)
        {
            NotifyHpChanged();
            return;
        }

        currentHP = Mathf.Max(0, currentHP - damage);
        EnemyDamageUI.Instance.ShowDamage(transform, damage);
        NotifyHpChanged();
        Debug.Log(damage);
        if (currentHP > 0) return;

        Die();
    }

    public void RestoreHp(int amount)
    {
        if (combatResource != null)
        {
            combatResource.RestoreHp(amount);

            if (!isCombatResourceSubscribed || amount <= 0)
            {
                SyncFromCombatResource(combatResource.CurrentHp, combatResource.MaxHp);
            }
            return;
        }

        if (amount <= 0)
        {
            NotifyHpChanged();
            return;
        }

        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
        NotifyHpChanged();
    }

    public void SetHpState(int nextMaxHp, int nextCurrentHp)
    {
        if (combatResource != null)
        {
            combatResource.ConfigureHp(nextMaxHp, nextCurrentHp);

            if (!isCombatResourceSubscribed)
            {
                SyncFromCombatResource(combatResource.CurrentHp, combatResource.MaxHp);
            }
            return;
        }

        maxHP = Mathf.Max(1, nextMaxHp);
        currentHP = Mathf.Clamp(nextCurrentHp, 0, maxHP);
        NotifyHpChanged();

        if (currentHP > 0) return;

        Die();
    }

    private void Die()
    {
        if (playerStateMachine != null)
        {
            playerStateMachine.SetDead(true);
        }
    }

    private void ResolveRefs()
    {
        if (combatResource == null) combatResource = GetComponent<PlayerCombatResource>();
        if (combatResource == null) combatResource = GetComponentInParent<PlayerCombatResource>();
        if (combatResource == null) combatResource = FindObjectOfType<PlayerCombatResource>();

        if (playerStateMachine == null) playerStateMachine = GetComponent<PlayerStateMachine>();
        if (playerStateMachine == null) playerStateMachine = GetComponentInParent<PlayerStateMachine>();
    }

    private int ResolveInitialHp()
    {
        return currentHP > 0
            ? Mathf.Clamp(currentHP, 0, maxHP)
            : maxHP;
    }

    private void InitializeCombatBridge()
    {
        ResolveRefs();
        if (combatResource == null)
        {
            combatBridgeInitialized = false;
            return;
        }

        // 구버전 HP 값을 최신 전투 자원에 맞춰서 기존 호출부를 그대로 유지한다.
        combatResource.ConfigureHp(maxHP, currentHP);
        combatBridgeInitialized = true;
    }

    private void SubscribeCombatResource()
    {
        if (combatResource == null) return;

        combatResource.OnHpChanged -= HandleCombatResourceHpChanged;
        combatResource.OnHpChanged += HandleCombatResourceHpChanged;
        isCombatResourceSubscribed = true;
    }

    private void UnsubscribeCombatResource()
    {
        isCombatResourceSubscribed = false;
        if (combatResource == null) return;

        combatResource.OnHpChanged -= HandleCombatResourceHpChanged;
    }

    private void HandleCombatResourceHpChanged(int nextCurrentHp, int nextMaxHp)
    {
        SyncFromCombatResource(nextCurrentHp, nextMaxHp);
    }

    private void SyncFromCombatResource(int nextCurrentHp, int nextMaxHp)
    {
        maxHP = Mathf.Max(1, nextMaxHp);
        currentHP = Mathf.Clamp(nextCurrentHp, 0, maxHP);
        NotifyHpChanged();
    }

    private void NotifyHpChanged()
    {
        OnHPChanged?.Invoke(currentHP, maxHP);
    }
}
