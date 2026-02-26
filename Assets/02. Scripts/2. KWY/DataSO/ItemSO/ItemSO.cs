using UnityEngine;
public enum Grade
{
    Normal,
    Epic,
    Legend
}
[CreateAssetMenu(fileName = "ItemData", menuName = "Data/ItemData")]

public class ItemSO : ScriptableObject
{
    public string itemName;
    public Sprite sprite;
    public int price;
    public Grade grade;
    [TextArea] public string description;
}
