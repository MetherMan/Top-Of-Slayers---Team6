using System;
using UnityEngine;

public class PlayerHP : MonoBehaviour
{
    [SerializeField] public int maxHP = 100;
    public int currentHP;

    public event Action<int, int> OnHPChanged;

    private void Awake()
    {
        currentHP = maxHP;

        OnHPChanged?.Invoke(currentHP, maxHP);
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log($"공격받음{currentHP}");
        OnHPChanged?.Invoke(currentHP, maxHP);

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }

    private void Die()
    {

    }
}
