using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/GachaDataBase")]

public class GachaDataSO : ScriptableObject
{
    public List<ItemSO> items = new List<ItemSO>();
}
