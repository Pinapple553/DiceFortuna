using UnityEngine;

[CreateAssetMenu(fileName = "NewDice", menuName = "Dice/Dice Data")]
public class DiceData : ScriptableObject
{
    public string diceName;
    public DiceSide[] sides;
    public string description;
}