using UnityEngine;

[CreateAssetMenu(fileName = "AttackSpec_", menuName = "Config/AttackSpec")]
public class AttackSpecSO : ScriptableObject
{
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
}
