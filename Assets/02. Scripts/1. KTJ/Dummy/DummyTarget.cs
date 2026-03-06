using System;
using UnityEngine;
using UnityEngine.Serialization;

public class DummyTarget : MonoBehaviour, DamageSystem.IDamageable
{
    [Header("연동")]
    [SerializeField] private TargetingSystem targeting;
    [SerializeField] private EnemyBase enemyBase;
    [SerializeField] private EnemyConfigSO enemySO;

    [FormerlySerializedAs("hp")]
    public int maxHp;
    public int currentHp;

    public event Action<int, int> OnHPChanged;
    private bool hpBarCreated;

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

        maxHp = ResolveSpawnHp();
        currentHp = maxHp;

        hpBarCreated = false;
        TryCreateHpBar();
        OnHPChanged?.Invoke(currentHp, maxHp);
    }

    private void Start()
    {
        TryCreateHpBar();
    }

    private void OnDisable()
    {
        hpBarCreated = false;
        if (targeting != null)
        {
            targeting.UnregisterTarget(transform);
        }
    }

    public void ApplyDamage(int amount)
    {
        if (amount <= 0) return;
        if (IsDead) return;

        currentHp = Mathf.Max(0, currentHp - amount);
        OnHPChanged?.Invoke(currentHp, maxHp);

        if (currentHp == 0)
        {
            Die();
        }
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

    private void TryCreateHpBar()
    {
        if (hpBarCreated) return;
        if (!EnemyHPUIManager.HasInstance) return;

        EnemyHPUIManager.Instance.CreateHPBar(this);
        hpBarCreated = true;
    }

    private int ResolveSpawnHp()
    {
        if (enemySO != null)
        {
            if (enemySO.maxHp > 0) return enemySO.maxHp;
            if (enemySO.hp > 0) return enemySO.hp;
        }

        return Mathf.Max(1, maxHp);
    }
}
