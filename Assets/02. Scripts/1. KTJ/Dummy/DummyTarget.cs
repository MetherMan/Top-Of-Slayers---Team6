using UnityEngine;

public class DummyTarget : MonoBehaviour, DamageSystem.IDamageable
{
    [Header("연동")]
    [SerializeField] private TargetingSystem targeting;
    [SerializeField] private EnemyBase enemyBase;

    [Header("스탯")]
    [SerializeField] private int hp = 30;

    private void OnEnable()
    {
        if (targeting == null)
        {
            targeting = FindObjectOfType<TargetingSystem>();
        }

        if (targeting != null)
        {
            targeting.RegisterTarget(transform);
        }
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

        hp -= amount;
        if (hp <= 0)
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
    public bool IsDead => hp <= 0;
}
