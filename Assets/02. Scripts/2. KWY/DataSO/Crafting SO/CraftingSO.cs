using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/CraftRecipe")]
public class CraftingSO : ScriptableObject
{
    public ItemSO ResultItem;

    public List<ItemSO> materials = new List<ItemSO>();
    public List<int> materialCounts = new List<int>();
}
