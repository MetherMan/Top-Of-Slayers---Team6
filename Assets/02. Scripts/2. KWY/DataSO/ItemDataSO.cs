using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/ItemDataBase")]

public class ItemDataSO : ScriptableObject
{
    public List<ItemSO> items = new List<ItemSO>();
}
