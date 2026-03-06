using UnityEngine;

[CreateAssetMenu(fileName = "AttackSpec_", menuName = "Config/AttackSpec")]
public class AttackSpecSO : ScriptableObject
{
    [Header("플레이어 기본 스탯")]
    [Min(0)] public int maxHp;
    [Min(0f)] public float moveSpeed;
    [Range(0f, 100f)] public float criticalChance;
    [Min(0)] public int healOnHit;

    [Header("대미지")]
    [Min(0)] public int baseDamage = 10;
    [Min(0f)] public float criticalMultiplier = 1.5f;

    [Header("대시")]
    [Min(0f)] public float dashSpeed = 10f;
    [Min(0f)] public float dashDuration = 0.2f;

    [Header("타이밍")]
    [Range(0f, 1f)] public float perfectMin = 0.45f;
    [Range(0f, 1f)] public float perfectMax = 0.55f;
    [Range(0f, 1f)] public float goodMin = 0.3f;
    [Range(0f, 1f)] public float goodMax = 0.7f;

    [Header("쿨타임")]
    [Min(0f)] public float cooldown = 0.5f;

    [Header("코스트")]
    [Min(0)] public int attackCost = 1;

    public int GetAttack()
    {
        return Mathf.Max(0, baseDamage);
    }

    public int GetHP(int fallback)
    {
        if (maxHp > 0)
        {
            return Mathf.Max(1, maxHp);
        }

        return Mathf.Max(1, fallback);
    }

    public int GetHeal()
    {
        return Mathf.Max(0, healOnHit);
    }

    public float GetCritical()
    {
        return Mathf.Max(0f, criticalChance);
    }

    public float GetSpeed(float fallback)
    {
        if (moveSpeed > 0f)
        {
            return Mathf.Max(0f, moveSpeed);
        }

        return Mathf.Max(0f, fallback);
    }
}
