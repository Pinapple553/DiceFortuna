using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Item/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;
    public bool playerSelectDice = true;
    public ItemEffect effect;
    public ItemTier[] tiers;
}
[System.Serializable]
public class ItemTier //0 = base teir
{
    public string tierDescription;
    public int cost;
    public int uses;
    public int diceTargets;
}