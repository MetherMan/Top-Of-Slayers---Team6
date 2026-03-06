using System;
using UnityEngine;

public class PlayerCombatResource : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private ChainCombatController chainCombat;
    [SerializeField] private PlayerStateMachine playerStateMachine;

    [Header("체력")]
    [SerializeField, Min(1)] private int maxHp = 3;
    [SerializeField, Min(1)] private int startHp = 3;

    [Header("공격 코스트")]
    [SerializeField, Min(0)] private int maxAttackCost = 10;
    [SerializeField, Min(0)] private int startAttackCost = 10;

    private int currentHp;
    private int currentAttackCost;
    private bool hasExternalHpConfiguration;
    private bool hasAwakened;

    public int CurrentHp => currentHp;
    public int MaxHp => maxHp;
    public int CurrentAttackCost => currentAttackCost;
    public int MaxAttackCost => maxAttackCost;
    public bool IsDead => currentHp <= 0;
    public bool IsChainInvulnerable => chainCombat != null && chainCombat.IsSlowActive;

    public event Action<int, int> OnHpChanged;
    public event Action<int, int> OnAttackCostChanged;
    public event Action<int> OnEnemyDamageIgnored;
    public event Action OnDead;

    private void Awake()
    {
        ResolveRefs();
        InitializeValues();
        hasAwakened = true;

        if (playerStateMachine != null && currentHp <= 0)
        {
            playerStateMachine.SetDead(true);
        }
    }

    private void OnValidate()
    {
        maxHp = Mathf.Max(1, maxHp);
        startHp = Mathf.Clamp(startHp, 1, maxHp);
        maxAttackCost = Mathf.Max(0, maxAttackCost);
        startAttackCost = Mathf.Clamp(startAttackCost, 0, maxAttackCost);
    }

    private void ResolveRefs()
    {
        if (chainCombat == null) chainCombat = GetComponent<ChainCombatController>();
        if (chainCombat == null) chainCombat = GetComponentInParent<ChainCombatController>();
        if (chainCombat == null) chainCombat = FindObjectOfType<ChainCombatController>();

        if (playerStateMachine == null) playerStateMachine = GetComponent<PlayerStateMachine>();
        if (playerStateMachine == null) playerStateMachine = GetComponentInParent<PlayerStateMachine>();
        if (playerStateMachine == null) playerStateMachine = FindObjectOfType<PlayerStateMachine>();
    }

    private void InitializeValues()
    {
        maxHp = Mathf.Max(1, maxHp);
        startHp = Mathf.Clamp(startHp, 1, maxHp);
        maxAttackCost = Mathf.Max(0, maxAttackCost);
        startAttackCost = Mathf.Clamp(startAttackCost, 0, maxAttackCost);

        currentHp = hasExternalHpConfiguration
            ? Mathf.Clamp(currentHp, 0, maxHp)
            : Mathf.Clamp(startHp, 1, maxHp);
        currentAttackCost = Mathf.Clamp(startAttackCost, 0, maxAttackCost);
        OnHpChanged?.Invoke(currentHp, maxHp);
        OnAttackCostChanged?.Invoke(currentAttackCost, maxAttackCost);
    }

    public bool CanStartAttack(bool isChainActive, int attackCost)
    {
        if (IsDead) return false;

        var requiredCost = Mathf.Max(0, attackCost);
        if (requiredCost <= 0) return true;
        if (isChainActive) return true;
        return currentAttackCost >= requiredCost;
    }

    public void ConsumeAttackCost(bool isChainActive, int attackCost)
    {
        var requiredCost = Mathf.Max(0, attackCost);
        if (requiredCost <= 0) return;
        if (isChainActive) return;

        var nextCost = Mathf.Max(0, currentAttackCost - requiredCost);
        if (nextCost == currentAttackCost) return;

        currentAttackCost = nextCost;
        OnAttackCostChanged?.Invoke(currentAttackCost, maxAttackCost);
    }

    public bool TakeEnemyHit(int amount)
    {
        if (amount <= 0) return false;
        if (IsDead) return false;

        if (IsChainInvulnerable)
        {
            OnEnemyDamageIgnored?.Invoke(amount);
            return false;
        }

        ApplyHpState(currentHp - amount, maxHp);
        return true;
    }

    public void RestoreHp(int amount)
    {
        if (amount <= 0) return;
        if (IsDead) return;

        ApplyHpState(currentHp + amount, maxHp);
    }

    public void RestoreAttackCost(int amount)
    {
        if (amount <= 0) return;

        var nextCost = Mathf.Min(maxAttackCost, currentAttackCost + amount);
        if (nextCost == currentAttackCost) return;

        currentAttackCost = nextCost;
        OnAttackCostChanged?.Invoke(currentAttackCost, maxAttackCost);
    }

    public void SetAttackCost(int amount)
    {
        var nextCost = Mathf.Clamp(amount, 0, maxAttackCost);
        if (nextCost == currentAttackCost) return;

        currentAttackCost = nextCost;
        OnAttackCostChanged?.Invoke(currentAttackCost, maxAttackCost);
    }

    public void SetHp(int amount)
    {
        hasExternalHpConfiguration = true;
        ApplyHpState(amount, maxHp);
    }

    public void ConfigureHp(int nextMaxHp, int nextCurrentHp)
    {
        hasExternalHpConfiguration = true;
        ApplyHpState(nextCurrentHp, nextMaxHp);
    }

    private void ApplyHpState(int nextHp, int nextMaxHp, bool notifyWhenUnchanged = false)
    {
        ResolveRefs();

        nextMaxHp = Mathf.Max(1, nextMaxHp);
        nextHp = Mathf.Clamp(nextHp, 0, nextMaxHp);

        var previousHp = currentHp;
        var previousMaxHp = maxHp;
        var wasDead = previousHp <= 0;

        maxHp = nextMaxHp;
        startHp = Mathf.Clamp(startHp, 1, maxHp);
        currentHp = nextHp;

        if (notifyWhenUnchanged || previousHp != currentHp || previousMaxHp != maxHp)
        {
            OnHpChanged?.Invoke(currentHp, maxHp);
        }

        var isDeadNow = currentHp <= 0;
        if (playerStateMachine != null)
        {
            if (isDeadNow && !wasDead)
            {
                playerStateMachine.SetDead(true);
            }
            else if (hasAwakened && !isDeadNow && wasDead)
            {
                playerStateMachine.SetDead(false);
            }
        }

        if (!wasDead && isDeadNow)
        {
            OnDead?.Invoke();
        }
    }
}
