using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/GachaDataBase")]

public class GachaDataSO : ScriptableObject
{
    [Header("Grade Probability (Total 100)")]
    [Range(0, 100)] public float legendChance = 5f;
    [Range(0, 100)] public float epicChance = 15f;
    [Range(0, 100)] public float normalChance = 80f;

    [Header("Items By Grade")]
    public List<ItemSO> legendItems = new List<ItemSO>();
    public List<ItemSO> epicItems = new List<ItemSO>();
    public List<ItemSO> normalItems = new List<ItemSO>();
}
