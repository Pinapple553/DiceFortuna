using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Item/Item Data")]
public class ItemData : ScriptableObject
{
	public string itemName;
	public Sprite icon;
	[TextArea] public string description;
	public int cost;             
	public ItemEffect effect;  
}