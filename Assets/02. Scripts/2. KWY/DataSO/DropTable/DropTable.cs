using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/DropTable")]
public class DropTable : ScriptableObject
{
    public List<DropItem> items;
}
