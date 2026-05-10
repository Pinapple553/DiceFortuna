using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Item/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Effect Effect;

}