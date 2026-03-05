using System;
using UnityEngine;

public class DummyTarget : MonoBehaviour, DamageSystem.IDamageable
{
    [Header("연동")]
    [SerializeField] private TargetingSystem targeting;
    [SerializeField] private EnemyBase enemyBase;
    [SerializeField] private EnemyConfigSO enemySO;

    public int maxHp;
    public int currentHp;

    public event Action<int, int> OnHPChanged;

    private void OnEnable()
    {
        if(enemyBase == null)
        {
            enemyBase = GetComponent<EnemyBase>();
        }

        if (targeting == null)
        {
            targeting = FindObjectOfType<TargetingSystem>();
        }

        if (targeting != null)
        {
            targeting.RegisterTarget(transform);
        }

        if(enemySO == null && enemyBase != null)
        {
            enemySO = enemyBase.GetEnemySO();
        }

        if(enemySO != null)
        {
            maxHp = enemySO.maxHp;
            currentHp = maxHp;
        }

        OnHPChanged?.Invoke(currentHp, maxHp);
    }

    private void Start()
    {
        EnemyHPUIManager.Instance.CreateHPBar(this);
    }

    private void OnDisable()
    {
        if (targeting != null)
        {
            targeting.UnregisterTarget(transform);
        }
    }

    public void ApplyDamage(int amount)
    {
        if (amount <= 0) return;

        currentHp -= amount;

        if (currentHp <= 0)
        {
            Die();
        }

        OnHPChanged?.Invoke(currentHp, maxHp);
    }

    private void Die()
    {
        if(enemyBase != null)
        {
            enemyBase.Die();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    public bool IsDead => currentHp <= 0;
}
